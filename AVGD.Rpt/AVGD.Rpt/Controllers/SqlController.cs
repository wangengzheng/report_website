
using AVGD.Common;
using AVGD.Rpt.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace AVGD.Rpt.Controllers
{
    public class SqlController : Controller
    {
        //
        // GET: /Sql/

        private DbReport _db = new DbReport();

        public ActionResult Index(string p, string s)
        {
			return null;

        }


        public int ExecuteSqlCommand(string sql, object[] para)
        {

            using (DbReport context = new DbReport())
            {
                return context.Database.ExecuteSqlCommand(sql, para);
            }
        }

        public int SqlQueryInt(string sql, object[] para)
        {

            using (DbReport context = new DbReport())
            {
                return context.Database.SqlQuery<int>(sql, para).FirstOrDefault();
            }
        }

        public DataTable GetDataTable(string sql, object[] para)
        {
            DataTable dt = new DataTable();
            using (DbReport context = new DbReport())
            {
                try
                {
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    DbDataAdapter da = new MySqlDataAdapter();
                    da.SelectCommand = cmd;
                    if (para != null && para.Length > 0)
                    {
                        cmd.Parameters.AddRange(para);
                    }
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    BugLog.Write(ex.ToString());
                    context.Database.Connection.Close();
                }
            }
            return dt;
        }

        public DataSet GetDataSet(string sql, object[] para)
        {
            DataSet ds = new DataSet();
            using (DbReport context = new DbReport())
            {
                try
                {
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    DbDataAdapter da = new MySqlDataAdapter();
                    da.SelectCommand = cmd;
                    if (para != null && para.Length > 0)
                    {
                        cmd.Parameters.AddRange(para);
                    }
                    da.Fill(ds);
                }
                catch (Exception ex)
                {
                    BugLog.Write(ex.ToString());
                    context.Database.Connection.Close();
                }
            }
            return ds;
        }

    }



    public static class MYSQLStringBuilderExtend
    {
        private static readonly string ParaName = "?Para";
        private static readonly int DefaultSize = 20;
        private static readonly string Para = "?";

        #region ----- Where ----

        ///<summary>Where 条件 传 Sql 语句 Value 对应条件的值 </summary>
        /// <param name="sql">sql 语句 <example> type = </example><returns>WHERE type = ?para* </returns></param>
        /// <param name="value">值</param>
        public static StringBuilder Where(this StringBuilder builder, string sql, object value, List<MySqlParameter> para)
        {
            string paraName = GetParaName(para.Count);
            para.Add(SetParaValue(paraName, value));
            return builder.AppendFormat(" WHERE {0} {1} ", sql, paraName);
        }

        public static StringBuilder Where(this StringBuilder builder, string sql, object value, MySqlDbType dbType, List<MySqlParameter> para)
        {
            string paraName = GetParaName(para.Count);
            para.Add(SetParaValue(paraName, value, dbType));
            return builder.AppendFormat(" WHERE {0} {1} ", sql, paraName);
        }


        #endregion

        #region ----- And -----

        ///<summary>And 条件 传 Sql 语句 Value 对应条件的值 </summary>
        /// <param name="sql">sql 语句 <example> type = </example><returns>AND type = ?para* </returns></param>
        /// <param name="value">值</param>
        public static StringBuilder And(this StringBuilder builder, string sql, object value, List<MySqlParameter> para)
        {
            string paraName = GetParaName(para.Count);
            para.Add(SetParaValue(paraName, value));
            return builder.AppendFormat(" AND {0} {1} ", sql, paraName);
        }

        public static StringBuilder And(this StringBuilder builder, string sql, object value, MySqlDbType dbType, List<MySqlParameter> para)
        {
            string paraName = GetParaName(para.Count);
            para.Add(SetParaValue(paraName, value, dbType));
            return builder.AppendFormat(" AND {0} {1} ", sql, paraName);
        }

        #endregion

        #region ---- OrderBy -----
        ///<summary>排序处理 传排序字段 Name 排序方式 Type</summary>
        /// <param name="name">排序字段</param>
        /// <param name="type">排序方式 DESC ASC</param>
        public static StringBuilder OrderBy(this StringBuilder builder, string name, string type, List<MySqlParameter> para)
        {

            if (type.Equals("ASC", StringComparison.OrdinalIgnoreCase))
            {
                type = "ASC";
            }
            else
            {
                type = "DESC";
            }

            return builder.AppendFormat(" ORDER BY {0} {1} ", name, string.IsNullOrEmpty(type) ? "ASC" : type);
        }




        #endregion

        #region  ----- Limit -------
        ///<summary>分页处理 传 PageIndex PageSize</summary>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">条数(取多少条)</param>
        public static StringBuilder Limit(this StringBuilder builder, int pageIndex, int pageSize, List<MySqlParameter> para)
        {

            string index = GetParaName(para.Count);
            para.Add(SetParaValue(index, pageIndex <= 0 ? 0 : (pageIndex - 1) * pageSize));

            string size = GetParaName(para.Count);
            para.Add(SetParaValue(size, pageSize < 0 ? DefaultSize : pageSize));

            return builder.AppendFormat(" LIMIT {0},{1} ", index, size);

        }
        ///<summary>分页处理 传 PageIndex PageSize</summary>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">条数(取多少条)</param>
        public static StringBuilder Limit(this StringBuilder builder, string pageIndex, string pageSize, List<MySqlParameter> para)
        {
            int _pageIndex = 0, _pageSize = DefaultSize;
            int.TryParse(pageIndex, out _pageIndex);
            int.TryParse(pageSize, out _pageSize);

            string index = GetParaName(para.Count);
            para.Add(SetParaValue(index, _pageIndex <= 0 ? 0 : (_pageIndex - 1) * _pageSize));

            string size = GetParaName(para.Count);
            para.Add(SetParaValue(size, _pageSize < 0 ? DefaultSize : _pageSize));

            return builder.AppendFormat(" LIMIT {0},{1} ", index, size);

        }

        #endregion


        #region ---- Insert ----


        public static StringBuilder Insert(this StringBuilder builder, string tableName, List<string> fields)
        {
            if (fields.Count <= 0)
            {
                throw new ArgumentNullException("参数列表不能为空，字段名不能跟");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName 不能为空！");
            }
            builder.AppendFormat(" INSERT INTO {0} ( ", tableName);
            string fieldNames = string.Join(",", fields);
            string fieldValues = string.Concat(Para + string.Join("," + Para, fields));
            builder.AppendFormat(" {0} ) VALUES ( {1} ", fieldNames, fieldValues);
            builder.Append(" ) ");
            return builder;
        }


        #endregion

        #region ------ Set -----

        public static StringBuilder Set(this StringBuilder bulider, string name, string value, List<MySqlParameter> para)
        {
            string paraName = GetParaName(para.Count);
            if (para.Count != 0)
            {
                bulider.AppendFormat(" , {0} = {1} ", name, paraName);
            }
            else
            {
                bulider.AppendFormat(" SET {0} = {1} ", name, paraName);
            }
            para.Add(SetParaValue(paraName, value));
            return bulider;
        }

        #endregion


        #region ----- 公共方法 ------

        private static string GetParaName(int Count)
        {
            return ParaName.GetParaName(Count);
        }


        private static string GetParaName(this string str, int index)
        {
            return str + index.ToString();
        }

        private static MySqlParameter SetParaValue(string name, object value)
        {
            return new MySqlParameter(name, value);
        }

        private static MySqlParameter SetParaValue(string name, object value, MySqlDbType type)
        {
            MySqlParameter m = new MySqlParameter();
            m.MySqlDbType = type;
            m.ParameterName = name;
            m.Value = value;
            return m;
        }

        #endregion

    }

    public class MYSQLInit
    {
        private System.Text.StringBuilder toStringBuilder = new System.Text.StringBuilder();
        public System.Text.StringBuilder Builder = new System.Text.StringBuilder();
        public System.Collections.Generic.List<MySql.Data.MySqlClient.MySqlParameter> ParaList = new System.Collections.Generic.List<MySql.Data.MySqlClient.MySqlParameter>();

        public override string ToString()
        {
            toStringBuilder.Append(Builder.ToString());
            foreach (var item in ParaList)
            {
                toStringBuilder.AppendFormat("==Key# {0} #Value# {1} #==\n\r\n\r", item.ParameterName, item.Value);
            }
            return toStringBuilder.ToString();
        }

        public string GetCurrentSQL()
        {
            return Builder.ToString();
        }

        public MySql.Data.MySqlClient.MySqlParameter[] GetCurrentPara()
        {
            return ParaList.ToArray();
        }
    }

    public static class MYSQLInitExtend
    {
        #region ---- Append ----

        public static MYSQLInit Append(this MYSQLInit @this, string sql)
        {
            @this.Builder.AppendFormat(" {0} ", sql);
            return @this;
        }

        #endregion

        #region ---- Where ----


        public static MYSQLInit Where(this MYSQLInit @this, string sql, object value)
        {
            @this.Builder.Where(sql, value, @this.ParaList);
            return @this;
        }

        public static MYSQLInit Where(this MYSQLInit @this, string sql, object value, MySqlDbType dbType)
        {
            @this.Builder.Where(sql, value, dbType, @this.ParaList);
            return @this;
        }


        #endregion

        #region ---- And ----

        public static MYSQLInit And(this MYSQLInit @this, string sql, object value)
        {
            @this.Builder.And(sql, value, @this.ParaList);
            return @this;
        }


        public static MYSQLInit And(this MYSQLInit @this, string sql, object value, MySqlDbType dbType)
        {
            @this.Builder.And(sql, value, dbType, @this.ParaList);
            return @this;
        }

        #endregion

        #region ---- OrderBy ----


        public static MYSQLInit OrderBy(this MYSQLInit @this, string name, string type)
        {
            @this.Builder.OrderBy(name, type, @this.ParaList);
            return @this;
        }




        #endregion

        #region ---- Limit ----

        public static MYSQLInit Limit(this MYSQLInit @this, int pageIndex, int pageSize)
        {
            @this.Builder.Limit(pageIndex, pageSize, @this.ParaList);
            return @this;
        }

        public static MYSQLInit Limit(this MYSQLInit @this, string pageIndex, string pageSize)
        {
            @this.Builder.Limit(pageIndex, pageSize, @this.ParaList);
            return @this;
        }

        #endregion

        #region ---- Insert ----

        public static MYSQLInit Insert(this MYSQLInit @this, string tableName, List<string> fields, List<object> values)
        {
            if (fields.Count != values.Count)
            {
                throw new ArgumentOutOfRangeException("字段列表跟值列表长度不一致");
            }
            @this.Builder.Insert(tableName, fields);
            int Index = 0;
            foreach (var item in fields)
            {
                @this.ParaList.Add(new MySqlParameter(item, values[Index++]));
            }

            return @this;
        }

        public static MYSQLInit Insert(this MYSQLInit @this, string tableName, List<string> fields, List<MySqlParameter> para)
        {
            if (fields.Count != para.Count)
            {
                throw new ArgumentOutOfRangeException("字段列表跟值列表长度不一致");
            }

            @this.Builder.Insert(tableName, fields);

            @this.ParaList = para;

            return @this;
        }

        #endregion

        #region ---- Update ----

        public static MYSQLInit Update(this MYSQLInit @this, string name, string value)
        {
            @this.Builder.Set(name, value, @this.ParaList);
            return @this;
        }

        #endregion
    }


    public static class MYSQLSQLExtend
    {
        /// <summary>
        /// 由字段名  获取前面的sql语句
        /// <example>"SELECT	a.OrderID AS '订单号' from sys_user".GetFiledName("订单号")   <remarks>取得结果 a.OrderID </remarks></example>
        /// </summary>
        /// <param name="string">sqlValue</param>
        /// <param name="name">字段</param>
        /// <returns></returns>
        public static string GetFiledName(this string @string, string name)
        {        
            var array = @string.Split(',');
            var length = array.Length;
            var index = 0;
            //给字段名 获取对应的sql语句
            //
            foreach (var item in array)
            {                
                if (item.Contains(name))
                {
                    
                    var trimValue = item.Replace("\n", " ").Replace("\t", " ").Replace("\r", " ").Trim();
                    //自定义字段名  可能包含 as 
                    var lastASIndex= trimValue.LastIndexOf("as", StringComparison.OrdinalIgnoreCase);
                    //下面的 trimValue.IndexOf(name)-1; 要求 sql 语句按严格写法写    
                    #region example
                    /*
                    select orderid as 'cc' from etao_order ;
                     * select orderid  'cc' from etao_order ;
                     * 以上可以得  以下不行
                     * select orderid as'cc'from etao_order ;                     
                     * select orderid'cc' from etao_order ;
                     */
                    #endregion
                    var lastNameIndex = trimValue.IndexOf(name) - 1;

                    var lastIndex = lastASIndex >= 0 ? lastASIndex : lastNameIndex;

                    if (index == 0)
                    {
                        var selectIndex = trimValue.IndexOf("select", StringComparison.OrdinalIgnoreCase);
                        selectIndex += "select".Length;
                        return trimValue.Substring(selectIndex,lastIndex-"select".Length);
                    }
                    else if (index == length - 1)
                    {
                        trimValue = trimValue.Substring(0, trimValue.IndexOf("from", StringComparison.OrdinalIgnoreCase));                       

                        if (trimValue.IsExitAS())
                        {
                            lastIndex = trimValue.LastIndexOf("as", StringComparison.OrdinalIgnoreCase);
                            return trimValue.Substring(0, lastIndex);
                        }
                        else 
                        {
                            //下面的 trimValue.IndexOf(name)-1; 要求 sql 语句按严格写法写    
                            return trimValue.Substring(0, trimValue.IndexOf(name) - 1);
                        }

                    }
                    else
                    {
                        return trimValue.Substring(0, lastIndex);
                    }
                }
                index++;
            }


            return name;
        }

        public static bool IsExitAS(this string @string)
        {
            //该字符串是否包含 as 关键字
            return @string.Split(" ").Any(a => a.Trim().ToLower() == "as");        
        }

    }

}
