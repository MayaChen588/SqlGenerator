using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class MysqlHelper
    {

        /// <summary>
        /// Generate create table sql
        /// </summary>
        /// <param name="table">table object</param>
        /// <returns></returns>
        public static string GenerateSql(Table table)
        {
            StringBuilder sql = new StringBuilder();
            bool haveUid = false;
            int columncount = 0;


            // create table 
            sql.AppendLine(String.Format("-- {0}", table.Name));
            sql.AppendLine(String.Format("create table `{0}`", table.Name));
            sql.AppendLine("(");

            foreach (var column in table.Columns)
            {
                columncount++;

                if (column.Name.ToLower() == "uid")
                {
                    haveUid = true;
                }

                sql.Append(String.Format("    {0,-31} {1, -15} {2}{3}{4}",
                    "`" + column.Name + "`",
                    (column.DataType.ToLower() == "char" || column.DataType.ToLower() == "varchar") ? column.DataType + "(" + column.DataLength + ")" : column.DataType,
                    column.Name.ToLower() == "uid" ? "not null" : "",
                    column.Name.ToLower() == "rowversion" ? "default current_timestamp on update current_timestamp" : "",
                    columncount < table.Columns.Count ? "," : ""));

                if (columncount == table.Columns.Count)
                {
                    if (haveUid)
                    {
                        sql.Append(",");
                    }
                }

                sql.Append(Environment.NewLine);
            }

            if (haveUid)
            {
                sql.AppendLine("    CONSTRAINT PRIMARY KEY (`Uid`)");
            }

            sql.AppendLine(") ENGINE=innoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;");

            sql.AppendLine("");


            // create index
            foreach (var index in table.Indexes)
            {
                sql.AppendLine("");
                sql.AppendLine(String.Format("create {0} index `{1}`",
                    index.IsUnique ? "unique" : "",
                    index.Name));

                sql.Append(String.Format("    on `{0}`", table.Name));
                sql.Append("(");

                columncount = 0;
                foreach (var column in index.Columns)
                {
                    columncount++;
                    sql.Append(String.Format("{0} `{1}`",
                        columncount > 1 ? "," : "",
                        column));
                }
                sql.Append(String.Format(");{0}", Environment.NewLine));
            }

            return sql.ToString();
        }




        /// <summary>
        /// Generate systemcode insert sql
        /// </summary>
        /// <param name="systemcode">systemcode object</param>
        /// <returns></returns>
        public static string GenerateSql(SystemCode systemcode)
        {
            StringBuilder sql = new StringBuilder();
            int itemcount = 0;


            sql.AppendLine(String.Format("---- {0}", systemcode.CodeKey));

            foreach (var codeitem in systemcode.CodeItems)
            {
                itemcount++;

                sql.AppendLine("insert into `TSystemCode`(`Uid`, `ItemKind`, `ItemCode`, `ItemValue`, `Description`, `Sort`, `ShowOptionItem`, `CodeType`, `CreateUserId`, `CreateTime`, `ModifyUserId`, `ModifyTime`)");
                sql.AppendLine(String.Format("  values('{0}', '{1}', '{2}', '{3}', '{4}', {5}, '{6}', '{7}', '{8}', {9}, '{10}', {11});",
                        Guid.NewGuid().ToString().ToLower(),
                        systemcode.CodeKey,
                        codeitem.Code,
                        codeitem.Value,
                        codeitem.Description,
                        codeitem.Sort,
                        "Y",
                        codeitem.CodeType, 
                        "000000",
                        "now()",
                        "000000",
                        "now()")
                    );
            }

            sql.AppendLine("");

            return sql.ToString();
        }    
    }
}
