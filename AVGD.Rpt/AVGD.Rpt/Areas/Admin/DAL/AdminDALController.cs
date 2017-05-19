using AVGD.Common;
using AVGD.Rpt.Areas.Admin.Models;
using AVGD.Rpt.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace AVGD.Rpt.Areas.Admin.DAL
{
    public class AdminDALController
    {
         public DbReport _db = null;

        public AdminDALController(DbReport db)
        {
            _db = db;
        }

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

        #region ---- 获取Sql语句的 DataTable 数据集 ----
        /// <summary>
        /// 获取Sql语句的 DataTable 数据集
        /// </summary>
        /// <param name="sqlValue">sql语句</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sqlValue)
        {
            DataSet ds = new DataSet();
            ds = GetDataSet(sqlValue);
            return ds.Tables[0] ?? new DataTable();
        }

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

        #region ---- 获取类别信息 ----
        public rpt_categorydetail GetCategoryDetail(string report)
        {
            return _db.rpt_categorydetail.Where(rpt => rpt.Id == report).FirstOrDefault();            
        }
        #endregion

        #region ---- 更新类别信息 ----
        public int UpdateCategoryMessage(AVGD.Rpt.Areas.Admin.Models.FormValue detail)
        {
            rpt_categorydetail rpt_categorydetail = _db.rpt_categorydetail.Where(category => category.Id == detail.report).FirstOrDefault();
            if (rpt_categorydetail != null)
            {
                _db.rpt_categorydetail.Attach(rpt_categorydetail);
                rpt_categorydetail.Order = detail.sorttype;
                rpt_categorydetail.Sqlvalue = detail.sqlvalue;
                rpt_categorydetail.Sort = detail.sortcolumn;
                rpt_categorydetail.Total = detail.totalcolumn;
                return  _db.SaveChanges();
            }
            return 0;
        }
        #endregion

        #region ---- 获取类别列信息 ----

        public List<rpt_column> GetReportColumn(string report)
        {
            return _db.rpt_column.Where(rpt => rpt.Columnreport == report).ToList();
        }
        #endregion

        #region ----报表列 更新 ----
        public int SetUpEditWinUpdate(SetupFormValue form)
        {
            int id = -1;
            int.TryParse(form.Id, out id);
            rpt_column rpt_column = _db.rpt_column.Where(rpt => rpt.Id == id).FirstOrDefault();
            if (rpt_column != null)
            {
                _db.rpt_column.Attach(rpt_column);
                rpt_column.Columnname = form.reportcolumn;
                rpt_column.Columnvalue = form.reportvalue;
                return  _db.SaveChanges();
            }
            return 0;
        }
        #endregion

        #region --- 报表列 删除 ----
        public int SetUpEditWinDelete(SetupFormValue form)
        {
            int id = -1;
            int.TryParse(form.Id, out id);
            rpt_column rpt_column = _db.rpt_column.Where(r => r.Id == id).FirstOrDefault();
            if (rpt_column != null)
            {
                _db.rpt_column.Remove(rpt_column);
                return _db.SaveChanges();
            }
            return 0;
        }
        #endregion

        #region ---- 报表列 新增 ----
        public int SetUpEditWinAdd(SetupFormValue form)
        {
            rpt_column rpt_column = new rpt_column();
            rpt_column.Columnname = form.reportcolumn;
            rpt_column.Columnreport = form.report;
            rpt_column.Columnvalue = form.reportvalue;
            _db.rpt_column.Add(rpt_column);
            return _db.SaveChanges();
        }
        #endregion

        #region ---- 设置 获取所有 报表列别 ----
        public List<rpt_category> GetAllCategoryDetail()
        {
            return _db.rpt_category.ToList();            
        }
        #endregion

        #region ---- 新增类别 ----
        public int AddCategory(Category category)
        {
            try
            {
                rpt_category rpt_category = new rpt_category();
                rpt_category.Id = category.Id;
                rpt_category.Catrgoryname = category.CategoryName;
                _db.rpt_category.Add(rpt_category);
                return _db.SaveChanges();
            }
            catch (Exception ex)
            {
                BugLog.Write(ex.ToString());
                return 0;
            }
        }
        #endregion

        #region ---- 更新类别 ----
        public int UpdateCategory(Category category)
        {
            rpt_category rpt_category = _db.rpt_category.Where(rpt => rpt.Id == category.Id).FirstOrDefault();
            if (rpt_category != null)
            {
                _db.rpt_category.Attach(rpt_category);
                rpt_category.Catrgoryname = category.CategoryName;
                return _db.SaveChanges();
            }
            return 0;
        }
        #endregion

        #region ---- 删除类别 ----
        public int DeleteCategory(Category category)
        { 
            rpt_category rpt_category = _db.rpt_category.Where(rpt => rpt.Id == category.Id).FirstOrDefault();
            if (rpt_category != null)
            {
                _db.rpt_category.Remove(rpt_category);
               return _db.SaveChanges();
            }
            return 0;
        }
        #endregion

        #region ---- 获取所有报表信息 ----
        public List<rpt_categorydetail> GetAllReport()
        {
            return _db.rpt_categorydetail.ToList();
        }
        #endregion

        #region ---- 类别下拉框的DIC ----
        public Dictionary<int, string> GetCategoryDropDownList()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            List<rpt_category> list = _db.rpt_category.ToList();
            foreach (var item in list)
            {
                dict.Add(item.Id, item.Catrgoryname);
            }
            return dict;
        }
        #endregion

        #region ---- 新增报表 ----
        public int AddReport(AVGD.Rpt.Areas.Admin.Models.FormValue value)
        {
            rpt_categorydetail rpt = new rpt_categorydetail();
            rpt.Id = value.report;
            rpt.Detailedname = value.reportname;
            rpt.Order = value.sorttype;
            rpt.Cateoryid = value.categoryId;
            rpt.Sort = value.sortcolumn;
            rpt.Sqlvalue = value.sqlvalue;
            rpt.Total = value.totalcolumn;
            _db.rpt_categorydetail.Add(rpt);
            return _db.SaveChanges();            
        }
        #endregion

        #region ---- 编辑报表 ----
        public int EditReport(AVGD.Rpt.Areas.Admin.Models.FormValue value)
        {
           if(value.report.IsEmpty())
            return 0;
           rpt_categorydetail rpt = _db.rpt_categorydetail.Where(r => r.Id == value.report).FirstOrDefault();
           if (rpt != null)
           {
               _db.rpt_categorydetail.Attach(rpt);
               rpt.Detailedname = value.reportname;
               rpt.Cateoryid = value.categoryId;
               rpt.Order = value.sorttype;
               rpt.Sort = value.sortcolumn;
               rpt.Sqlvalue = value.sqlvalue;
               rpt.Total = value.totalcolumn;
               return _db.SaveChanges();
           }
           return 0;
        }
        #endregion

        #region ---- 删除报表 ----
        public int RemoveReport(string report)
        {
            if(report.IsEmpty())
            return 0;
            rpt_categorydetail rpt = _db.rpt_categorydetail.Where(r => r.Id == report).FirstOrDefault();
            if (rpt != null)
            {
                _db.rpt_categorydetail.Remove(rpt);
                return _db.SaveChanges();
            }
            return 0;

        }
        #endregion
    }
}