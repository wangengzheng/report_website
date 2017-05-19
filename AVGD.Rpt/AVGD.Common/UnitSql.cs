using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AVGD.Common
{
    public class UnitSql
    {
        /// <summary>
        /// 获取 SqlValue 里面 as 前面的 Sql 值
        /// </summary>
        /// <param name="name">表单值</param>
        /// <param name="sqlValue"> Sql 值</param>
        /// <returns> c1 as `c11`  返回 c1</returns>
        /// http://stackoverflow.com/questions/1297455/using-a-custom-field-in-where-clause-of-sql-query#comment1128450_1297461
        public static string GetFieldSqlByFormName(string name, string sqlValue)
        {
            /* select c1,c2 from sys_users */
            if (!sqlValue.Contains("as", StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }
            string[] Fields = sqlValue.Split(',');
            int length = Fields.Length;
            for (int index = 0; index < length; index++)
            {
                string value = Fields[index].Replace("\n", " ").Replace("\t", " ").Replace("\r", " ").Trim();
                if (value.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (value.Contains("case", StringComparison.OrdinalIgnoreCase) && value.Contains("when", StringComparison.OrdinalIgnoreCase) && value.Contains("end", StringComparison.OrdinalIgnoreCase))
                    {
                        /* CASE a.IsCheckState  WHEN 0 THEN  '等待买家付款' WHEN 6 THEN '订单退款中' end END AS '订单状态' -----> 要 a.IsCheckState */
                        return value.Substring(value.IndexOf("case", StringComparison.OrdinalIgnoreCase) + 4, value.IndexOf("when", StringComparison.OrdinalIgnoreCase) - 4);
                    }
                    if (value.Contains("as", StringComparison.OrdinalIgnoreCase))
                    {
                        if (index == 0)
                        {
                            /* Select c1 As c11 , c2 as c22 from sys_users ----->要 Select c1 As c11 中的c1  */
                            return value.Substring(value.IndexOf("select", StringComparison.OrdinalIgnoreCase) + 6, value.IndexOf("as", StringComparison.OrdinalIgnoreCase) - 6);
                        }
                        /* select c1 as c11,c2 as c22 , c3 as c33 from sys_users ------>要 c2 as c22 或者 c3 as c33 from sys_users 中的 c2 或者 c3*/
                        return value.Substring(0, value.IndexOf("as", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
            return name;
        }

    }
}