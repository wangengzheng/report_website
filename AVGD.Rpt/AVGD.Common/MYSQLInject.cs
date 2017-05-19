using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace AVGD.Common
{
    public class MYSQLInit
    {
        public StringBuilder Builder = new StringBuilder();
        public List<MySqlParameter> ParaList = new List<MySqlParameter>();

        //public override string ToString()
        //{
        //    Builder.Append("\n\t");
        //    foreach (var item in ParaList)
        //    {
        //        Builder.AppendFormat("==Key# {0} #Value# {1} #==\n\r\n\r", item.ParameterName, item.Value);
        //    }
        //    return Builder.ToString();
        //}

        public string GetCurrentSQL()
        {
            return Builder.ToString();
        }

        public Array GetCurrentPara()
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

    }

    public static class MYSQLStringBuilderExtend
    {
        private static readonly string ParaName = "?Para";
        private static readonly int DefaultSize = 20;

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
            string paraName = GetParaName(para.Count);
            para.Add(SetParaValue(paraName, name));

            string paraName1 = GetParaName(para.Count);
            para.Add(SetParaValue(paraName1, type ?? "ASC"));

            return builder.AppendFormat(" ORDER BY {0} {1} ", paraName, paraName1);
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
}