using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class Column
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public string DataLength { get; set; }
    }
}
