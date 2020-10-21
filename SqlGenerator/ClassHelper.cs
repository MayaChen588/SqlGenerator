using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class ClassHelper
    {
        /// <summary>
        /// Generate c# value object class
        /// </summary>
        /// <param name="table">table object</param>
        /// <returns></returns>
        public static string GenerateVoClass(Table table)
        {
            StringBuilder sb = new StringBuilder();
            int columncount = 0;
            string className = table.Name.Replace("TExt", "");

            // generate class script
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Anz.Jcic.Infrastructure.DataAccess.Models.JisModel;");
            sb.AppendLine("using Anz.Jcic.Infrastructure.Domain;");
            sb.AppendLine("using Anz.Jcic.Infrastructure.Resources;");
            sb.AppendLine("");
            sb.AppendLine("namespace Anz.Jcic.Domain.Entity.JcicAtom");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine($"    /// {table.Description}");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public class {className} : EntityBase");
            sb.AppendLine("    {");


            // generate 建構子
            sb.AppendLine(@"
        /// <summary>
        /// 使用Data entity物件初始化Entity執行個體
        /// </summary>
        /// <param name=""dataEntity"">data entity物件</param>
        /// <remarks>僅限同一組件內由Manager呼叫使用，以建立初始物件</remarks>
" + $"        internal {className}(TExt{className} dataEntity)" + @"
        {
            if (dataEntity == null)
            {
                throw new ArgumentNullException(nameof(dataEntity));
            }

            if (String.IsNullOrEmpty(dataEntity.FormType))
            {
                throw new ArgumentException(String.Format(MessageResource.NotSetPropertyValue, ""Form.FormType""));
            }

            TransferFromDataEntity(dataEntity);
        }
");

            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("");


            // generate Columns
            columncount = 0;
            foreach (var column in table.Columns)
            {
                columncount++;

                if (column.Name.Equals("Uid", StringComparison.OrdinalIgnoreCase ) ||
                    column.Name.Equals("RowVersion", StringComparison.OrdinalIgnoreCase) ||
                    column.Name.Equals("CreateUserId", StringComparison.OrdinalIgnoreCase) ||
                    column.Name.Equals("CreateTime", StringComparison.OrdinalIgnoreCase) ||
                    column.Name.Equals("ModifyUserId", StringComparison.OrdinalIgnoreCase) ||
                    column.Name.Equals("ModifyTime", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string dataType = String.Empty;
                if (column.DataType.ToLower().EndsWith("char"))
                {
                    dataType = "string";
                }
                else if (column.DataType.ToLower().Contains("blob"))
                {
                    dataType = "byte[]";
                }
                else if (column.DataType.ToLower().Contains("clob"))
                {
                    dataType = "byte[]";
                }
                else if (column.DataType.ToLower() == "timestamp")
                {
                    dataType = "datetime?";
                }
                else if (column.DataType.ToLower() == "datetime")
                {
                    dataType = "datetime?";
                }
                else if (column.DataType.ToLower() == "int")
                {
                    dataType = "int?";
                }
                else if (column.DataType.ToLower() == "double")
                {
                    dataType = "decimal?";
                }
                else if (column.DataType.ToLower().StartsWith("decimal"))
                {
                    dataType = "decimal?";
                }
                else
                {
                    dataType = column.DataType.ToLower();
                }

                sb.AppendLine("        /// <summary>");
                sb.AppendLine($"        /// {column.Description}");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine($"        public {dataType} {column.Name} {{ get; set; }}");
            }

            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("");


            // generate Methods
            sb.AppendLine(@"
        /// <summary>
        /// 將Data Entity物件轉換為Entity物件
        /// </summary>
        /// <param name=""dataEntity"">Data Entity物件</param>
" + $"        private void TransferFromDataEntity(TExt{className} dataEntity)" + @"
        {
            if (dataEntity == null)
            {
                throw new ArgumentNullException(nameof(dataEntity));
            }

");

            columncount = 0;
            foreach (var column in table.Columns)
            {
                columncount++;

                if (column.Name.Equals("Uid", StringComparison.OrdinalIgnoreCase) ||
                    column.Name.Equals("RowVersion", StringComparison.OrdinalIgnoreCase) ||
                    column.Name.Equals("CreateUserId", StringComparison.OrdinalIgnoreCase) ||
                    column.Name.Equals("CreateTime", StringComparison.OrdinalIgnoreCase) ||
                    column.Name.Equals("ModifyUserId", StringComparison.OrdinalIgnoreCase) ||
                    column.Name.Equals("ModifyTime", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sb.AppendLine($"            this.{column.Name} = dataEntity.{column.Name};");
            }


            sb.AppendLine(@"
            this.ExtendProperty = new ExtendProperty()
            {
                Uid = dataEntity.Uid.ToString(),
                Version = BitConverter.ToInt64(dataEntity.RowVersion, 0),
                CreateUserId = !String.IsNullOrEmpty(dataEntity.CreateUserId) ? dataEntity.CreateUserId : null,
                CreateTime = dataEntity.CreateTime != null ? dataEntity.CreateTime : (DateTime?)null,
                ModifyUserId = !String.IsNullOrEmpty(dataEntity.ModifyUserId) ? dataEntity.ModifyUserId : null,
                ModifyTime = dataEntity.ModifyTime == null ? (DateTime?)null : dataEntity.ModifyTime,
            };
        }
");


            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
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
