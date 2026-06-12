using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.DataTransferObjects
{
    public class CategoryTotalAmountDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
