using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Data;
using static Dapper.SqlMapper;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 交易服务实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Id 保证全局唯一。
    /// </para>
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly ILogger<TransactionService> _logger;
        private readonly ISqlSugarClient _db;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILedgerRepository _ledgerRepository;
        private readonly ITransactionCategoryRepository _transactionCategoryRepository;

        /// <summary>
        /// 初始化 <see cref="TransactionService"/> 类的新实例。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="db">SqlSugar 客户端，用于事务控制。</param>
        /// <param name="transactionRepository">交易仓储。</param>
        /// <param name="ledgerRepository">账本仓储。</param>
        /// <param name="transactionCategoryRepository">交易分类仓储。</param>
        public TransactionService(
            ILogger<TransactionService> logger,
            ISqlSugarClient db,
            ITransactionRepository transactionRepository,
            ILedgerRepository ledgerRepository,
            ITransactionCategoryRepository transactionCategoryRepository
            )
        {
            _logger = logger;
            _db = db;
            _transactionRepository = transactionRepository;
            _ledgerRepository = ledgerRepository;
            _transactionCategoryRepository = transactionCategoryRepository;
        }

        /// <summary>
        /// 创建新的交易。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Amount、Date 等字段。</param>
        /// <param name="operatorId">操作人 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当账本、交易分类已删除时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        public async Task<long> CreateAsync(CreateTransactionDto createDto, long operatorId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.Description))
                throw new ArgumentException("交易描述不能为空。", nameof(createDto.Description));
            if (createDto.Amount <= 0)
                throw new ArgumentException("交易金额必须大于0。", nameof(createDto.Amount));

            var ledgerEntity = await _ledgerRepository.GetAsync(createDto.LedgerId);
            if (ledgerEntity == null)
                throw new ArgumentException($"指定的账本不存在或已被删除。");

            var transactionCategoryEntity = await _transactionCategoryRepository.GetAsync(createDto.TransactionCategoryId);
            if (ledgerEntity == null)
                throw new ArgumentException($"指定的交易分类不存在或已被删除。");

            var entity = new Transaction
            {
                LedgerId = createDto.LedgerId,
                TransactionCategoryId = createDto.TransactionCategoryId,
                Type = createDto.Type,
                Amount = createDto.Amount,
                Description = createDto.Description.Trim(),
                Date = createDto.Date,
                Status = "0",
                Remark = createDto.Remark?.Trim(),
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow
            };

            var newId = await _transactionRepository.CreateAsync(entity);

            _logger.LogInformation("用户 {UserId} 创建交易 {TransactionDescription} (ID: {TransactionId}) 成功。", operatorId, createDto.Description, newId);

            return newId;
        }

        /// <summary>
        /// 更新指定的交易。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        public async Task<long> UpdateAsync(UpdateTransactionDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的交易 ID。", nameof(updateDto.Id));
            if (string.IsNullOrWhiteSpace(updateDto.Description))
                throw new ArgumentException("交易描述不能为空。", nameof(updateDto.Description));
            if (updateDto.Amount <= 0)
                throw new ArgumentException("交易金额必须大于0。", nameof(updateDto.Amount));

            var entity = await _transactionRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("指定的交易不存在或已被删除。", nameof(updateDto.Id));

            entity.TransactionCategoryId = updateDto.TransactionCategoryId;
            entity.Type = updateDto.Type;
            entity.Amount = updateDto.Amount;
            entity.Description = updateDto.Description;
            entity.Date = updateDto.Date;
            entity.Status = updateDto.Status?.Trim() ?? entity.Status;
            entity.Remark = updateDto.Remark?.Trim();
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _transactionRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新交易 {TransactionDescription} (ID: {TransactionId}) 失败，未影响任何数据库记录。", operatorId, entity.Description, entity.Id);
                throw new InvalidOperationException($"用户 {operatorId} 更新交易 {entity.Description} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新交易 {TransactionDescription} (ID: {TransactionId}) 成功。", operatorId, entity.Description, entity.Id);

            return entity.Id;
        }

        /// <summary>
        /// 软删除指定的交易。
        /// </summary>
        /// <param name="id">要删除的交易 ID。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        public async Task<long> DeleteAsync(long id, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的交易 ID。", nameof(id));

            var entity = await _transactionRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("交易不存在或已被删除。");

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var rowsAffected = 0;

                    // 删除关联数据
                    //await _transactionRepository.DeleteByDemo1IdAsync(entity.Demo1Id, operatorId);
                    //rowsAffected = await _transactionRepository.DeleteByDemo1IdAsync(entity.Demo1Id, operatorId);
                    //if (rowsAffected == 0)
                    //{
                    //    _logger.LogWarning("用户 {UserId} 删除交易 {TransactionName} (ID: {TransactionId}) 关联数据失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id);
                    //    throw new InvalidOperationException($"用户 {operatorId} 删除交易 {entity.Name} (ID: {entity.Id}) 关联数据失败，未影响任何数据库记录。");
                    //}

                    // 执行软删除
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedById = operatorId;
                    rowsAffected = await _transactionRepository.DeleteAsync(entity);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning("用户 {UserId} 删除交易 {TransactionDescription} (ID: {TransactionId}) 失败，未影响任何数据库记录。", operatorId, entity.Description, entity.Id);
                        throw new InvalidOperationException($"用户 {operatorId} 删除交易 {entity.Description} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 删除交易 {TransactionDescription} (ID: {TransactionId}) 失败，{ErrorMessage}。", operatorId, entity.Description, entity.Id, ex.Message);
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 删除交易 {TransactionDescription} (ID: {TransactionId}) 成功。", operatorId, entity.Description, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 批量软删除交易。
        /// </summary>
        /// <param name="ids">要删除的交易 ID 列表。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当部分 ID 不存在或已被删除时可能抛出。</exception>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            if (ids == null || !ids.Any())
                return 0;

            var idList = ids.Distinct().Where(id => id > 0).ToList();
            if (!idList.Any())
                return 0;

            // 查询待删除的交易实体
            var entities = await _transactionRepository.GetByIdsAsync(idList);
            if (entities.Count != idList.Count)
                throw new ArgumentException("部分交易不存在或已被删除。");

            //var demo1Ids = entities.Select(e => e.Demo1Id).ToList();

            // 使用 UseTranAsync 替代 BeginTranAsync
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 批量删除关联数据
                    //await _transactionRepository.DeleteByDemo1IdsAsync(demo1Ids, operatorId);

                    // 批量软删除交易
                    var deletedCount = await _transactionRepository.DeleteBatchAsync(idList, operatorId);

                    // 校验删除数量
                    if (deletedCount != idList.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量删除交易失败，期望删除 {Count} 条，但实际仅成功删除 {DeletedCount} 条。", operatorId, idList.Count, deletedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量删除交易失败，期望删除 {idList.Count} 条，但实际仅成功删除 {deletedCount} 条。");
                    }

                    // 事务成功，返回删除数量
                    return deletedCount;
                },
                // 可选：错误回调，用于记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量删除交易失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量删除交易成功，删除数量 {DeletedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 更新交易的状态。
        /// </summary>
        /// <param name="id">要更新的交易 ID。</param>
        /// <param name="status">状态</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        public async Task<long> ChangeStatusAsync(long id, string status, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的交易 ID。", nameof(id));

            if (string.IsNullOrWhiteSpace(status) || (status != "0" && status != "1"))
                throw new ArgumentException("无效的状态值，状态值必须为 '0'（已完成）或 '1'（已作废）");

            var entity = await _transactionRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("交易不存在或已被删除。");

            entity.Status = status;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _transactionRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新交易状态 {TransactionDescription} (ID: {TransactionId},Status:{Status}) 失败，未影响任何数据库记录。", operatorId, entity.Description, entity.Id, status);
                throw new InvalidOperationException($"用户 {operatorId} 更新交易状态 {entity.Description} (ID: {entity.Id},Status: {status}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新交易状态 {TransactionDescription} (ID: {TransactionId},Status:{Status}) 成功。", operatorId, entity.Description, entity.Id, status);

            return entity.Id;
        }

        /// <summary>
        /// 根据主键获取单条未删除的交易详情。
        /// </summary>
        /// <param name="id">交易唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<TransactionSummary?> GetSummaryAsync(long id)
        {
            var entity = await _transactionRepository.GetSummaryAsync(id);
            if (entity == null)
                return null;
            return entity;
        }

        /// <summary>
        /// 查询未删除的交易记录，支持按名称和描述模糊搜索，用户Id、交易账本Id、交易分类Id、状态精确搜索，日期范围搜索。
        /// </summary>
        /// <param name="ledgerId">交易账本Id，用于精确搜索（可选）。</param>
        /// <param name="userId">用户Id。</param>
        /// <param name="transactionCategoryId">交易分类Id，用于精确搜索（可选）。</param>
        /// <param name="status">交易状态，用于模糊搜索（可选）。</param>
        /// <param name="startDate">交易开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">交易结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="description">交易描述，用于模糊搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<TransactionSummary>> GetAllSummaryAsync(
            long userId,
            long? ledgerId = null,
            long? transactionCategoryId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? description = null)
        {
            var entities = await _transactionRepository.GetAllSummaryAsync(userId, ledgerId, transactionCategoryId, status, startDate, endDate, description);
            return entities;
        }

        /// <summary>
        /// 分页查询未删除的交易记录，支持按名称和描述模糊搜索，用户Id、交易账本Id、交易分类Id、状态精确搜索，日期范围搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="userId">用户Id。</param>
        /// <param name="ledgerId">交易账本Id。</param>
        /// <param name="transactionCategoryId">交易分类Id，用于精确搜索（可选）。</param>
        /// <param name="status">交易状态，用于模糊搜索（可选）。</param>
        /// <param name="startDate">交易开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">交易结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="description">交易描述，用于模糊搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<TransactionSummary>> GetPagedSummaryAsync(
            int pageNumber,
            int pageSize,
            long userId,
            long? ledgerId = null,
            long? transactionCategoryId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? description = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _transactionRepository.GetPagedSummaryAsync(pageNumber, pageSize, userId, ledgerId, transactionCategoryId, status, startDate, endDate, description, orderByField, orderByType);
            var pagedData = data.ToList();

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<TransactionSummary>(
                items: pagedData,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 将领域实体映射为应用层数据传输对象。
        /// </summary>
        /// <param name="entity">源实体对象。</param>
        /// <returns>映射后的 TransactionDto 实例。</returns>
        private static TransactionDto MapToDto(Transaction entity)
        {
            return new TransactionDto
            {
                Id = entity.Id,
                LedgerId = entity.LedgerId,
                TransactionCategoryId = entity.TransactionCategoryId,
                Type = entity.Type,
                Amount = entity.Amount,
                Description = entity.Description,
                Date = entity.Date,
                Status = entity.Status,
                Remark = entity.Remark,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        /// <summary>
        /// 获取指定账本的实时数据快照（包含日/周/月/年收支统计）。
        /// <param name="ledgerId">要查询的账本Id</param>
        /// <returns>返回账本实时数据快照 Dto，即使未查询到数据也不会返回 null</returns>
        public async Task<LedgerSnapshotDto> GetLedgerSnapshotAsync(long ledgerId)
        {
            if (ledgerId <= 0)
                throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));

            var result = await _transactionRepository.GetLedgerSnapshotAsync(ledgerId);

            return result;
        }

        /// <summary>
        /// 获取本月支出分类排名 TopN
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔支出交易的分类 Dto 列表。若无数据返回空列表。</returns>
        public async Task<List<TransactionCategoryStatDto>> GetMonthlyExpenseTransactionCategoryTopNAsync(long ledgerId, int top)
        {
            if (ledgerId <= 0)
                throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));
            if (top <= 0) throw new ArgumentException("无效的排名数量。", nameof(top));

            var result = await _transactionRepository.GetMonthlyExpenseTransactionCategoryTopNAsync(ledgerId, top);

            return result;
        }

        /// <summary>
        /// 获取本月收入分类排名 TopN
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔收入交易的 Dto 列表。若无数据返回空列表。</returns>
        public async Task<List<TransactionCategoryStatDto>> GetMonthlyIncomeTransactionCategoryTopNAsync(long ledgerId, int top)
        {
            if (ledgerId <= 0)
                throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));
            if (top <= 0) throw new ArgumentException("无效的排名数量。", nameof(top));

            var result = await _transactionRepository.GetMonthlyIncomeTransactionCategoryTopNAsync(ledgerId, top);

            return result;
        }

        /// <summary>
        /// 获取本月支出分类构成
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <returns>按金额降序排列的本月所有交易的分类 Dto 列表。若无数据返回空列表。</returns>
        public async Task<List<TransactionCategoryStatDto>> GetMonthlyExpenseTransactionCategoryAsync(long ledgerId)
        {
            if (ledgerId <= 0)
                throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));

            var result = await _transactionRepository.GetMonthlyExpenseTransactionCategoryAsync(ledgerId);

            return result;
        }

        /// <summary>
        /// 获取本月单笔金额最大的前 N 笔支出交易记录。
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <returns>按金额降序排列的前 N 笔交易 Dto 列表。若无数据返回空列表。</returns>
        public async Task<List<TransactionSummary>> GetMonthlyExpenseTransactionTopNAsync(long ledgerId, int top)
        {
            if (ledgerId <= 0)
                throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));
            if (top <= 0) throw new ArgumentException("无效的排名数量。", nameof(top));

            var result = await _transactionRepository.GetMonthlyExpenseTransactionTopNAsync(ledgerId, top);

            return result;
        }

        /// <summary>
        /// 统计指定账本本年度每月的收入与支出金额。
        /// </summary>
        /// <param name="ledgerId">账本 ID</param>
        /// <returns>
        /// 本年 1-12 月的收支统计 Dto 列表。
        /// 注意：未来未开始的月份返回 null，已过去但无交易的月份返回 0。
        /// </returns>
        public async Task<List<TransactionMonthlyStatDto>> GetYearlyStatsAsync(long ledgerId)
        {
            if (ledgerId <= 0)
                throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));

            var result = await _transactionRepository.GetYearlyStatsAsync(ledgerId);

            return result;
        }

        /// <summary>
        /// 统计指定时间范围内的收入和支出总金额。
        /// </summary>
        /// <param name="ledgerId">账本 ID</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>包含总收入和总支出的统计对象</returns>
        public async Task<TransactionTotalStatDto> GetRangeStatsAsync(long ledgerId, DateTime startDate, DateTime endDate)
        {
            if (ledgerId <= 0)
                throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));

            var result = await _transactionRepository.GetRangeStatsAsync(ledgerId, startDate, endDate);

            return result;
        }

        /// <summary>
        /// 获取指定账本最早的一笔交易。
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<TransactionDto?> GetEarliestAsync(long ledgerId)
        {
            var entity = await _transactionRepository.GetEarliestAsync(ledgerId);
            if (entity == null)
                return null;
            return MapToDto(entity);
        }


        // TODO: 要判断是否是我的账本

    }
}