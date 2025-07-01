namespace KnowledgeBase.API.Models.Configurations
{
    /// <summary>
    /// JWT配置设置
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// JWT密钥
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// 发行者
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// 受众
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// 访问令牌过期时间（分钟）
        /// </summary>
        public int ExpiryMinutes { get; set; } = 60;

        /// <summary>
        /// 刷新令牌过期时间（天）
        /// </summary>
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }
}