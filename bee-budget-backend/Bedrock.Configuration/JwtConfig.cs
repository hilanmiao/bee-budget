namespace Bedrock.Configuration
{
    /// <summary>
    /// JWT（JSON Web Token）配置类，用于存储JWT相关配置信息
    /// </summary>
    public class JwtConfig
    {
        /// <summary>
        /// 用于签名和验证JWT的密钥
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// JWT的签发者（Issuer），标识Token的生成方
        /// </summary>
        public string? Issuer { get; set; }

        /// <summary>
        /// JWT的目标受众（Audience），标识Token的接收方
        /// </summary>
        public string? Audience { get; set; }

        /// <summary>
        /// JWT的过期时间（单位：分钟）
        /// </summary>
        public int ExpiryMinutes { get; set; }

        /// <summary>
        /// 刷新令牌的有效期（单位：天）
        /// </summary>
        public int RefreshTokenExpiryDays { get; set; }
    }
}