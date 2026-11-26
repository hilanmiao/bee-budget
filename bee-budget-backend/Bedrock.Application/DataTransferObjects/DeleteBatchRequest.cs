namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 批量删除请求的数据传输对象，包含待删除记录的 ID 列表。
    /// </summary>
    public class DeleteBatchRequest
    {
        /// <summary>
        /// 待删除记录的 ID 列表。
        /// </summary>
        public List<long> Ids { get; set; } = new();
    }
}