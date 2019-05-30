using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class Index
    {
        List<string> _Columns = null;

        public Index()
        {
            _Columns = new List<string>();
        }

        public string Name { get; set; }
        public bool IsUnique { get; set; }
        public List<string> Columns
        {
            get
            {
                return _Columns;
            }
        }
    }
}
