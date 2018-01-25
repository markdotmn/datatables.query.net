using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTables.Models
{
    public class SearchRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public ColumnRequestItem[] Columns { get; set; }
        public OrderRequestItem[] Order { get; set; }
        public SearchRequestItem Search { get; set; }
    }
}
