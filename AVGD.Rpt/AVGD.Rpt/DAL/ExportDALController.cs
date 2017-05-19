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
    public class ExportDALController
    {
        public DbReport _db = null;

        public ExportDALController(DbReport db)
        {
            _db = db;
        }
        #region ---- 获取报表对应的Sql语句 ----

        /// <summary>
        /// 获取报表对应的Sql语句
        /// </summary>
        /// <param name="report">报表Id</param>
        /// <param name="isFillter">是否筛选  是（获取保存当前的sql的session  导出数据需要） 否 （从数据库中找默认选出所有的sql语句）</param>
        /// <returns>Sql语句</returns>
        public string GetSqlValue(string report, bool isFillter = false/*如果是高级查询  顶部查询 应该为true*/)
        {
            if (SessionHelper.GetSqlValue().IsNotEmpty() && isFillter)
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

        #region ---- 获取Sql语句的 DataTable 数据集 ----
        /// <summary>
        /// 获取Sql语句的 DataTable 数据集
        /// </summary>
        /// <param name="sqlValue">sql语句</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sqlValue)
        {
            DataSet ds = new DataSet();
            //using (MySqlConnection connection = new MySqlConnection(_db.Database.Connection.ConnectionString))
            //{
            //    if (connection.State == ConnectionState.Closed) connection.Open();
            //    using (MySqlCommand cmd = new MySqlCommand(sqlValue, connection))
            //    {
            //        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            //        adapter.SelectCommand.CommandType = CommandType.Text;
            //        adapter.Fill(ds);
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
                    da.Fill(ds);
                }
                catch (Exception ex)
                {
                    BugLog.Write(ex.ToString());
                    context.Database.Connection.Close();
                }
            }


            return ds.Tables[0] ?? new DataTable();
        }

        #endregion
    }
}