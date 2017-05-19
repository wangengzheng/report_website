using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AVGD.Rpt.ViewModel
{
    public class SQLInject
    {
        private int ParaIndex { get; set; }

        private static readonly string ParaName = "?Para";

        public List<MySql.Data.MySqlClient.MySqlParameter> ParaList = new List<MySqlParameter>();

        public System.Text.StringBuilder SqlValue = new System.Text.StringBuilder();


        /// <summary>
        ///  获取 参数的名字
        /// </summary>
        /// <param name="index">下标  防止相同</param>
        /// <returns>?Para+ index</returns>
        private static string GetParaName(int index)
        {
            return ParaName + index.ToString();
        }

        /// <summary>
        /// 设置 MySqlParameter
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>new MySqlParameter(name, value);</returns>
        private static MySqlParameter SetParaValue<T>(string name, T value, MySqlDbType type)
        {
            MySqlParameter m = new MySqlParameter();
            m.MySqlDbType = type;
            m.ParameterName = name;
            m.Value = value;
            return m;
        }

        private static MySqlParameter SetParaValue<T>(string name, T value)
        {
            return new MySqlParameter(name, value);
        }


        /// <summary>
        /// 拼接 sql 语句 防止注入攻击
        /// </summary>
        /// <param name="sqlValue">例如 where seqid <</param>
        /// <param name="value">100</param>
        public  void SetSqlValue<T>(string sqlValue,T value)
        {
            var paraName = GetParaName(ParaIndex++);
            //参数名
            SqlValue.AppendFormat(" {0} {1}", sqlValue, paraName);

            ParaList.Add(SetParaValue<T>(paraName, value));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlValue"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public void SetSqlValue<T>(string sqlValue, T value,MySqlDbType type)
        {
            var paraName = GetParaName(ParaIndex++);
            //参数名
            SqlValue.AppendFormat(" {0} {1}", sqlValue, paraName);

            ParaList.Add(SetParaValue<T>(paraName, value,type));
        }
    }

    public static class SQL
    {
        private static readonly string ParaName = "?Para";



        public static System.Text.StringBuilder MosaicSQL(this System.Text.StringBuilder sb, string sql)
        {
            return sb.AppendFormat(" {0} ", sql);
        }
        /// <summary>
        /// Mosaic 拼接 sql
        /// </summary>
        /// <typeparam name="T">Value 类型</typeparam>
        /// <param name="sb">StringBuilder</param>
        /// <param name="sql"> userlogno like</param>
        /// <param name="Value">%a%</param>
        /// <param name="list"> MySqlParameter List 集合</param>
        /// <returns></returns>
        public static System.Text.StringBuilder MosaicSQL<TValueType>(this System.Text.StringBuilder sb, string sql, TValueType value, List<MySqlParameter> list)
        {
            string paraName = ParaName.GetParaName(list.Count);
            // 变量 名 ?Para0
            if (list.Count > 0)
            {
                sb.AppendFormat(" and {0} {1}", sql, paraName);
            }
            else {
                sb.AppendFormat(" where  {0} {1}", sql, paraName);
            }
            list.Add(SetParaValue<TValueType>(paraName, value));

            return sb;
        }

        public static System.Text.StringBuilder MosaicSQL<TValueType>(this System.Text.StringBuilder sb, string sql, TValueType value,MySqlDbType type ,List<MySqlParameter> list)
        {
            string paraName = ParaName.GetParaName(list.Count);
            // 变量 名 ?Para0
            if (list.Count > 0)
            {
                sb.AppendFormat(" and {0} {1}", sql, paraName);
            }
            else
            {
                sb.AppendFormat(" where  {0} {1}", sql, paraName);
            }
            list.Add(SetParaValue<TValueType>(paraName, value,type));

            return sb;
        }

        /// <summary>
        /// Mosaic 拼接 sql
        /// </summary>
        /// <typeparam name="TValueType">Value 类型</typeparam>
        /// <param name="sb">StringBuilder</param>
        /// <param name="field">字段名</param>
        /// <param name="operator">操作符</param>
        /// <param name="value">值</param>
        /// <param name="list">MySqlParameter List 集合</param>
        /// <returns></returns>
        public static System.Text.StringBuilder MosaicSQL<TValueType>(this System.Text.StringBuilder sb, string field,string @operator, TValueType value, List<MySqlParameter> list)
        {
            string paraName = ParaName.GetParaName(list.Count);
            // 变量 名 ?Para0
            if (list.Count > 0)
            {
                sb.AppendFormat(" and {0} {1} {2}", field,@operator, paraName);
            }
            else
            {
                sb.AppendFormat(" where  {0} {1} {2}", field, @operator, paraName);
            }
            list.Add(SetParaValue<TValueType>(paraName, value));

            return sb;
        }

        public static System.Text.StringBuilder MosaicSQL<TValueType>(this System.Text.StringBuilder sb,  string field,string @operator, TValueType value, MySqlDbType type, List<MySqlParameter> list)
        {
            string paraName = ParaName.GetParaName(list.Count);
            // 变量 名 ?Para0
            if (list.Count > 0)
            {
                sb.AppendFormat(" and {0} {1} {2}", field,@operator, paraName);
            }
            else
            {
                sb.AppendFormat(" where  {0} {1} {2}", field, @operator, paraName);
            }
            list.Add(SetParaValue<TValueType>(paraName, value, type));

            return sb;
        }

        private static string GetParaName(this string str, int index)
        {
            return str + index.ToString();
        }

        private static MySqlParameter SetParaValue<TValueType>(string name, TValueType value)
        {
            return new MySqlParameter(name, value);
        }

        private static MySqlParameter SetParaValue<T>(string name, T value,MySqlDbType type)
        {
            MySqlParameter m = new MySqlParameter();
            m.MySqlDbType = type;
            m.ParameterName = name;
            m.Value = value;
            return m;
        }

        public static List<T> ConvertTo<T>( this DataTable datatable) where T : new()
        {
            List<T> Temp = new List<T>();
            try
            {
                List<string> columnsNames = new List<string>();
                foreach (DataColumn DataColumn in datatable.Columns)
                    columnsNames.Add(DataColumn.ColumnName);
                Temp = datatable.AsEnumerable().ToList().ConvertAll<T>(row => getObject<T>(row, columnsNames));
                return Temp;
            }
            catch
            {
                return Temp;
            }

        }

        public static T getObject<T>(this DataRow row, List<string> columnsName) where T : new()
        {
            T obj = new T();
            try
            {
                string columnname = "";
                string value = "";
                PropertyInfo[] Properties;
                Properties = typeof(T).GetProperties();
                foreach (PropertyInfo objProperty in Properties)
                {
                    columnname = columnsName.Find(name => name.ToLower() == objProperty.Name.ToLower());
                    if (!string.IsNullOrEmpty(columnname))
                    {
                        value = row[columnname].ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (Nullable.GetUnderlyingType(objProperty.PropertyType) != null)
                            {
                                value = row[columnname].ToString().Replace("$", "").Replace(",", "");
                                objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(Nullable.GetUnderlyingType(objProperty.PropertyType).ToString())), null);
                            }
                            else
                            {
                                value = row[columnname].ToString().Replace("%", "");
                                objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(objProperty.PropertyType.ToString())), null);
                            }
                        }
                    }
                }
                return obj;
            }
            catch
            {
                return obj;
            }
        }

    }

    public static class DataTableHelper
    {
        public static List<T> DataTableToList<T>(this DataTable dataTable) where T : new()
        {
            var dataList = new List<T>();

            //Define what attributes to be read from the class
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;

            //Read Attribute Names and Types
            var objFieldNames = typeof(T).GetProperties(flags).Cast<System.Reflection.PropertyInfo>().
                Select(item => new
                {
                    Name = item.Name,
                    Type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType
                }).ToList();

            //Read Datatable column names and types
            var dtlFieldNames = dataTable.Columns.Cast<DataColumn>().
                Select(item => new
                {
                    Name = item.ColumnName,
                    Type = item.DataType
                }).ToList();

            foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
            {
                var classObj = new T();

                foreach (var dtField in dtlFieldNames)
                {
                    System.Reflection.PropertyInfo propertyInfos = classObj.GetType().GetProperty(dtField.Name);

                    var field = objFieldNames.Find(x => x.Name == dtField.Name);

                    if (field != null)
                    {

                        if (propertyInfos.PropertyType == typeof(DateTime))
                        {
                            propertyInfos.SetValue
                            (classObj, convertToDateTime(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(Nullable<DateTime>))
                        {
                            propertyInfos.SetValue
                            (classObj, convertToDateTime(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(int))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToInt(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(long))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToLong(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(decimal))
                        {
                            propertyInfos.SetValue
                            (classObj, ConvertToDecimal(dataRow[dtField.Name]), null);
                        }
                        else if (propertyInfos.PropertyType == typeof(String))
                        {
                            if (dataRow[dtField.Name].GetType() == typeof(DateTime))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToDateString(dataRow[dtField.Name]), null);
                            }
                            else
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToString(dataRow[dtField.Name]), null);
                            }
                        }
                        else
                        {

                            propertyInfos.SetValue
                                (classObj, Convert.ChangeType(dataRow[dtField.Name], propertyInfos.PropertyType), null);

                        }
                    }
                }
                dataList.Add(classObj);
            }
            return dataList;
        }

        private static string ConvertToDateString(object date)
        {
            if (date == null)
                return string.Empty;

            return date == null ? string.Empty : Convert.ToDateTime(date).ConvertDate();
        }

        private static string ConvertToString(object value)
        {
            return Convert.ToString(ReturnEmptyIfNull(value));
        }

        private static int ConvertToInt(object value)
        {
            return Convert.ToInt32(ReturnZeroIfNull(value));
        }

        private static long ConvertToLong(object value)
        {
            return Convert.ToInt64(ReturnZeroIfNull(value));
        }

        private static decimal ConvertToDecimal(object value)
        {
            return Convert.ToDecimal(ReturnZeroIfNull(value));
        }

        private static DateTime convertToDateTime(object date)
        {
            return Convert.ToDateTime(ReturnDateTimeMinIfNull(date));
        }

        public static string ConvertDate(this DateTime datetTime, bool excludeHoursAndMinutes = false)
        {
            if (datetTime != DateTime.MinValue)
            {
                if (excludeHoursAndMinutes)
                    return datetTime.ToString("yyyy-MM-dd");
                return datetTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            return null;
        }
        public static object ReturnEmptyIfNull(this object value)
        {
            if (value == DBNull.Value)
                return string.Empty;
            if (value == null)
                return string.Empty;
            return value;
        }
        public static object ReturnZeroIfNull(this object value)
        {
            if (value == DBNull.Value)
                return 0;
            if (value == null)
                return 0;
            return value;
        }
        public static object ReturnDateTimeMinIfNull(this object value)
        {
            if (value == DBNull.Value)
                return DateTime.MinValue;
            if (value == null)
                return DateTime.MinValue;
            return value;
        }
    
    }
}