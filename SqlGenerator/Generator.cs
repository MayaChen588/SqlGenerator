using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class Generator
    {

        public static void GenerateTable(string sqlType, string sourceFile, string distFile)
        {
            List<Table> tables = new List<Table>();
            List<string> skipLines = new List<string>();

            Table sqlTable = null;

            var sqlLines = File.ReadAllLines(sourceFile, Encoding.UTF8);
            int currLineIndex = 0;
            string currLineText = String.Empty;

            foreach (var line in sqlLines)
            {
                currLineIndex++;
                currLineText = line;
                Console.WriteLine(String.Format("{0,4} {1}", currLineIndex, currLineText));

                if (String.IsNullOrWhiteSpace(currLineText)
                    || !currLineText.Contains('\t')
                    || currLineText.StartsWith("1.")
                    // || currLineText.StartsWith("鍵值欄位")
                    || currLineText.StartsWith("備註")
                    || currLineText.StartsWith("欄位名稱")
                    || currLineText.StartsWith("索引名稱"))
                {
                    skipLines.Add(String.Format(String.Format("{0,4} {1}", currLineIndex, currLineText)));
                    continue;
                }


                var splits = currLineText.Split('\t');

                if (splits.Length == 2)
                {
                    if (splits[0].Trim() == "資料表名稱")
                    {
                        // new table
                        if (sqlTable != null)
                        {
                            tables.Add(sqlTable);
                            sqlTable = null;
                        }

                        sqlTable = new Table() { Name = splits[1].Trim() };
                        continue;
                    }

                    if (splits[0].Trim() == "資料表說明")
                    {
                        if (sqlTable != null)
                        {
                            sqlTable.Description = splits[1].Trim();
                        }
                        continue;
                    }

                    if (splits[0].Trim() == "鍵值欄位")
                    {
                        if (sqlTable != null)
                        {
                            sqlTable.PrimaryKey = splits[1].Trim();
                        }
                        continue;
                    }


                    // index
                    Index newIndex = new Index();
                    newIndex.Name = splits[0].Trim();

                    if (splits[1].Contains("[unique]"))
                    {
                        newIndex.IsUnique = true;
                    }

                    var columns = splits[1].Replace("[unique]", "").Split(',');

                    foreach (var item in columns)
                    {
                        if (!String.IsNullOrWhiteSpace(item))
                        {
                            newIndex.Columns.Add(item.Trim());
                        }
                    }

                    sqlTable.Indexes.Add(newIndex);
                }

                if (splits.Length == 5)
                {
                    // Column
                    Column newColumn = new Column();
                    newColumn.Name = splits[0].Trim();
                    newColumn.Description = splits[1].Trim();
                    newColumn.DataType = splits[2].Trim();
                    newColumn.DataLength = splits[3].Trim();

                    sqlTable.Columns.Add(newColumn);
                }
            }

            if (sqlTable != null)
            {
                tables.Add(sqlTable);
                sqlTable = null;
            }


            if (skipLines.Count > 0)
            {
                Console.WriteLine(String.Format("\r\n\r\n<<<<<<<<< Skip {0} lines >>>>>>>>>>>", skipLines.Count));
                using (var file = new StreamWriter("skip.txt", false, Encoding.UTF8))
                {
                    foreach (var skipline in skipLines)
                    {
                        Console.WriteLine(skipline);
                        file.WriteLine(skipline);
                    }
                }
            }


            using (var file = new StreamWriter(distFile, false, Encoding.UTF8))
            {
                Console.WriteLine(String.Format("\r\n\r\n<<<<<<<<< Generate sql >>>>>>>>>>>"));
                foreach (var table in tables)
                {
                    Console.WriteLine(String.Format("Generate sql {0}...", table.Name));

                    if (sqlType == "mysql")
                    {
                        file.WriteLine(MysqlHelper.GenerateSql(table));
                    }
                    if (sqlType == "mssql")
                    {
                        file.WriteLine(MSSqlHelper.GenerateSql(table));
                        file.WriteLine("GO");
                    }

                    file.Write(Environment.NewLine);
                    file.Write(Environment.NewLine);
                }
            }

            Console.WriteLine(String.Format("Generate {0} Tables.", tables.Count));        
        }



        public static void GenerateSystemCode(string sqlType, string sourceFile, string distFile)
        {
            List<SystemCode> sysCodes = new List<SystemCode>();
            List<string> skipLines = new List<string>();

            SystemCode syscode = null;

            var sqlLines = File.ReadAllLines(sourceFile, Encoding.UTF8);
            int currLineIndex = 0;
            string currLineText = String.Empty;

            foreach (var line in sqlLines)
            {
                currLineIndex++;
                currLineText = line;
                Console.WriteLine(String.Format("{0,4} {1}", currLineIndex, currLineText));

                if (String.IsNullOrWhiteSpace(currLineText)
                    || currLineText.StartsWith("項目鍵值"))
                {
                    skipLines.Add(String.Format(String.Format("{0,4} {1}", currLineIndex, currLineText)));
                    continue;
                }

                if (!currLineText.Contains('\t')
                    && currLineText.Contains(" ("))
                {
                    if (syscode != null)
                    {
                        sysCodes.Add(syscode);
                        syscode = null;
                    }

                    var codesplits = currLineText.Split(' ');
                    syscode = new SystemCode(codesplits[0], codesplits[1].Replace("(", "").Replace(")", ""));
                }

                var splits = currLineText.Split('\t');


                if (splits.Length == 5)
                {
                    SystemCodeItem codeItem = new SystemCodeItem();
                    codeItem.Code = splits[0].Trim();
                    codeItem.Value = splits[1].Trim();
                    codeItem.CodeType = splits[2].Trim();
                    codeItem.Sort = splits[3].Trim();
                    codeItem.Description = splits[4].Trim();

                    syscode.Add(codeItem);
                }
            }

            if (syscode != null)
            {
                sysCodes.Add(syscode);
                syscode = null;
            }


            if (skipLines.Count > 0)
            {
                Console.WriteLine(String.Format("\r\n\r\n<<<<<<<<< Skip {0} lines >>>>>>>>>>>", skipLines.Count));
                using (var file = new StreamWriter("skip.txt", false, Encoding.UTF8))
                {
                    foreach (var skipline in skipLines)
                    {
                        Console.WriteLine(skipline);
                        file.WriteLine(skipline);
                    }
                }
            }


            using (var file = new StreamWriter(distFile, false, Encoding.UTF8))
            {
                Console.WriteLine(String.Format("\r\n\r\n<<<<<<<<< Generate sql >>>>>>>>>>>"));
                foreach (var code in sysCodes)
                {
                    Console.WriteLine(String.Format("Generate sql {0}...", code.CodeKey));

                    if (sqlType == "mysql")
                    {
                        file.WriteLine(MysqlHelper.GenerateSql(code));
                    }
                    if (sqlType == "mssql")
                    {
                        file.WriteLine(MSSqlHelper.GenerateSql(code));
                    }

                }
            }

            Console.WriteLine(String.Format("Generate {0} SystemCodes.", sysCodes.Count));
        }    
    
    }
}
