namespace Bedrock.Application.DataTransferObjects
{
    public class CreateSysDeptDto
    {

        /// <summary>
        /// 父级部门ID，用于构建部门树结构。
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 祖先部门ID列表，用于快速查询部门层级关系。
        /// </summary>
        public string Ancestors { get; set; } = string.Empty;

        /// <summary>
        /// 部门名称。
        /// </summary>
        public string DeptName { get; set; } = string.Empty;

        /// <summary>
        /// 显示顺序，用于控制部门的排序。
        /// </summary>
        public int OrderNum { get; set; }

        /// <summary>
        /// 部门负责人姓名。
        /// </summary>
        public string? Leader { get; set; }

        /// <summary>
        /// 联系电话。
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 邮箱地址。
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 部门状态（0: 正常, 1: 停用）。
        /// </summary>
        public string Status { get; set; } = string.Empty;

    }

}