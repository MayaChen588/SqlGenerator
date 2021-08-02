using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class MSSqlHelper
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
            sql.AppendLine($"-- {table.Name}");
            sql.AppendLine($"create table dbo.[{table.Name}]");
            sql.AppendLine("(");

            foreach (var column in table.Columns)
            {
                columncount++;

                if (column.Name.ToLower() == "uid")
                {
                    haveUid = true;
                }

                string dataType = String.Empty;
                if (column.DataType.ToLower().EndsWith("char"))
                {
                    string dataLen = fixDataLength(column.DataLength);
                    dataType = column.DataType.ToLower() + "(" + dataLen + ")";
                }
                else if (column.DataType.ToLower().Contains("blob"))
                {
                    string dataLen = fixDataLength(column.DataLength);
                    dataType = "varbinary(" + dataLen + ")";
                }
                else if (column.DataType.ToLower().Contains("clob"))
                {
                    string dataLen = fixDataLength(column.DataLength);
                    dataType = "ntext";
                }
                //else if (column.DataType.ToLower() == "timestamp")
                //{
                //    dataType = "datetime";
                //}
                else if (column.DataType.ToLower() == "double")
                {
                    dataType = "decimal(19,2)";
                }
                else
                {
                    dataType = column.DataType.ToLower();
                }

                sql.Append(String.Format("    {0,-31} {1, -15}{2}{3}{4}",
                    "[" + column.Name + "]",
                    dataType,
                    column.DataType.ToLower().EndsWith("char") ? " collate Chinese_Taiwan_Stroke_CI_AS_WS" : "",
                    column.Mandatory ? " not null" : "",
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

            if (!String.IsNullOrWhiteSpace(table.PrimaryKey))
            {
                if (table.PrimaryKey.Contains("[nonclustered]"))
                {
                    sql.AppendLine($"    CONSTRAINT PK_{table.Name} PRIMARY KEY NONCLUSTERED ([{table.PrimaryKey.Replace("[nonclustered]", "").Trim()}])");
                }
                else
                {
                    sql.AppendLine($"    CONSTRAINT PK_{table.Name} PRIMARY KEY ({table.PrimaryKey.Trim()})");
                }
            }

            sql.AppendLine(");");

            sql.AppendLine("");


            // create index
            foreach (var index in table.Indexes)
            {
                sql.AppendLine("");
                sql.AppendLine(String.Format("create {0} index [{1}]",
                    index.IsUnique ? "unique" : "",
                    index.Name));

                sql.Append($"    on dbo.[{table.Name}]");
                sql.Append("(");

                columncount = 0;
                foreach (var column in index.Columns)
                {
                    columncount++;
                    sql.Append(String.Format("{0} [{1}]",
                        columncount > 1 ? "," : "",
                        column));
                }
                sql.Append($");{Environment.NewLine}");
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

                sql.AppendLine("insert into dbo.[TSystemCode]([Uid], [ItemKind], [ItemCode], [ItemValue], [Description], [Sort], [ShowOptionItem], [CodeType], [CreateUserId], [CreateTime], [ModifyUserId], [ModifyTime])");
                sql.AppendLine(String.Format("  values({0}, '{1}', '{2}', N'{3}', N'{4}', {5}, '{6}', '{7}', '{8}', {9}, '{10}', {11});",
                        //Guid.NewGuid().ToString().ToLower(),
                        "newid()",
                        systemcode.CodeKey,
                        codeitem.Code,
                        codeitem.Value.Replace("'", "''"),
                        codeitem.Description.Replace("'", "''"),
                        codeitem.Sort,
                        "Y",
                        codeitem.CodeType,
                        "000000",
                        "getdate()",
                        "000000",
                        "getdate()")
                    );
            }

            sql.AppendLine("");

            return sql.ToString();
        }



        private static string fixDataLength(string originalLen)
        {
            string dataLen = String.Empty;

            if (originalLen.ToLower() != "max")
            {
                if (Int32.TryParse(originalLen, out int len))
                {
                    if (len > 8000)
                    {
                        dataLen = "max";
                    }
                    else
                    {
                        dataLen = len.ToString();
                    }
                }
                else
                {
                    dataLen = "max";
                }
            }
            else
            {
                dataLen = "max";
            }

            return dataLen;
        }

    }
}
