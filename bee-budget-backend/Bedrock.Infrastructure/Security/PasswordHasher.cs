using Bedrock.Application.Interfaces;

namespace Bedrock.Infrastructure.Security
{
    /// <summary>
    /// 密码哈希处理类，实现IPasswordHasher接口
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// 对密码进行哈希加密
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <returns>加密后的哈希密码</returns>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password); // 使用BCrypt算法加密密码
        }

        /// <summary>
        /// 验证提供的密码与存储的哈希密码是否匹配
        /// </summary>
        /// <param name="hashedPassword">存储的哈希密码</param>
        /// <param name="providedPassword">用户提供的密码</param>
        /// <returns>验证结果（true表示匹配，false表示不匹配）</returns>
        public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword); // 使用BCrypt验证密码
        }
    }
}