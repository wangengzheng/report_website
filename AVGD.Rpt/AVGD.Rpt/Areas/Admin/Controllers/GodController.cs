using AVGD.Common;
using AVGD.Rpt.DAL;
using AVGD.Rpt.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AVGD.Rpt.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class GodController : Controller
    {
        private  DbReport _db = new DbReport();

        [HttpGet]
       public ActionResult Index(string sqlValue,bool isSecurityLable=false)
        {
            if (Request.Form.AllKeys.Contains("isSecurityLable") || Request.QueryString.ToString().IndexOf("isSecurityLable") > 0)
            {
                return View();
            }
            if (!isSecurityLable/*避免通过 Get 直接请求 Index 方法*/ || sqlValue.Trim().IsEmpty())
            {
                return View();
            }
            CommondController commond = new CommondController(_db);

            DataTable T = commond.GetDataTableOneRow(sqlValue);
            Session["SqlValue"] = sqlValue;
            ViewBag.sqlValue =sqlValue ;//sqlValue.Replace("\r","").Replace("\t"," ").Replace("\n"," ");
            #region T 不为空的时候
            if (T != null && T.Rows.Count>0)
            {
                var CName= T.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
                var CType=T.Columns.Cast<DataColumn>().Select(x => x.DataType.FullName).ToArray();

                ViewBag.ColumnName = CName;
                ViewBag.ColumnType = CType;

                ViewBag.zdString = string.Join(",", CName);
                ViewBag.lxString = string.Join(",", CType);
                ViewBag.不显示的类型 = "System.TimeSpan,System.Byte[]";
                ViewBag.排序字段 = CName[0];
                ViewBag.排序方式 = "desc";
                
            }
            #endregion
            return View();
        }

        [HttpPost, ValidateInput(false), BackMangerMYSQLInject,ValidateAntiForgeryToken]
        public ActionResult Index()
        {
            AVGD.Common.SessionHelper.RestSqlValue();
            return Index(sqlValue: Request.Form["sqlValue"],isSecurityLable:true);        
        }

        #region ---- EasyUi 前端获取 JSON 格式 ----
        //[GZipOrDeflate]
        [MYSQLInject]
        public JsonResult getJsonFromReport(PageList pagelist)
        {
            int total = 0;
            CommondController commond = new CommondController(_db);
            string sqlValue = commond.GetSqlValue(pagelist.report, isFillter: true);/*TODO:isFiller:true getJsonFromReport*/
            List<Dictionary<string, string>> rows = null;
            if (sqlValue.IsNotEmpty())
            {
                pagelist.sql = sqlValue;
                total = commond.GetCount(sqlValue);
                rows = commond.GetJSON(LimitDataTable(pagelist));
            }
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }

        public DataTable LimitDataTable(PageList pagelist)
        {
            CheckPageList(pagelist);
            DataTable T = null;
            string sql = string.Empty;
            if (pagelist.sql.IsNotEmpty())
            {
                pagelist.page = (pagelist.page < 1 ? 1 : pagelist.page);
                pagelist.rows = (pagelist.rows < 10 ? 10 : pagelist.rows);

                if (pagelist.sort.IsNotEmpty())
                {
                    if (pagelist.order.IsEmpty()) pagelist.order = "asc";
                    if (pagelist.sql.Contains("order by"))
                    {
                        pagelist.sql = pagelist.sql.Substring(0, pagelist.sql.IndexOf("order by"));
                    }
                    sql = string.Format(" {0}  order by  {1}  {2}  limit {3},{4}", pagelist.sql, pagelist.sort, pagelist.order, ((pagelist.page - 1) * pagelist.rows).ToString(), pagelist.rows.ToString());
                }
                else
                {
                    sql = pagelist.sql + " limit  " + ((pagelist.page - 1) * pagelist.rows).ToString() + "," + pagelist.rows.ToString();
                }
                pagelist.limitvalue = sql;
                Session["LimitSqlValue"] = sql;
                CommondController commond = new CommondController(_db);
                T = commond.GetDataTable(sql);
            }
            return T;

        }

        #endregion

        #region ---- 操作符部分视图 ----

        public ActionResult Operatorpartialview(string selectname)
        {
            ViewBag.selectname = selectname;
            return PartialView("_Operator");
        }

        #endregion

        #region ---- 校验 SQL 语句是否能运行 ----    
        [HttpPost,ValidateInput(false),BackMangerMYSQLInject,ValidateAntiForgeryToken]
        public string CheckSQLSuccess(string sqlValue)
        {
            if (sqlValue.Trim().IsEmpty()) return "False";
            try
            {
                int ROWCOUNT = new CommondController(_db).ROWCOUNT(sqlValue);
                if (ROWCOUNT > 0)
                {
                    return "True";
                }
                else {
                    return "False";
                }
            }
            catch (Exception ex)
            {
                BugLog.Write("错误的 SQL 语句 " + ex.ToString() + "\n\r" + sqlValue);
                return "Error";
            }
        }
        #endregion


        #region ----- 检查传递过来的 PageList 数据 -----

        public void CheckPageList(PageList page)
        { 
           int pageSize=0,pageIndex=0;
           if (!int.TryParse(page.page.ToString(), out pageIndex))
           {
               page.page = 1;
           }
           if (!int.TryParse(page.rows.ToString(), out pageSize))
           {
               page.rows = 20;
           }
        
        }

        #endregion

    }
}
