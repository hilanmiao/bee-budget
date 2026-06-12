namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 密码哈希器接口，定义了密码的哈希化和验证操作。
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// 对明文密码进行哈希处理。
        /// </summary>
        /// <param name="password">需要哈希化的明文密码。</param>
        /// <returns>返回哈希化后的密码字符串。</returns>
        string HashPassword(string password);

        /// <summary>
        /// 验证提供的密码是否与哈希密码匹配。
        /// </summary>
        /// <param name="hashedPassword">已哈希化的密码。</param>
        /// <param name="providedPassword">用户提供的明文密码。</param>
        /// <returns>如果匹配则返回 true，否则返回 false。</returns>
        bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    }
}