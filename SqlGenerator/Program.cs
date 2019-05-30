using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 4)
            {
                Console.WriteLine("Command: ProgramName GenerateType{table/code/trans} SqlType{mysql/mssql} SourceFile DestFile");
                Console.ReadKey();
                return;
            }


            try
            {
                string generateType = args[0];
                string sqlType = args[1].ToLower();
                string sourceFile = args[2];
                string distFile = args[3];

                if (generateType.ToLower().Trim() == "table")
                {
                    Generator.GenerateTable(sqlType, sourceFile, distFile);
                }
                else if (generateType.ToLower().Trim() == "code")
                {
                    Generator.GenerateSystemCode(sqlType, sourceFile, distFile);
                }
                else if (generateType.ToLower().Trim() == "trans")
                {
                    DB2Parser.TransferCreateTable(sourceFile, distFile);
                }
                else
                {
                    throw new ArgumentException("Unkown Generate Type {0}", generateType);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("End...");
            Console.ReadKey();
        }
    }
}
