using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 服务层实现，通过调用仓储层完成相关的业务逻辑。
    /// </summary>
    public class SysDeptService : ISysDeptService
    {
        private readonly ISqlSugarClient _db; // 数据库上下文实例
        private readonly ISysDeptRepository _SysDeptRepository; // 仓储接口

        /// <summary>
        /// 构造函数注入依赖项。
        /// </summary>
        /// <param name="db">数据库上下文。</param>
        /// <param name="appRepository">仓储接口。</param>
        public SysDeptService(ISqlSugarClient db, ISysDeptRepository SysDeptRepository)
        {
            _db = db;
            _SysDeptRepository = SysDeptRepository;
        }


        /// <summary>
        /// 创建新记录。
        /// </summary>
        /// <param name="createDto">用于创建新记录的数据传输对象。</param>
        /// <param name="createdById">创建者的唯一标识符。</param>
        /// <returns>返回新创建记录的唯一标识符。</returns>
        public async Task<Guid> CreateAsync(CreateSysDeptDto createDto, Guid createdById)
        {
            // 检查版本号


            var entity = new SysDept
            {
                Id = Guid.NewGuid(),
                DeptName = createDto.DeptName,
                ParentId = createDto.ParentId,
                Ancestors = createDto.Ancestors,
                Leader = createDto.Leader,
                Phone = createDto.Phone,
                Email = createDto.Email,
                Status = createDto.Status,
                OrderNum = createDto.OrderNum,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow
            };

            await _SysDeptRepository.CreateAsync(entity);

            return entity.Id;
        }

        /// <summary>
        /// 更新现有记录。
        /// </summary>
        /// <param name="updateDto">用于更新记录的数据传输对象。</param>
        /// <param name="updatedById">更新者的唯一标识符。</param>
        /// <returns>返回被更新记录的唯一标识符。</returns>
        public async Task<Guid> UpdateAsync(UpdateSysDeptDto updateDto, Guid updatedById)
        {
            var entity = await _SysDeptRepository.GetAsync(updateDto.Id);
            if (entity == null)
            {
                throw new InvalidOperationException("实体不存在");
            }

            entity.DeptName = updateDto.DeptName;
            entity.ParentId = updateDto.ParentId;
            entity.Ancestors = updateDto.Ancestors;
            entity.Leader = updateDto.Leader;
            entity.Phone = updateDto.Phone;
            entity.Email = updateDto.Email;
            entity.Status = updateDto.Status;
            entity.OrderNum = updateDto.OrderNum;

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedById = updatedById;

            await _SysDeptRepository.UpdateAsync(entity);

            return entity.Id;
        }

        /// <summary>
        /// 删除指定记录（软删除）。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <param name="deletedById">删除者的唯一标识符。</param>
        /// <returns>返回被删除记录的唯一标识符。</returns>
        public async Task<Guid> DeleteAsync(Guid id, Guid deletedById)
        {
            var entity = await _SysDeptRepository.GetAsync(id);
            if (entity == null)
            {
                throw new InvalidOperationException("实体不存在");
            }

            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedById = deletedById;

            await _SysDeptRepository.DeleteAsync(entity);

            return entity.Id;
        }

        /// <summary>
        /// 根据唯一标识符获取单个记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>返回与 ID 匹配的数据传输对象。</returns>
        public async Task<SysDeptDto> GetAsync(Guid id)
        {
            var entity = await _SysDeptRepository.GetAsync(id);
            if (entity == null)
            {
                throw new InvalidOperationException("实体不存在");
            }
            return MapToDto(entity);
        }

        /// <summary>
        /// 获取所有记录。
        /// </summary>
        /// <returns>返回包含所有记录的数据传输对象集合。</returns>
        public async Task<IEnumerable<SysDeptDto>> GetAllAsync()
        {
            var entities = await _SysDeptRepository.GetAllAsync();
            return entities.Select(entity => MapToDto(entity)).ToList();
        }

        /// <summary>
        /// 分页获取记录，并支持通过名称进行模糊搜索。
        /// </summary>
        /// <param name="pageNumber">当前页码（从 1 开始）。</param>
        /// <param name="pageSize">每页显示的记录数。</param>
        /// <param name="appId">可选的 AppId，用于筛选特定应用的记录。</param>
        /// <returns>返回分页结果，包含数据和分页信息。</returns>
        public async Task<PaginationResult<SysDeptDto>> GetPagedAsync(int pageNumber, int pageSize, string? appId = null)
        {
            var (data, totalCount) = await _SysDeptRepository.GetPagedAsync(pageNumber, pageSize, appId);
            var pagedData = data.Select(entity => MapToDto(entity)).ToList();

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<SysDeptDto>(
                items: pagedData,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 将实体对象映射为数据传输对象。
        /// </summary>
        /// <param name="entity">实体对象。</param>
        /// <returns>返回对应的数据传输对象。</returns>
        private SysDeptDto MapToDto(SysDept entity)
        {
            return new SysDeptDto
            {
                Id = entity.Id,
                DeptName = entity.DeptName,
                ParentId = entity.ParentId,
                Ancestors = entity.Ancestors,
                Leader = entity.Leader,
                Phone = entity.Phone,
                Email = entity.Email,
                Status = entity.Status,
                OrderNum = entity.OrderNum,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }


    }
}