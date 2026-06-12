namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 泛型分页结果类，用于封装分页查询的结果。
    /// </summary>
    /// <typeparam name="T">数据项的类型。</typeparam>
    public class PaginationResult<T>
    {
        /// <summary>
        /// 当前页面的数据项集合。
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// 总页数，基于指定的每页大小和总数据项数量计算得出。
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 数据库或数据源中的总数据项数量。
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// 当前请求的页码（从1开始）。
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每页显示的数据项数量。
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 初始化一个新的实例。
        /// </summary>
        /// <param name="items">当前页面的数据项集合。</param>
        /// <param name="totalPages">总页数。</param>
        /// <param name="totalItems">数据库或数据源中的总数据项数量。</param>
        /// <param name="currentPage">当前请求的页码。</param>
        /// <param name="pageSize">每页显示的数据项数量。</param>
        public PaginationResult(IEnumerable<T> items, int totalPages, int totalItems, int currentPage, int pageSize)
        {
            Items = items;
            TotalPages = totalPages;
            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
        }
    }
}