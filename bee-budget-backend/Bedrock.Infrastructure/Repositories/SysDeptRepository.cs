using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 仓储实现类，用于管理实体的增删改查及分页操作。
    /// </summary>
    public class SysDeptRepository : ISysDeptRepository
    {
        private readonly ISqlSugarClient _db; // 数据库上下文实例

        /// <summary>
        /// 构造函数注入数据库上下文。
        /// </summary>
        /// <param name="db">SqlSugar 数据库上下文。</param>
        public SysDeptRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 创建新记录。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        public async Task CreateAsync(SysDept entity)
        {
            // 插入新记录
            await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        /// <summary>
        /// 更新现有记录。
        /// </summary>
        /// <param name="entity">包含更新信息的实体对象。</param>
        /// <exception cref="Exception">如果更新失败，则抛出异常。</exception>
        public async Task UpdateAsync(SysDept entity)
        {
            // 执行更新操作
            var updateResult = await _db.Updateable(entity).ExecuteCommandAsync();
            if (updateResult <= 0)
            {
                throw new Exception("Failed to update the entity."); // 更新失败时抛出异常
            }
        }

        /// <summary>
        /// 删除记录（软删除）。
        /// </summary>
        /// <param name="entity">要删除的实体对象。</param>
        /// <exception cref="Exception">如果更新失败，则抛出异常。</exception>
        public async Task DeleteAsync(SysDept entity)
        {
            var updateResult = await _db.Updateable(entity).ExecuteCommandAsync();
            if (updateResult <= 0)
            {
                throw new Exception("Failed to soft delete the entity."); // 更新失败时抛出异常
            }
        }

        /// <summary>
        /// 根据唯一标识符获取单个记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>返回与 ID 匹配的实体对象；如果记录不存在，则返回 null。</returns>
        public async Task<SysDept> GetAsync(Guid id)
        {
            var entity = await _db.Queryable<SysDept>()
                .Where(e => e.DeletedAt == null) // 查询未删除的记录
                .Where(e => e.Id == id)
                .FirstAsync();

            return entity; // 返回查询结果，可能为 null
        }

        /// <summary>
        /// 获取所有记录。
        /// </summary>
        /// <returns>返回未删除的记录列表，并按创建时间倒序排序。</returns>
        public async Task<IEnumerable<SysDept>> GetAllAsync()
        {
            // 查询未删除的记录，并按创建时间倒序排序
            var entities = await _db.Queryable<SysDept>()
                .Where(e => e.DeletedAt == null)
                .OrderBy(e => e.CreatedAt, OrderByType.Desc)
                .ToListAsync();

            return entities; // 返回查询结果
        }

        /// <summary>
        /// 分页获取记录，并支持通过 AppId 进行筛选。
        /// </summary>
        /// <param name="pageNumber">当前页码（从 1 开始）。</param>
        /// <param name="pageSize">每页显示的记录数。</param>
        /// <param name="appId">可选的 AppId，用于筛选特定应用的记录。</param>
        /// <returns>返回分页结果，包含数据和总记录数。</returns>
        public async Task<(IEnumerable<SysDept> Data, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? deptName = null)
        {
            // 查询未删除的记录
            var query = _db.Queryable<SysDept>().Where(e => e.DeletedAt == null);

            // 如果提供了名称，则按名称进行模糊搜索
            if (!string.IsNullOrWhiteSpace(deptName))
            {
                query = query.Where(e => e.DeptName == deptName);
            }

            // 获取总记录数
            var totalCount = await query.CountAsync();

            // 分页查询，并按创建时间倒序排序
            var pagedData = await query
                .OrderBy(e => e.CreatedAt, OrderByType.Desc) // 按创建时间倒序排序
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount); // 返回分页结果
        }


    }
}