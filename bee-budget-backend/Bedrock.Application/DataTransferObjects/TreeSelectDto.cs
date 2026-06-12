namespace Bedrock.Application.DataTransferObjects
{
    public class TreeSelectDto
    {
        /// <summary>
        /// 节点Id。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 节点名称。
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 子节点。
        /// </summary>
        public List<TreeSelectDto>? Children { get; set; }

        public TreeSelectDto(SysMenuDto dto)
        { 
            Id = dto.Id;
            Label = dto.Name;
            Children = dto.Children?.Select(x => new TreeSelectDto(x)).ToList();
        }
    }

}