using SqlSugar;

namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 系统字典分类实体类，用于对字典项进行分组管理，每个分类通过唯一编码（Code）标识。
    /// </summary>
    public class SysDictCategory
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 字典分类名称
        /// 示例："用户状态"、"性别"、"操作类型"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// 字典分类唯一编码（关键字段），用于关联字典项。
        /// 必须全局唯一，前端通过此编码请求对应字典数据。
        /// 示例："user_status"、"gender"、"operation_type"
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 状态（0正常 1停用）
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 备注。
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间（UTC）。
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 创建者ID，指向创建该记录的用户。
        /// </summary>
        public long? CreatedById { get; set; }

        /// <summary>
        /// 更新时间（UTC）。
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 更新者ID，指向最后一次更新该记录的用户。
        /// </summary>
        public long? UpdatedById { get; set; }

        /// <summary>
        /// 删除时间（UTC）。
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 删除者ID，指向删除该记录的用户。
        /// </summary>
        public long? DeletedById { get; set; }
    }
}