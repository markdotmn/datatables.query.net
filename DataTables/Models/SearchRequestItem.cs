using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTables.Models
{
    public class SearchRequestItem
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }
}
