using Bedrock.Core.Entities;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 仓储接口，定义了对实体的基本数据操作。
    /// </summary>
    public interface ISysDeptRepository
    {
        /// <summary>
        /// 创建新记录。
        /// </summary>
        /// <param name="entity">要创建的实体对象。</param>
        Task CreateAsync(SysDept entity);

        /// <summary>
        /// 更新现有记录。
        /// </summary>
        /// <param name="entity">包含更新信息的实体对象。</param>
        Task UpdateAsync(SysDept entity);

        /// <summary>
        /// 删除指定记录。
        /// </summary>
        /// <param name="entity">要删除的实体对象。</param>
        Task DeleteAsync(SysDept entity);

        /// <summary>
        /// 根据唯一标识符获取单个记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>返回与 ID 匹配的实体对象。</returns>
        Task<SysDept> GetAsync(Guid id);

        /// <summary>
        /// 获取所有记录。
        /// </summary>
        /// <returns>返回实体对象的集合。</returns>
        Task<IEnumerable<SysDept>> GetAllAsync();

        /// <summary>
        /// 分页获取记录，并支持通过名称进行模糊搜索。
        /// </summary>
        /// <param name="pageNumber">当前页码（从 1 开始）。</param>
        /// <param name="pageSize">每页显示的记录数。</param>
        /// <param name="deptName">可选的 deptName，用于筛选特定应用的记录。</param>
        /// <returns>返回分页结果，包含数据和总记录数。</returns>
        Task<(IEnumerable<SysDept> Data, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? deptName = null);

    }
}