namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 强制指定用户下线时使用的数据传输对象。
    /// </summary>
    public class ForceLogoutDto
    {
        /// <summary>
        /// 用户 ID。
        /// </summary>
        public long UserId { get; set; }
    }
}