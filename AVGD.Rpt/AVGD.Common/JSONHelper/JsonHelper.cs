using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace AVGD.Common
{
    //http://www.c-sharpcorner.com/UploadFile/9bff34/3-ways-to-convert-datatable-to-json-string-in-Asp-Net-C-Sharp/
    public class JsonHelper
    {
        #region ---- DataTable2JSON ----

        #region StringBuilding


        /// <summary>
        /// 将DataTable转换为JSON string 
        /// </summary>
        /// <param name="table">DataTable对象</param>
        /// <returns></returns>
        public static string DataTableToJSONWithStringBuilder(DataTable table)
        {
            var JSONString = new StringBuilder();            
            if (table.Rows.Count > 0)
            {
                JSONString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JSONString.Append("{");
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j < table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JSONString.Append("}");
                    }
                    else
                    {
                        JSONString.Append("},");
                    }
                }
                JSONString.Append("]");
            }
            return JSONString.ToString();
        }


        #endregion

        #region JavaScriptSerializer

        /// <summary>
        /// 将DataTable转换为JSON string 
        /// </summary>
        /// <param name="table">DataTable对象</param>
        /// <returns></returns>
        public static string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }  

        #endregion

        #region Newtonsoft JsonConvert

        /// <summary>
        /// 将DataTable转换为JSON string 
        /// </summary>
        /// <param name="table">DataTable对象</param>
        /// <returns></returns>
        public static string DataTableToJSONWithJSONNet(DataTable table) 
        {  
           string JSONString=string.Empty;              
           JSONString = JsonConvert.SerializeObject(table);  
           return JSONString;  
        }
 
        #endregion

        #region MyMethod
        /// <summary>
        /// 转化为 List 符合项目需求
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> DataTableToJSONWithList(DataTable T)
        {
          
            List<Dictionary<string, string>> data = null;
            if (T != null)
            {
                data = new List<Dictionary<string, string>>(T.Rows.Count);
                int rowNumber = 0;
                int columuNameLen = T.Columns.Count;
                Dictionary<string, string> smallData=null;
                foreach (System.Data.DataRow rows in T.Rows)
                {
                    smallData = new Dictionary<string, string>(columuNameLen);
                    int columnnumber = 0;
                    foreach (System.Data.DataColumn columns in T.Columns)
                    {
                        smallData.Add(T.Columns[columnnumber].ColumnName.ToString(), T.Rows[rowNumber][columnnumber].ToString());
                        columnnumber++;
                    }
                    data.Add(smallData);
                    rowNumber++;
                }
            }
            return data;
        }
        #endregion

        #region MyRegion
        //http://stackoverflow.com/questions/17398019/how-to-convert-datatable-to-json-in-c-sharp
        //public string DataTable2Json(DataTable dt)
        //{
        //    //convert datatable to list using LINQ. Input datatable is "dt"
        //    var lst = dt.AsEnumerable()
        //        .Select(r =>r.Table.Columns.GetEnumerator()
        //                .Select(c => new KeyValuePair<string, object>(c.ColumnName, r[c.Ordinal])
        //               ).ToDictionary(z => z.Key, z => z.Value)
        //        ).ToList();
        //    //now serialize it
        //    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //    return serializer.Serialize(lst);
        
        //}
        #endregion
        #endregion
    }
}