using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    /// <summary>
    /// DB2指令解析類別
    /// </summary>
    public class DB2Parser
    {
        /// <summary>
        /// 轉換為TSQL資料表建立語法
        /// </summary>
        /// <param name="sourceFile">來源檔案路徑檔名</param>
        /// <param name="distFile">轉換結果檔案路徑檔名</param>
        public static void TransferCreateTable(string sourceFile, string distFile)
        {
            if (String.IsNullOrWhiteSpace(sourceFile))
            {
                Console.WriteLine("沒有提供來源檔案路徑檔名!");
                return;
            }

            if (!File.Exists(sourceFile))
            {
                Console.WriteLine("來源檔案不存在!");
                return;
            }


            try
            {
                List<Table> tables = new List<Table>();
                List<ColumnDist> columnDists = new List<ColumnDist>();
                List<string> columnDistStrings = new List<string>();
                var sqlLines = File.ReadAllLines(sourceFile, Encoding.GetEncoding(950));

                int count = 0;
                int currLineIndex = 0;
                string currLineText = null;
                string tableName = null;
                Table sqlTable = null;
                StringBuilder columnDistStr = null;

                foreach (var line in sqlLines)
                {
                    currLineIndex++;
                    currLineText = line;
                    Console.WriteLine(String.Format("{0,8} {1}", currLineIndex, ""));

                    if (currLineText.Trim().StartsWith("COMMENT ON TABLE"))
                    {
                        //COMMENT ON TABLE "SFAUSR  "."SFA_COP_CIF_LN_INFO" IS '企金CIF_放款戶號相關資訊';

                        var commentparts = currLineText.Replace("COMMENT ON TABLE", "").Replace("  ", " ").Trim().Split(new string[] { " IS " }, StringSplitOptions.None);

                        if (commentparts.Length >= 2)
                        {
                            var tableparts = commentparts[0].Replace("\"", "").Replace(" ", "").Split('.');

                            if (tableparts.Length >= 2)
                            {
                                var tmpTable = tables.Where(x => x.Name == tableparts[1]).FirstOrDefault();

                                if (tmpTable != null)
                                {
                                    tmpTable.Description = commentparts[1].Replace("'", "").Trim();
                                }
                            }
                        }

                        continue;
                    }

                    if (currLineText.Trim().StartsWith("COMMENT ON COLUMN"))
                    {
                        //COMMENT ON COLUMN "SFAUSR  "."SFA_COP_CIF_LN_INFO"."ACCOUNT_BALANCE" IS '授信餘額';

                        var commentparts = currLineText.Replace("COMMENT ON COLUMN", "").Replace("  ", " ").Trim().Split(new string[] { " IS " }, StringSplitOptions.None);

                        if (commentparts.Length >= 2)
                        {
                            var columnparts = commentparts[0].Replace("\"", "").Replace(" ", "").Split('.');

                            if (columnparts.Length >= 3)
                            {
                                var tmpTable = tables.Where(x => x.Name == columnparts[1]).FirstOrDefault();

                                if (tmpTable != null)
                                {
                                    var tmpColumn = tmpTable.Columns.Where(x => x.Name == columnparts[2]).FirstOrDefault();

                                    if (tmpColumn != null)
                                    {
                                        tmpColumn.Description = commentparts[1].Replace("'", "").Trim();
                                    }
                                }
                            }
                        }

                        continue;
                    }


                    //if (currLineText.Trim().StartsWith("UPDATE SYSSTAT.COLDIST"))
                    //{
                    //    //UPDATE SYSSTAT.COLDIST
                    //    //SET COLVALUE = '1020',
                    //    //    VALCOUNT = 43286
                    //    //WHERE COLNAME = 'EX_UNIT' AND TABNAME = 'SFA_RPT_EX101'
                    //    //      AND TABSCHEMA = 'SFAUSR  '
                    //    //      AND TYPE = 'F'
                    //    //      AND SEQNO = 1;

                    //    if (columnDistStr != null)
                    //    {
                    //        columnDistStrings.Add(columnDistStr.ToString());
                    //        columnDistStr = null;
                    //    }

                    //    columnDistStr = new StringBuilder();
                    //    continue;
                    //}

                    //if (currLineText.Trim().StartsWith("SET COLVALUE"))
                    //{
                    //    if (columnDistStr == null)
                    //    {
                    //        columnDistStr = new StringBuilder();
                    //    }

                    //    columnDistStr.Append($" {currLineText}");
                    //    continue;
                    //}

                    //if (currLineText.Trim().StartsWith("WHERE COLNAME") ||
                    //    currLineText.Trim().StartsWith("AND TYPE =") ||
                    //    currLineText.Trim().StartsWith("AND SEQNO ="))
                    //{
                    //    if (columnDistStr != null)
                    //    {
                    //        columnDistStr.Append($" {currLineText}");

                    //        if (currLineText.Trim().EndsWith(";"))
                    //        {
                    //            columnDists.Add(GenerateColumnDist(columnDistStr.ToString()));
                    //            columnDistStr = null;
                    //        }
                    //    }

                    //    continue;
                    //}




                    if (currLineText.Trim().StartsWith("CREATE TABLE"))
                    {
                        //CREATE TABLE "SFAUSR  "."SFA_USER_ROLE_SET.ixf"(
                        //          "EMP_NO" VARCHAR(20) NOT NULL,
                        //          "ROLE_SEQNO" DECIMAL(21, 0) NOT NULL,
                        //          "REMARK" VARCHAR(500))
                        //     IN "USERSPACE1";

                        if (sqlTable != null)
                        {
                            tables.Add(sqlTable);
                            sqlTable = null;
                        }

                        tableName = currLineText.Substring(
                            currLineText.IndexOf(".\"") + 2 ,
                            currLineText.IndexOf("\"", currLineText.IndexOf(".\"") + 2) - currLineText.IndexOf(".\"") - 2).Trim().ToUpper();

                        if (tables.Where(x => x.Name == tableName).Any())
                        {
                            continue;
                        }

                        sqlTable = new Table()
                        {
                            Name = tableName
                        };
                        continue;
                    }

                    if (!currLineText.Trim().StartsWith("IN \""))
                    {
                        // IN "SFA_CMN_TB1";

                        if (sqlTable != null)
                        {
                            //"LN_ACT_NO" VARCHAR(20) NOT NULL,
                            //"PRE_APR_DATE" VARCHAR(8) , 
                            //"ACCOUNT_BALANCE" DECIMAL(15, 2) )   

                            var columndifinations = currLineText.Replace("NOT NULL", "").Replace("  ", " ").Trim().Split(' ');
                            if (columndifinations.Length >= 2)
                            {
                                Column column = new Column()
                                {
                                    Name = columndifinations[0].Replace("\"", "").Trim().ToUpper()
                                };

                                if (columndifinations[1].ToLower().Contains("char") ||
                                    columndifinations[1].ToLower().Contains("blob") ||
                                    columndifinations[1].ToLower().Contains("clob"))
                                {
                                    var columnType = columndifinations[1].Trim().Replace(" ", "").Split('(');
                                    column.DataType = columnType[0].ToLower().Trim();
                                    column.DataLength = columnType[1].ToLower().Replace(")", "").Replace(",", "").Trim();
                                }
                                else if (columndifinations[1].ToLower().Contains("timestamp"))
                                {
                                    column.DataType = "timestamp";
                                }
                                else
                                {
                                    column.DataType = columndifinations[1].Trim().ToLower().Trim().TrimEnd(new char[] { ',' });
                                }

                                sqlTable.Columns.Add(column);
                            }
                        }
                    }

                    if (currLineText.Trim().EndsWith(";"))
                    {
                        // IN "SFA_CMN_TB1";

                        if (sqlTable != null)
                        {
                            tables.Add(sqlTable);
                            sqlTable = null;
                        }
                    }
                }




                using (var file = new StreamWriter(distFile, false, Encoding.UTF8))
                {
                    Console.WriteLine("\r\n\r\n<<<<<<<<< Generate sql >>>>>>>>>>>");
                    count = 0;
                    foreach (var table in tables)
                    {
                        count++;
                        Console.WriteLine($"Generate sql {count} {table.Name}...");

                        file.WriteLine(MSSqlHelper.GenerateSql(table));

                        file.Write(Environment.NewLine);
                        file.Write(Environment.NewLine);
                    }

                    Console.WriteLine(String.Format("Generate {0} Tables.", tables.Count));
                }

                using (var file = new StreamWriter($"{distFile}.csv", false, Encoding.UTF8))
                {
                    Console.WriteLine("\r\n\r\n<<<<<<<<< Generate table csv >>>>>>>>>>>");
                    count = 0;
                    string tmpdatatype = null;
                    foreach (var table in tables)
                    {
                        count++;
                        Console.WriteLine($"Generate table {count} {table.Name}...");

                        foreach (var column in table.Columns)
                        {
                            tmpdatatype = column.DataType.ToLower().EndsWith("char") ? column.DataType + "(" + column.DataLength + ")" : column.DataType;
                            file.WriteLine($"\"{table.Name}\",\"{table.Description}\",\"{column.Name}\",\"{tmpdatatype}\",\"{column.Description}\"");
                        }
                    }
                }


                //if(true)
                //{
                //    Console.WriteLine(String.Format("\r\n\r\n<<<<<<<<< Generate insert sql >>>>>>>>>>>"));

                //    RowDist rowDist = null;
                //    ColumnDist columnDist = null;
                //    List<RowDist> tmpRows = new List<RowDist>();

                //    count = 0;
                //    foreach (var item in columnDists)
                //    {
                //        count++;
                //        Console.WriteLine($"Prepare insert data {count} {item.TableName}.{item.ColumnName}...");

                //        if (!tables.Where(x => x.Name == item.TableName).Any())
                //        {
                //            continue;
                //        }


                //        columnDist = null;
                //        rowDist = tmpRows.Where(x => x.TableName == item.TableName).FirstOrDefault();

                //        if (rowDist != null)
                //        {
                //            columnDist = rowDist.Columns.Where(x => x.SeqNo == item.SeqNo && 
                //                x.ColumnName == item.ColumnName &&
                //                x.Type == item.Type).FirstOrDefault();
                //        }
                //        else
                //        {
                //            rowDist = new RowDist()
                //            {
                //                TableName = item.TableName
                //            };

                //            tmpRows.Add(rowDist);
                //        }

                //        if (columnDist == null)
                //        {
                //            rowDist.Columns.Add(item);
                //        }
                //    }

                //    using (var file = new StreamWriter($"{distFile}-1.sql", false, Encoding.UTF8))
                //    {
                //        count = 0;

                //        StringBuilder sqlColmun = null;
                //        StringBuilder sqlValue = null;

                //        foreach (var row in tmpRows)
                //        {
                //            count++;
                //            Console.WriteLine($"Prepare insert sql {count} {row.TableName}...");
                //            sqlColmun = new StringBuilder();
                //            sqlValue = new StringBuilder();

                //            sqlColmun.Append($"insert into {row.TableName} (!");
                //            sqlValue.Append($" values(!");

                //            foreach (var column in row.Columns)
                //            {
                //                sqlColmun.Append($", {column.ColumnName}");
                //                sqlValue.Append($", \"{column.ColumnValue}\"");
                //            }

                //            sqlColmun.Append($")");
                //            sqlValue.Append($")");

                //            file.WriteLine($"{sqlColmun.Replace("!,", "").ToString()} {sqlValue.Replace("!,", "").ToString()};");
                //        }
                //    }

                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }




        private static ColumnDist GenerateColumnDist(string sql)
        {
            //UPDATE SYSSTAT.COLDIST
            //SET COLVALUE = X'5346415F44575F494E544552564945575F53473031',
            //    VALCOUNT = 86052
            //WHERE COLNAME = 'TBNAME' AND TABNAME = 'SYSCOLDIST'
            //      AND TABSCHEMA = 'SYSIBM  '
            //      AND TYPE = 'Q'
            //      AND SEQNO = 8;

            ColumnDist columnDist = new ColumnDist();

            string colvalue = null;
            string tmpSql = sql.Replace(" ", "");

            columnDist.TableName = tmpSql.Substring(tmpSql.IndexOf("TABNAME='") + 9,
                    tmpSql.IndexOf("'", tmpSql.IndexOf("TABNAME='") + 9) - tmpSql.IndexOf("TABNAME='") - 9).ToUpper();

            columnDist.ColumnName = tmpSql.Substring(tmpSql.IndexOf("COLNAME='") + 9,
                    tmpSql.IndexOf("'", tmpSql.IndexOf("COLNAME='") + 9) - tmpSql.IndexOf("COLNAME='") - 9).ToUpper();

            columnDist.Type = tmpSql.Substring(tmpSql.IndexOf("TYPE='") + 6,
                    tmpSql.IndexOf("'", tmpSql.IndexOf("TYPE='") + 6) - tmpSql.IndexOf("TYPE='") - 6);

            columnDist.Type = tmpSql.Substring(tmpSql.IndexOf("SEQNO=") + 6,
                    tmpSql.IndexOf("'", tmpSql.IndexOf("SEQNO=") + 6) - tmpSql.IndexOf("SEQNO=") - 6);

            if (tmpSql.IndexOf("COLVALUE=X'") > 0)
            {
                colvalue = tmpSql.Substring(tmpSql.IndexOf("COLVALUE=X'") + 11,
                    tmpSql.IndexOf("'", tmpSql.IndexOf("COLVALUE=X'") + 11) - tmpSql.IndexOf("COLVALUE=X'") - 11);

                colvalue = Encoding.GetEncoding(950).GetString(Utili.HexStringToBytes(colvalue));
            }
            else
            {
                colvalue = tmpSql.Substring(tmpSql.IndexOf("COLVALUE='") + 10,
                    tmpSql.IndexOf("'", tmpSql.IndexOf("COLVALUE='") + 10) - tmpSql.IndexOf("COLVALUE='") - 10);
            }

            columnDist.ColumnValue = colvalue;

            return columnDist;
        }





        private class RowDist
        {
            public RowDist()
            {
                Columns = new List<ColumnDist>();
            }

            public string TableName { get; set; }
            public List<ColumnDist> Columns { get; set; }
        }


    }
}
