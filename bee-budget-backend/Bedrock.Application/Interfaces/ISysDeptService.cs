using Bedrock.Application.DataTransferObjects;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 服务接口，定义了对资源的基本业务逻辑操作。
    /// </summary>
    public interface ISysDeptService
    {
        /// <summary>
        /// 创建新记录。
        /// </summary>
        /// <param name="createDto">用于创建新记录的数据传输对象。</param>
        /// <param name="createdById">创建者的唯一标识符。</param>
        /// <returns>返回新创建记录的唯一标识符。</returns>
        Task<Guid> CreateAsync(CreateSysDeptDto createDto, Guid createdById);

        /// <summary>
        /// 更新现有记录。
        /// </summary>
        /// <param name="updateDto">用于更新记录的数据传输对象。</param>
        /// <param name="updatedById">更新者的唯一标识符。</param>
        /// <returns>返回被更新记录的唯一标识符。</returns>
        Task<Guid> UpdateAsync(UpdateSysDeptDto updateDto, Guid updatedById);

        /// <summary>
        /// 删除指定记录（软删除）。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <param name="deletedById">删除者的唯一标识符。</param>
        /// <returns>返回被删除记录的唯一标识符。</returns>
        Task<Guid> DeleteAsync(Guid id, Guid deletedById);

        /// <summary>
        /// 根据唯一标识符获取单个记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>返回与 ID 匹配的数据传输对象。</returns>
        Task<SysDeptDto> GetAsync(Guid id);

        /// <summary>
        /// 获取所有记录。
        /// </summary>
        /// <returns>返回数据传输对象的集合。</returns>
        Task<IEnumerable<SysDeptDto>> GetAllAsync();

        /// <summary>
        /// 分页获取记录，并支持通过名称进行模糊搜索。
        /// </summary>
        /// <param name="pageNumber">当前页码（从 1 开始）。</param>
        /// <param name="pageSize">每页显示的记录数。</param>
        /// <param name="deptName">可选的 deptName，用于筛选特定应用的记录。</param>
        /// <returns>返回包含分页结果和分页信息的 PaginationResult。</returns>
        Task<PaginationResult<SysDeptDto>> GetPagedAsync(int pageNumber, int pageSize, string? deptName = null);

    }
}