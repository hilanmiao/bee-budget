using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;
using System.Linq.Expressions;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 交易仓储实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Id 保证全局唯一。
    /// </para>
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 构造函数注入 SqlSugar 数据库上下文。
        /// </summary>
        /// <param name="db">数据库客户端实例。</param>
        public TransactionRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 创建一条新的交易记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        public async Task<long> CreateAsync(Transaction entity)
        {
            var id = await _db.Insertable(entity).ExecuteReturnBigIdentityAsync();
            return id;
        }

        /// <summary>
        /// 更新一条交易记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 Id。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> UpdateAsync(Transaction entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 软删除指定的交易记录（标记 DeletedAt 和 DeletedById）。
        /// </summary>
        /// <param name="entity">要删除的实体，用于获取 Id 和审计信息。</param>
        /// <returns>返回受影响的记录数（1 或 0）。</returns>
        public async Task<int> DeleteAsync(Transaction entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量软删除交易记录，并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的操作人 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空，则返回 0。</returns>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<Transaction>()
                .SetColumns(it => new Transaction
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(it => idList.Contains(it.Id))
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据主键获取单条未删除的交易记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        public async Task<Transaction?> GetAsync(long id)
        {
            var entity = await _db.Queryable<Transaction>()
                .Where(it => it.Id == id && it.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据 ID 列表批量获取未删除的交易实体。
        /// </summary>
        /// <param name="ids">交易 ID 列表（可为 List、Array 等）。</param>
        /// <returns>返回匹配的实体列表（已执行查询，非延迟）。</returns>
        public async Task<List<Transaction>> GetByIdsAsync(IEnumerable<long> ids)
        {
            if (ids == null)
                return new List<Transaction>();

            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return new List<Transaction>();

            return await _db.Queryable<Transaction>()
                .Where(it => idList.Contains(it.Id))
                .Where(it => it.DeletedAt == null)
                .ToListAsync();
        }

        /// <summary>
        /// 根据账本Id，批量软删除交易记录，并记录操作人。
        /// </summary>
        /// <param name="ledgerId">要删除的记录 账本Id。</param>
        /// <param name="operatorId">执行删除操作的操作人 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；</returns>    
        public async Task<int> DeleteByLedgerIdAsync(long ledgerId, long operatorId)
        {
            var rowsAffected = await _db.Updateable<Transaction>()
                .SetColumns(it => new Transaction
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(it => it.LedgerId == ledgerId)
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据账本Id列表，批量软删除样例2记录，并记录操作人。
        /// </summary>
        /// <param name="ledgerIds">要删除的记录 账本Id 列表。</param>
        /// <param name="operatorId">执行删除操作的操作人 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 LedgerId 列表为空，则返回 0。</returns>        
        public async Task<int> DeleteByLedgerIdsAsync(IEnumerable<long> ledgerIds, long operatorId)
        {
            var ledgerIdList = ledgerIds as long[] ?? ledgerIds.ToArray();
            if (!ledgerIdList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<Transaction>()
                .SetColumns(it => new Transaction
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(it => ledgerIdList.Contains(it.LedgerId) && it.DeletedAt == null)
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据交易分类Id，批量软删除交易记录，并记录操作人。
        /// </summary>
        /// <param name="transactionCategoryId">要删除的记录 交易分类Id。</param>
        /// <param name="operatorId">执行删除操作的操作人 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；</returns>    
        public async Task<int> DeleteByTransactionCategoryIdAsync(long transactionCategoryId, long operatorId)
        {
            var rowsAffected = await _db.Updateable<Transaction>()
                .SetColumns(it => new Transaction
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(it => it.TransactionCategoryId == transactionCategoryId)
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据交易分类Id列表，批量软删除交易记录，并记录操作人。
        /// </summary>
        /// <param name="transactionCategoryIds">要删除的记录 交易分类Id 列表。</param>
        /// <param name="operatorId">执行删除操作的操作人 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 TransactionCategoryId 列表为空，则返回 0。</returns>    
        public async Task<int> DeleteByTransactionCategoryIdsAsync(IEnumerable<long> transactionCategoryIds, long operatorId)
        {
            var transactionCategoryIdList = transactionCategoryIds as long[] ?? transactionCategoryIds.ToArray();
            if (!transactionCategoryIdList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<Transaction>()
                .SetColumns(it => new Transaction
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(it => transactionCategoryIdList.Contains(it.TransactionCategoryId) && it.DeletedAt == null)
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        #region 关联表操作

        /// <summary>
        /// 根据主键获取单条未删除的交易记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        public async Task<TransactionSummary?> GetSummaryAsync(long id)
        {
            var entity = await _db.Queryable<Transaction, Ledger, TransactionCategory>((it, l, tc) => new object[]
               {
                    JoinType.Left, it.LedgerId == l.Id, // 连接其他表
                    JoinType.Left, it.TransactionCategoryId == tc.Id,
               })
               .Where(it => it.DeletedAt == null) // 查询未删除的记录
               .Where(it => it.Id == id)
               .Select((it, l, tc) => new TransactionSummary
               {
                   Id = it.Id,
                   LedgerId = l.Id,
                   LedgerName = l.Name,
                   TransactionCategoryId = tc.Id,
                   TransactionCategoryName = tc.Name,
                   TransactionCategoryIcon = tc.Icon,
                   Type = it.Type,
                   Amount = it.Amount,
                   Description = it.Description,
                   Date = it.Date,
                   Status = it.Status,
                   Remark = it.Remark,
                   CreatedById = it.CreatedById,
                   CreatedAt = it.CreatedAt,
                   UpdatedById = it.UpdatedById,
                   UpdatedAt = it.UpdatedAt,
                   DeletedById = it.DeletedById,
                   DeletedAt = it.DeletedAt
               }) // 映射到摘要
               .FirstAsync();

            return entity; // 返回查询结果，可能为 null
        }

        /// <summary>
        /// 查询未删除的交易记录，支持按名称和描述模糊搜索，用户Id、交易账本Id、交易分类Id、状态精确搜索，日期范围搜索。
        /// </summary>
        /// <param name="userId">用户Id。</param>
        /// <param name="ledgerId">交易账本Id，用于精确搜索（可选）。</param>
        /// <param name="transactionCategoryId">交易分类Id，用于精确搜索（可选）。</param>
        /// <param name="status">交易状态，用于模糊搜索（可选）。</param>
        /// <param name="startDate">交易开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">交易结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="description">交易描述，用于模糊搜索（可选）。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<TransactionSummary>> GetAllSummaryAsync(
            long userId,
            long? ledgerId = null,
            long? transactionCategoryId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? description = null)
        {
            var query = _db.Queryable<Transaction, Ledger, TransactionCategory>((it, l, tc) => new object[]
                {
                    JoinType.Left, it.LedgerId == l.Id, // 连接其他表
                    JoinType.Left, it.TransactionCategoryId == tc.Id,
                })
                .Where(it => it.DeletedAt == null); // 查询未删除的记录


            // 账本权限过滤
            if (ledgerId != null)
            {
                // 场景 A：指定了具体账本
                // 建议：即使指定了 ID，也最好验证一下该账本是否属于当前用户，防止越权访问
                query = query.Where((it, l, tc) => it.LedgerId == ledgerId && l.UserId == userId);
            }
            else
            {
                // 场景 B：未指定账本 -> 查询该用户下的所有账本
                // 这里直接对关联表 'l' (Ledger) 进行过滤
                query = query.Where((it, l, tc) => l.UserId == userId);
            }

            if (transactionCategoryId != null)
            {
                query = query.Where(it => it.TransactionCategoryId == transactionCategoryId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(it => it.Status == status);
            }

            if (startDate != null)
            {
                query = query.Where(it => it.Date >= startDate.Value);
            }

            if (endDate != null)
            {
                query = query.Where(it => it.Date < endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                query = query.Where(it => it.Description.Contains(description));
            }

            var entities = await query
                //.OrderBy(it => it.CreatedAt, OrderByType.Desc) // 按创建时间倒序排序
                .OrderBy(it => it.Date, OrderByType.Desc) // 按交易时间倒序排序
                .Select((it, l, tc) => new TransactionSummary
                {
                    Id = it.Id,
                    LedgerId = l.Id,
                    LedgerName = l.Name,
                    TransactionCategoryId = tc.Id,
                    TransactionCategoryName = tc.Name,
                    TransactionCategoryIcon = tc.Icon,
                    Type = it.Type,
                    Amount = it.Amount,
                    Description = it.Description,
                    Date = it.Date,
                    Status = it.Status,
                    Remark = it.Remark,
                    CreatedById = it.CreatedById,
                    CreatedAt = it.CreatedAt,
                    UpdatedById = it.UpdatedById,
                    UpdatedAt = it.UpdatedAt,
                    DeletedById = it.DeletedById,
                    DeletedAt = it.DeletedAt
                }) // 映射到摘要
                .ToListAsync();

            return entities; // 返回查询结果，可能为 null
        }

        /// <summary>
        /// 分页查询未删除的交易记录，支持按名称和描述模糊搜索，用户Id、交易账本Id、交易分类Id、状态精确搜索，日期范围搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="userId">用户Id。</param>
        /// <param name="ledgerId">交易账本Id，用于精确搜索（可选）。</param>
        /// <param name="transactionCategoryId">交易分类Id，用于精确搜索（可选）。</param>
        /// <param name="status">交易状态，用于模糊搜索（可选）。</param>
        /// <param name="startDate">交易开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">交易结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="description">交易描述，用于模糊搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        public async Task<(List<TransactionSummary> Data, int TotalCount)> GetPagedSummaryAsync(
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
            // 排序字段映射：前端字段名 → 数据库字段名
            orderByField = (orderByField ?? "date") switch
            {
                "id" => "id",
                "createdAt" => "created_at",
                "updatedAt" => "updated_at",
                "date" => "date",
                _ => "created_at"
            };
            orderByType = orderByType ?? "DESC";

            var query = _db.Queryable<Transaction, Ledger, TransactionCategory>((it, l, tc) => new object[]
            {
                JoinType.Left, it.LedgerId == l.Id, // 连接其他表
                JoinType.Left, it.TransactionCategoryId == tc.Id,
            })
            .Where(it => it.DeletedAt == null); // 查询未删除的记录

            // 账本权限过滤
            if (ledgerId != null)
            {
                // 场景 A：指定了具体账本
                // 建议：即使指定了 ID，也最好验证一下该账本是否属于当前用户，防止越权访问
                query = query.Where((it, l, tc) => it.LedgerId == ledgerId && l.UserId == userId);
            }
            else
            {
                // 场景 B：未指定账本 -> 查询该用户下的所有账本
                // 这里直接对关联表 'l' (Ledger) 进行过滤
                query = query.Where((it, l, tc) => l.UserId == userId);
            }

            if (transactionCategoryId != null)
            {
                query = query.Where(it => it.TransactionCategoryId == transactionCategoryId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(it => it.Status == status);
            }

            if (startDate != null)
            {
                query = query.Where(it => it.Date >= startDate.Value);
            }

            if (endDate != null)
            {
                query = query.Where(it => it.Date < endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                query = query.Where(it => it.Description.Contains(description));
            }

            // 获取总记录数
            var totalCount = await query.CountAsync();

            // 分页查询，并按创建时间倒序排序
            var pagedData = await query
                .OrderBy($"it.{orderByField} {orderByType}")
                .OrderBy(it => it.CreatedAt, OrderByType.Desc)
                .Select((it, l, tc) => new TransactionSummary
                {
                    Id = it.Id,
                    LedgerId = l.Id,
                    LedgerName = l.Name,
                    TransactionCategoryId = tc.Id,
                    TransactionCategoryName = tc.Name,
                    TransactionCategoryIcon = tc.Icon,
                    Type = it.Type,
                    Amount = it.Amount,
                    Description = it.Description,
                    Date = it.Date,
                    Status = it.Status,
                    Remark = it.Remark,
                    CreatedById = it.CreatedById,
                    CreatedAt = it.CreatedAt,
                    UpdatedById = it.UpdatedById,
                    UpdatedAt = it.UpdatedAt,
                    DeletedById = it.DeletedById,
                    DeletedAt = it.DeletedAt
                }) // 映射到摘要
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount); // 返回分页结果
        }

        #endregion

        /// <summary>
        /// 获取指定账本的实时数据快照（包含日/周/月/年收支统计）。
        /// <param name="ledgerId">账本Id</param>
        /// <returns>返回账本实时数据快照 Dto，即使未查询到数据也不会返回 null</returns>
        public async Task<LedgerSnapshotDto> GetLedgerSnapshotAsync(long ledgerId)
        {
            var now = DateTime.UtcNow; // 确保与数据库存储时区一致
            var todayStart = now.Date;

            // 计算本周开始时间 (以周一为起点)
            int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = todayStart.AddDays(-diff);

            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            // 使用条件聚合一次性查询所有统计数据
            var result = await _db.Queryable<Transaction>()
                .Where(it => it.DeletedAt == null)
                .Where(it => it.LedgerId == ledgerId)
                .Where(it => it.Status != "1") // 排除已作废的
                .Select(it => new LedgerSnapshotDto
                {
                    // --- 今日 ---
                    TodayIncome = SqlFunc.AggregateSum<decimal>(
                        SqlFunc.IIF(it.Date >= todayStart && it.Type == "收入", it.Amount, 0m)
                    ),
                    TodayExpense = SqlFunc.AggregateSum<decimal>(
                        SqlFunc.IIF(it.Date >= todayStart && it.Type == "支出", it.Amount, 0m)
                    ),

                    // --- 本周 ---
                    WeekIncome = SqlFunc.AggregateSum<decimal>(
                        SqlFunc.IIF(it.Date >= weekStart && it.Type == "收入", it.Amount, 0m)
                    ),
                    WeekExpense = SqlFunc.AggregateSum<decimal>(
                        SqlFunc.IIF(it.Date >= weekStart && it.Type == "支出", it.Amount, 0m)
                    ),

                    // --- 本月 ---
                    MonthIncome = SqlFunc.AggregateSum<decimal>(
                        SqlFunc.IIF(it.Date >= monthStart && it.Type == "收入", it.Amount, 0m)
                    ),
                    MonthExpense = SqlFunc.AggregateSum<decimal>(
                        SqlFunc.IIF(it.Date >= monthStart && it.Type == "支出", it.Amount, 0m)
                    ),

                    // --- 本年 ---
                    YearIncome = SqlFunc.AggregateSum<decimal>(
                        SqlFunc.IIF(it.Date >= yearStart && it.Type == "收入", it.Amount, 0m)
                    ),
                    YearExpense = SqlFunc.AggregateSum<decimal>(
                        SqlFunc.IIF(it.Date >= yearStart && it.Type == "支出", it.Amount, 0m)
                    )
                })
                .FirstAsync();

            // 统计类的接口一般不返回 null
            return result ?? new LedgerSnapshotDto();
        }

        /// <summary>
        /// 获取本月支出分类排名 TopN
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔支出交易的分类 Dto 列表。若无数据返回空列表。</returns>
        public async Task<List<TransactionCategoryStatDto>> GetMonthlyExpenseTransactionCategoryTopNAsync(long ledgerId, int top)
        {
            if (ledgerId <= 0) throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));
            if (top <= 0) throw new ArgumentException("无效的排名数量。", nameof(top));

            var timeRange = GetMonthTimeRange(); // 辅助方法获取本月起止

            var list = await _db.Queryable<Transaction, TransactionCategory>((it, tc) => new object[]
                {
                    JoinType.Left, it.TransactionCategoryId == tc.Id,
                })
                .Where(it => it.DeletedAt == null)
                .Where(it => it.Status != "1") // 排除已作废的
                .Where(it => it.LedgerId == ledgerId)
                .Where(it => it.Type == "支出")
                .Where(it => it.Date >= timeRange.Start && it.Date < timeRange.End)
                .GroupBy(it => it.TransactionCategoryId)
                .OrderByDescending((it, tc) => SqlFunc.AggregateSum<decimal>(it.Amount))
                .Select((it, tc) => new TransactionCategoryStatDto
                {
                    TransactionCategoryId = it.TransactionCategoryId,
                    TransactionCategoryName = tc.Name,
                    TransactionCategoryIcon = tc.Icon,
                    TransactionTotalAmount = SqlFunc.AggregateSum<decimal>(it.Amount),
                    TransactionCount = SqlFunc.AggregateCount(it.Id)
                })
                //.OrderByDescending(it => it.TransactionTotalAmount) // 放在这里就不对了，因为 TransactionTotalAmount 是在 Select 里计算的，无法在 GroupBy 后直接使用
                .Take(top)
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// 获取本月收入分类排名 TopN
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔收入交易的 Dto 列表。若无数据返回空列表。</returns>
        public async Task<List<TransactionCategoryStatDto>> GetMonthlyIncomeTransactionCategoryTopNAsync(long ledgerId, int top)
        {
            if (ledgerId <= 0) throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));
            if (top <= 0) throw new ArgumentException("无效的排名数量。", nameof(top));

            var timeRange = GetMonthTimeRange();

            var list = await _db.Queryable<Transaction, TransactionCategory>((it, tc) => new object[]
                {
                    JoinType.Left, it.TransactionCategoryId == tc.Id,
                })
                .Where(it => it.DeletedAt == null)
                .Where(it => it.Status != "1") // 排除已作废的
                .Where(it => it.LedgerId == ledgerId)
                .Where(it => it.Type == "收入")
                .Where(it => it.Date >= timeRange.Start && it.Date < timeRange.End)
                .GroupBy(it => it.TransactionCategoryId)
                .OrderByDescending((it, tc) => SqlFunc.AggregateSum<decimal>(it.Amount))
                .Select((it, tc) => new TransactionCategoryStatDto
                {
                    TransactionCategoryId = it.TransactionCategoryId,
                    TransactionCategoryName = tc.Name,
                    TransactionCategoryIcon = tc.Icon,
                    TransactionTotalAmount = SqlFunc.AggregateSum<decimal>(it.Amount),
                    TransactionCount = SqlFunc.AggregateCount(it.Id)
                })
                //.OrderByDescending(it => it.TransactionTotalAmount) // 放在这里就不对了，因为 TransactionTotalAmount 是在 Select 里计算的，无法在 GroupBy 后直接使用
                .Take(top)
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// 获取本月支出分类
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <returns>按金额降序排列的本月所有交易的分类 Dto 列表。若无数据返回空列表。</returns>
        public async Task<List<TransactionCategoryStatDto>> GetMonthlyExpenseTransactionCategoryAsync(long ledgerId)
        {
            if (ledgerId <= 0) throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));

            var timeRange = GetMonthTimeRange();

            var list = await _db.Queryable<Transaction, TransactionCategory>((it, tc) => new object[]
                {
                    JoinType.Left, it.TransactionCategoryId == tc.Id,
                })
                .Where(it => it.DeletedAt == null)
                .Where(it => it.Status != "1") // 排除已作废的
                .Where(it => it.LedgerId == ledgerId)
                .Where(it => it.Type == "支出")
                .Where(it => it.Date >= timeRange.Start && it.Date < timeRange.End)
                .GroupBy(it => it.TransactionCategoryId)
                .OrderByDescending((it, tc) => SqlFunc.AggregateSum<decimal>(it.Amount))
                .Select((it, tc) => new TransactionCategoryStatDto
                {
                    TransactionCategoryId = it.TransactionCategoryId,
                    TransactionCategoryName = tc.Name,
                    TransactionCategoryIcon = tc.Icon,
                    TransactionTotalAmount = SqlFunc.AggregateSum<decimal>(it.Amount),
                    TransactionCount = SqlFunc.AggregateCount(it.Id)
                })
                //.OrderByDescending(it => it.TransactionTotalAmount) // 放在这里就不对了，因为 TransactionTotalAmount 是在 Select 里计算的，无法在 GroupBy 后直接使用
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// 获取本月单笔金额最大的前 N 笔支出交易记录。
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔交易 Dto 列表。若无数据返回空列表。</returns>
        public async Task<List<TransactionSummary>> GetMonthlyExpenseTransactionTopNAsync(long ledgerId, int top)
        {
            if (ledgerId <= 0)
                throw new ArgumentException("无效的账本 ID。", nameof(ledgerId));
            if (top <= 0) throw new ArgumentException("无效的排名数量。", nameof(top));

            var timeRange = GetMonthTimeRange();

            var list = await _db.Queryable<Transaction, TransactionCategory>((it, tc) => new object[]
                {
                    JoinType.Left, it.TransactionCategoryId == tc.Id,
                })
                .Where(it => it.DeletedAt == null)
                .Where(it => it.Status != "1") // 排除已作废的
                .Where(it => it.LedgerId == ledgerId)
                .Where(it => it.Type == "支出")
                .Where(it => it.Date >= timeRange.Start && it.Date < timeRange.End)
                .OrderByDescending(it => it.Amount)
                .Select((it, tc) => new TransactionSummary
                {
                    Id = it.Id,
                    TransactionCategoryId = tc.Id,
                    TransactionCategoryName = tc.Name,
                    TransactionCategoryIcon = tc.Icon,
                    Type = it.Type,
                    Amount = it.Amount,
                    Description = it.Description,
                    Date = it.Date,
                    Status = it.Status,
                    Remark = it.Remark,
                }) // 映射到摘要
                .Take(top)
                .ToListAsync();

            return list;
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
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            var today = DateTime.Now;

            // 1. 从数据库查询原始数据
            // 注意：我们只查今年有数据的月份，没数据的月份数据库不会返回行
            var rawList = await _db.Queryable<Transaction>()
                .Where(it => it.DeletedAt == null)
                .Where(it => it.Status != "1") // 排除已作废的
                .Where(it => it.LedgerId == ledgerId)
                .Where(it => it.Date.Year == currentYear) // 限制在今年
                                                          // 按月份数字分组 (1, 2, ... 12)
                                                          // SqlSugar 会将 it.Date.Month 映射为 SQL 的 MONTH(date)
                .GroupBy(it => it.Date.Month)
                .OrderBy(it => it.Date.Month)
                .Select(it => new TransactionMonthlyStatDto
                {
                    MonthIndex = it.Date.Month,

                    // 条件聚合
                    RawIncomeAmount = SqlFunc.AggregateSum(
                        it.Type == "收入" ? it.Amount : (decimal?)null
                    ),
                    RawExpenseAmount = SqlFunc.AggregateSum(
                        it.Type == "支出" ? it.Amount : (decimal?)null
                    )
                })
                .ToListAsync();

            // 2. C# 层进行数据补全和逻辑处理
            var finalList = new List<TransactionMonthlyStatDto>();

            for (int i = 1; i <= 12; i++)
            {
                var monthLabel = $"{i}月";

                // 判断这个月份是否"已经开始" (即当前时间是否已经进入了该月)
                // 逻辑：如果 当前月份 > i，说明该月已完全过去。
                //      如果 当前月份 == i，说明该月正在进行中 (也算"已开始")。
                //      如果 当前月份 < i，说明该月是未来 (未开始)。
                bool isMonthStarted = (currentMonth >= i);

                // 查找数据库中是否有该月的数据
                var dbRecord = rawList.FirstOrDefault(x => x.MonthIndex == i);

                var dto = new TransactionMonthlyStatDto
                {
                    MonthIndex = i,
                    MonthLabel = monthLabel,
                };

                if (dbRecord != null)
                {
                    dto.RawIncomeAmount = dbRecord.RawIncomeAmount;
                    dto.RawExpenseAmount = dbRecord.RawExpenseAmount;
                    // 情况 A: 数据库里有数据 (说明该月发生过交易)
                    // 无论是否未来 (理论上未来不会有数据，除非手动插入未来日期)，直接取真实值
                    dto.IncomeAmount = dbRecord.RawIncomeAmount;
                    dto.ExpenseAmount = dbRecord.RawExpenseAmount;
                }
                else
                {
                    // 情况 B: 数据库里没数据 (该月无交易记录)
                    if (isMonthStarted)
                    {
                        // 月份已过去或进行中，但没数据 -> 补 0 (代表记账了但金额为0，或者没记账视为0)
                        // 这样图表上会显示为 0 的点，而不是断点
                        dto.IncomeAmount = 0;
                        dto.ExpenseAmount = 0;
                    }
                    else
                    {
                        // 月份还没开始 (未来) -> 保持 null
                        // 前端图表库 (如 ECharts) 遇到 null 通常会断开线条或不绘制该点，符合需求
                        dto.IncomeAmount = null;
                        dto.ExpenseAmount = null;
                    }
                }

                finalList.Add(dto);
            }

            return finalList;
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
            // 使用条件聚合一次性查询收入和支出
            // 注意：数据库未匹配到行时，SUM 返回 null，C# 中对应 decimal?
            var result = await _db.Queryable<Transaction>()
                .Where(it => it.DeletedAt == null)
                .Where(it => it.Status != "1") // 排除已作废的
                .Where(it => it.LedgerId == ledgerId)
                .Where(it => it.Date >= startDate && it.Date <= endDate) // 时间范围筛选
                .Select(it => new TransactionTotalStatDto
                {
                    // 如果是"收入"，则累加金额，否则忽略
                    TotalIncome = SqlFunc.AggregateSum<decimal>(
                        SqlFunc.IIF(it.Type == "收入", it.Amount, 0m)
                    ),
                    // 如果是"支出"，则累加金额，否则忽略
                    TotalExpense = SqlFunc.AggregateSum(
                        SqlFunc.IIF(it.Type == "支出", it.Amount, 0m)
                    )
                })
                .FirstAsync(); // 使用 FirstAsync 获取单行聚合结果

            return result;
        }

        /// <summary>
        /// 获取指定账本最早的一笔交易。
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        public async Task<Transaction?> GetEarliestAsync(long ledgerId)
        {
            var entity = await _db.Queryable<Transaction>()
                .Where(it => it.DeletedAt == null)
                .Where(it => it.LedgerId == ledgerId)
                .OrderBy(it => it.Date, OrderByType.Asc)
                .FirstAsync();

            return entity;
        }

        // 辅助方法：获取本月时间范围
        private (DateTime Start, DateTime End) GetMonthTimeRange()
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1);
            var end = start.AddMonths(1);
            return (start, end);
        }

    }
}