using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class Table
    {
        private List<Column> _Columns = null;
        private List<Index> _Indexes = null;


        public Table()
        {
            _Columns = new List<Column>();
            _Indexes = new List<Index>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string PrimaryKey { get; set; }


        public List<Column> Columns
        {
            get
            {
                return _Columns;
            }
        }

        public List<Index> Indexes
        {
            get
            {
                return _Indexes;
            }
        }
    }
}
