using AVGD.Common;
using AVGD.Rpt.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace AVGD.Rpt.DAL
{
    public class CommondController
    {
        public DbReport _db = null;

        public CommondController(DbReport db)
        {
            _db = db;
        }

        #region ---- 报表对应的Sql语句 ----

        /// <summary>
        /// 获取报表对应的Sql语句
        /// </summary>
        /// <param name="report">报表Id</param>
        /// <param name="isFillter">是否筛选  是（获取保存当前的sql的session  导出数据需要） 否 （从数据库中找默认选出所有的sql语句）</param>
        /// <returns>Sql语句</returns>
        public string GetSqlValue(string report, bool isFillter = false/*如果是高级查询  顶部查询 应该为true*/)
        {
            if ( isFillter && SessionHelper.GetSqlValue().IsNotEmpty())
            {
                return SessionHelper.GetSqlValue();
            }
            if (report.IsNotEmpty())
            {
                rpt_categorydetail categoryDetail = GetCategoryDetail(report);
                if (categoryDetail != null && categoryDetail.Sqlvalue.IsNotEmpty())
                {
                    return categoryDetail.Sqlvalue;
                }
            }
            return "";
        }

        /// <summary>
        /// 删除 Sql 语句
        /// </summary>
        public void RemoveSqlValue()
        {
            SessionHelper.RestSqlValue();
        }

        #endregion

        #region ---- 获取Sql语句的Count总条数 ----
        /// <summary>
        /// 获取Sql语句的Count总条数
        /// </summary>
        /// <param name="sqlValue">sql语句</param>
        ///  <param name="optimization">是否优化</param>
        /// <returns></returns>
        public int GetCount(string sqlValue,bool optimization=true)
        {
            var cookieKey = sqlValue.GetMD5String();
            if (Cookies.Get("Count_" + cookieKey).IsEmpty())
            {
                var fromIndex = sqlValue.IndexOf("from", StringComparison.OrdinalIgnoreCase);
                var resutlSql = "select  '' " + sqlValue.Substring(fromIndex);
                //修改 select '' 空内容 来优化 sql
                var count = _db.Database.SqlQuery<int>(string.Format("select count(*) from ( {0} ) a", false ? resutlSql : sqlValue)).FirstOrDefault();
                Cookies.Set("Count_" + cookieKey, count.ToString(), DateTime.Now.AddMinutes((double)15));
                return count;
            }
            else
            {
                int count = 0;
                int.TryParse(Cookies.Get("Count_" + cookieKey), out count);
                return count;
            }
        }

        public int GetCount(string sqlValue, Array para, bool optimization = true)
        {

            int count = 0;
            try
            {
                using (DbReport context = new DbReport())
                {
                    var fromIndex = sqlValue.IndexOf("from", StringComparison.OrdinalIgnoreCase);
                    var resutlSql = "select  '' " + sqlValue.Substring(fromIndex);
                    //修改 select '' 空内容 来优化 sql   //参考todo #001
                    count = context.Database.SqlQuery<int>(string.Format("select count(*) from ( {0} ) a", optimization ? resutlSql : sqlValue), (object[])para).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                BugLog.Write(ex.ToString());
            }
            return count;
        }

        private string GetStringInArray(Array array)
        {
            var str = string.Empty;
            var length=array.Length;
            for (int i = 0; i < length; i++)
            {
                str += array.GetValue(i).ToString();
            }
            return str;
        }

        #endregion

        #region ---- 获取Sql语句的 DataTable 数据集 ----
        /// <summary>
        /// 获取Sql语句的 DataTable 数据集
        /// </summary>
        /// <param name="sqlValue">sql语句</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sqlValue)
        {
            DataTable dt = new DataTable();
            using (DbReport context = new DbReport())
            {            
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = sqlValue;
                    cmd.CommandType = CommandType.Text;
                    DbDataAdapter da = new MySqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dt);             
            }
            return dt;
        }

        public DataTable GetDataTableWithParam(string sqlValue, /*
                                                                 * MySqlParameter[] dic*/Array dic)
        {
            DataTable dt = new DataTable();
            //using (MySqlConnection connection = new MySqlConnection(_db.Database.Connection.ConnectionString))
            //{
            //    if (connection.State == ConnectionState.Closed) connection.Open();
            //    using (MySqlCommand cmd = new MySqlCommand(sqlValue, connection))
            //    {
            //        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            //        adapter.SelectCommand.CommandType = CommandType.Text;
            //        if (dic != null && dic.Length != 0)
            //        {
            //            cmd.Parameters.AddRange(dic);
            //        }
            //        adapter.Fill(dt);
            //    }
            //}
            using (DbReport context = new DbReport())
            {
                try
                {
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = sqlValue;
                    cmd.CommandType = CommandType.Text;
                    DbDataAdapter da = new MySqlDataAdapter();
                    da.SelectCommand = cmd;
                    if (dic!=null && dic.Length > 0)
                    {
                        cmd.Parameters.AddRange(dic);
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

        /* 测试 sql 注入攻击
                 public void Test2()
        {
            int index = 0;
            List<MySqlParameter> myDic = new List<MySqlParameter>();
            string  parameterName=""; 
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string[] names = "seqid,userlogno,type,TrueName".Split(",");
            string[] values = "1,admin,1,管理员".Split(",");

            foreach (var item in names)
            {
                parameterName = GetParaName(index);
                if (item == "seqid")
                {
                    sb.AppendFormat(" and seqid= {0}", parameterName);
                    myDic.Add( new MySqlParameter(parameterName, values[index]));
                }
                else if (item == "userlogno")
                {

                    sb.AppendFormat(" and userlogno= {0}", parameterName);
                    myDic.Add(new MySqlParameter(parameterName, values[index]));
                }
                else if (item == "type")
                {

                    sb.AppendFormat(" and type= {0}", parameterName);
                    myDic.Add(new MySqlParameter(parameterName, values[index]));
                }
                else if (item == "TrueName")
                {

                    sb.AppendFormat(" and TrueName= {0}", parameterName);
                    myDic.Add(new MySqlParameter(parameterName, values[index]));
                }
                index++;
            }

            TestSql(sb.ToString(), myDic.ToArray());
        }

        private string TestSql(string sqlValue,MySqlParameter[] dic)
        {
            sqlValue = "select * from sys_users where 1=1" + sqlValue;
            DataTable dt = GetDataTableWithParam(sqlValue,dic);

            return string.Empty;
        }

        private DataTable GetDataTableWithParam(string sqlValue, MySqlParameter[]  dic)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection connection = new MySqlConnection(_db.Database.Connection.ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(sqlValue, connection))
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.SelectCommand.CommandType = CommandType.Text;
                    if (dic != null && dic.Length != 0)
                    {
                        cmd.Parameters.AddRange(dic);
                    }
                    adapter.Fill(dt);
                }
            }
            return dt;
        
        }
        private MySqlParameter SetMySqlParameter<T>(string name, T value, DbType Type)
        {
            MySqlParameter nameParameter = new MySqlParameter();
            nameParameter.DbType = Type;
            nameParameter.ParameterName = name;
            nameParameter.Value = value;
            return nameParameter;
        }
         */

        #endregion

        #region ---- 获取由DataTable数据集转化的JSON字符串 ----
        /// <summary>
        /// 获取由DataTable数据集转化的List<Dictionary<string,string>>集合
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> GetJSON(DataTable dt)
        {
            return JsonHelper.DataTableToJSONWithList(dt);
        }
        #endregion

        #region ---- 获取报表类别的详细信息 ----
        /// <summary>
        /// 获取报表类别的详细信息<有缓存></有缓存>
        /// </summary>
        /// <param name="report">报表列别ID</param>
        /// <returns></returns>
        public rpt_categorydetail GetCategoryDetail(string report)
        {
            try
            {
                List<rpt_categorydetail> categorydetail = CacheHelper.Cache.RetrieveObject<List<rpt_categorydetail>>("Cache_rpt_categorydetails");
                if (categorydetail == null)
                {
                    categorydetail = _db.rpt_categorydetail.Select(rpt => rpt).ToList<rpt_categorydetail>();
                    if (categorydetail != null)
                    {
                        var path = AppDomain.CurrentDomain.BaseDirectory + "SysLog\\Cache_rpt_categorydetails.log";
                        BugLog.NoAsyncWrite("载入缓存 Cache_rpt_categorydetails", path);
                        CacheHelper.Cache.AddObjectWithFileChange("Cache_rpt_categorydetails", categorydetail, path);
                    }
                }
                return categorydetail.Where(rpt => rpt.Id == report).FirstOrDefault();
            }
            catch (Exception ex)
            {
                BugLog.Write(ex.ToString());
                return new rpt_categorydetail();
            }
        }
        #endregion

        #region ---- 获取DataTable一条数据 ----

        /// <summary>
        /// 获取DataTable的一条数据
        /// </summary>
        /// <param name="sqlValue">sql语句</param>
        /// <returns></returns>
        public DataTable GetDataTableOneRow(string sqlValue)
        {
            //return GetDataTable(string.Format("({0}) limit 1",sqlValue));
            return GetDataTable(string.Format("select top 1 * from ({0}) as a", sqlValue));
            //return GetDataTable(string.Format("select * from ( {0} ) a limit 0,1", sqlValue));
        }
        #endregion

        #region ---- 判断当前语句是否执行成功 ----
        /// <summary>
        /// 获取当前 sql 运行受影响行数
        /// </summary>
        /// <param name="sqlValue"></param>
        /// <returns></returns>
        public int ROWCOUNT(string sqlValue)
        {
            using (DbReport context = new DbReport())
            {
                //return context.Database.SqlQuery<int>(string.Format("select count(*) from ({0} limit 1 )a", sqlValue)).FirstOrDefault();                
                return context.Database.SqlQuery<int>(string.Format("select count(*) from (select top 1 * from ({0}) as b) as a", sqlValue)).FirstOrDefault();
            }
        
        }

        #endregion


        #region ---- 获取sql语句报表的订单总数 -----
        /// <summary>
        /// 返回订单号总数
        /// </summary>
        /// <param name="sqlValue">sql语句</param>
        /// <returns></returns>
        public int GetOrderCount(string sqlValue)
        {
            if (sqlValue.Contains("订单号"))
            {
                string newSqlValue = string.Format("select COUNT(DISTINCT `订单号`) from ( {0} ) a", sqlValue);
                return _db.Database.SqlQuery<int>(newSqlValue).FirstOrDefault();
            }
            else
            {
                return GetCount(sqlValue,optimization:false);
            }
        }

        public string GetOrderCountSqlValue(string sqlValue)
        {
            if (sqlValue.Contains("订单号"))
            {
                return string.Format("select COUNT(DISTINCT `订单号`) from ( {0} ) a", sqlValue);
            }
            else
            {
                return sqlValue;
            }
        }
        #endregion

        #region ---- 由sql语句 获取对应的DataSet ----
        public DataSet GetDataSet(string sqlValue)
        {
            DataSet ds = new DataSet();
            using (DbReport context = new DbReport())
            {
                try
                {
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = sqlValue;
                    cmd.CommandType = CommandType.Text;
                    DbDataAdapter da = new MySqlDataAdapter();
                    da.SelectCommand = cmd;              
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

        public DataSet GetDataSetCache(string sqlValue)
        {
            var dataSetKey = sqlValue.GetMD5String();
            var dataSet = CacheHelper.Cache.RetrieveObject(dataSetKey);
            if (dataSet == null)
            {
                dataSet = GetDataSet(sqlValue);
                if (dataSet != null)
                {
                    CacheHelper.Cache.AddObject(dataSetKey, dataSet);
                }
            }
            return dataSet as DataSet;
        }

        #endregion

        #region ---- 获取报表类别里面sql对应的列名对应的语句 -----

        public List<rpt_column> GetRptColumnEntity(string report)
        {
            try
            {
                List<rpt_column> column = CacheHelper.Cache.RetrieveObject<List<rpt_column>>("Cache_rpt_column");
                if (column == null)
                {
                    column = _db.rpt_column.Select(rpt => rpt).ToList<rpt_column>();
                    if (column != null)
                    {
                        var path = AppDomain.CurrentDomain.BaseDirectory + "SysLog\\Cache_rpt_column.log";
                        BugLog.NoAsyncWrite("载入缓存 Cache_rpt_column", path);
                        CacheHelper.Cache.AddObjectWithFileChange("Cache_rpt_column", column, path);
                    }
                }
                return column.Where(rpt => rpt.Columnreport == report).ToList();
            }
            catch (Exception ex)
            {
                BugLog.Write(ex.ToString());
                return new List<rpt_column>();
            }

        }

        #endregion

        #region todo

        #region #001
        //        select  *  FROM
        //    ezg_orders AS a
        //INNER JOIN ezg_orderdetails AS b ON (b.OrderID = a.OrderCode)
        //LEFT JOIN lamp_brand AS c ON (a.BrandID = c.seqid)
        //LEFT JOIN eds_mer AS d ON (c.MerId = d.MerId)
        //LEFT JOIN sys_users AS e ON (a.DealerCode = e.seqid)
        //LEFT JOIN lamp_product AS f ON (b.ProductID = f.seqid)
        //WHERE
        //    e.Company NOT LIKE '%测试%'
        //AND d.MerCompany NOT LIKE '%测试%'
        //AND e.Company NOT LIKE '%演示%'
        //AND d.MerCompany NOT LIKE '%演示%';



        //select  ''  FROM
        //    ezg_orders AS a
        //INNER JOIN ezg_orderdetails AS b ON (b.OrderID = a.OrderCode)
        //LEFT JOIN lamp_brand AS c ON (a.BrandID = c.seqid)
        //LEFT JOIN eds_mer AS d ON (c.MerId = d.MerId)
        //LEFT JOIN sys_users AS e ON (a.DealerCode = e.seqid)
        //LEFT JOIN lamp_product AS f ON (b.ProductID = f.seqid)
        //WHERE
        //    e.Company NOT LIKE '%测试%'
        //AND d.MerCompany NOT LIKE '%测试%'
        //AND e.Company NOT LIKE '%演示%'
        //AND d.MerCompany NOT LIKE '%演示%'
        #endregion


        #endregion

        #region test
        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetBrandList()
        {
            string @string = @"           select b.*,GROUP_CONCAT(a.StyleLabel) as StyleLabel from (select  a.BrandCode,a.StyleLabel from lamp_product as a 
            left join lamp_brand as b on(a.BrandCode=b.seqid)
            where StyleLabel is not null and StyleLabel<>'' and b.IsOperManage=1 and b.`status`=1 GROUP BY BrandCode,StyleLabel) as a
            left join lamp_brand as b on(a.BrandCode=b.seqid)
            GROUP BY a.BrandCode";

            return GetDataTable(@string);
        }
        #endregion
    }
}
