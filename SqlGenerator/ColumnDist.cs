using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    /// <summary>
    /// DB2 UPDATE SYSSTAT.COLDIST資訊類別
    /// </summary>
    public class ColumnDist
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public string Type { get; set; }
        public int SeqNo { get; set; }
    }
}
