using AVGD.Common;
using AVGD.Rpt.Areas.Admin.DAL;
using AVGD.Rpt.Areas.Admin.Models;
using AVGD.Rpt.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AVGD.Rpt.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : Controller
    {
        private DbReport _db = new DbReport();
        [GZipOrDeflate]
        public ActionResult Index([Bind(Include="report,title")]PageList pagelist)
        {
            if (!string.IsNullOrWhiteSpace(pagelist.report))
            {
                AdminDALController adminDAL=new AdminDALController(_db);
                rpt_categorydetail categoryDetail = adminDAL.GetCategoryDetail(pagelist.report);
                if (categoryDetail != null)
                {
                    ViewBag.报表类型 = categoryDetail.Id;
                    ViewBag.报表名称 = categoryDetail.Detailedname;
                    ViewBag.报表sql语句 = categoryDetail.Sqlvalue;
                    ViewBag.统计字段 = categoryDetail.Total;
                    ViewBag.排序字段 = categoryDetail.Sort;
                    ViewBag.排序方式 = categoryDetail.Order;
                }
            }
            ViewBag.report = pagelist.report;
            ViewBag.Title = pagelist.title;
            return View();
        }

        #region  ---- 报表类别 ----

        #region --- 获取报表类别信息 ----
        [GZipOrDeflate]
        public ActionResult getonerowjson(string report)
        {
            List<rpt_categorydetail> list = new List<rpt_categorydetail>();
            AdminDALController adminDAL=new AdminDALController(_db);
            list.Add(adminDAL.GetCategoryDetail(report));
            return Json(new { rows = list },JsonRequestBehavior.AllowGet);
        }    
        #endregion

        #region ---- 更新报表类别信息 ----
        [ValidateInput(false)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[BackMangerMYSQLInject]
            
        public ActionResult UpdateMessage()
        {
            var data = new  AVGD.Rpt.Areas.Admin.Models.FormValue
            {
                sqlvalue = Request.Form["报表sql语句"],
                totalcolumn = Request.Form["统计字段"],
                sortcolumn = Request.Form["排序字段"],
                sorttype = Request.Form["排序方式"],
                report = Request.Form["report"]
            };
            #region 检验输入的sql语句是否安全
             if (!checkSql(data.sqlvalue))
            {
                return Content("错误!");
            }
            #endregion         
             AdminDALController adminDAL = new AdminDALController(_db);
             int rowEf = adminDAL.UpdateCategoryMessage(data);
            if (1 == rowEf)
            {
                return Content("ok");
            }
            //LogHelper.WriteLog(typeof(rptedsmallController), "admin home updatemessage" + data);
            return Content("错误!");

        }
        /// <summary>
        /// 检查sql语句是否符合正常的输入
        /// </summary>
        /// <param name="sql">执行的sql语句</param>
        /// <returns></returns>
        private bool checkSql(string sql)
        {
            if (sql != string.Empty && sql != null)
            {
                string[] excluded = "drop,delete,update,database,exec,xp_cmdshell,sysadmin,dbcreator,diskadmin,processadmin,server,admin,setupadmin,securityadmin,bulkadmin,declare,alert ,create table,mysqladmin,mysqldump,insert ,create ,use ,show ,tables ,information_schema".Split(',');
                string sqlstr = sql.ToString().ToLower();
                foreach (var indexstr in excluded)
                {
                    if (sqlstr.Contains(indexstr))
                    {
                        BugLog.Write("输入排除的sql关键字！！");
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion
        
        #endregion

        #region  ---- 报表列 ----

        #region ---- 获取报表列信息 ----
        public ActionResult GetReportColumn(string report)
        {
            List<rpt_column> list = null;
            AdminDALController adminDAL = new AdminDALController(_db);
            list = adminDAL.GetReportColumn(report);
            return Json(new { rows = list }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ---- 报表列信息 更新 ----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetUpEditWinUpdate()
        {
            var data = new SetupFormValue
            {
                reportcolumn = Request.Form["报表列名"],
                reportvalue = Request.Form["列名的值"],
                Id = Request.Form["Id"]
            };
            AdminDALController adminDAL = new AdminDALController(_db);

            int rowEf = adminDAL.SetUpEditWinUpdate(data);

            if (1 == rowEf)
            {
                return Content("ok");
            }            
            return Content("更新出错,请联系管理员!");
        }
        #endregion

        #region ---- 报表列信息 删除 ----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetUpEditWinDelete()
        {
            var data = new SetupFormValue
            {
                Id = Request.Form["Id"]
            };
            AdminDALController adminDAL = new AdminDALController(_db);
            int rowEf = adminDAL.SetUpEditWinDelete(data);
            if (1 == rowEf)
            {
                return Content("ok");
            }            
            return Content("删除出错,请联系管理员!");
        }
        #endregion

        #region ---- 报表列信息 新增 ----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetUpEditWinAdd()
        {
            var data = new SetupFormValue
            {
                reportcolumn = Request.Form["报表列名"],
                reportvalue = Request.Form["列名的值"],
                report = Request.Form["report"]
            };
            AdminDALController adminDAL = new AdminDALController(_db);
            int  rowEf = adminDAL.SetUpEditWinAdd(data);
            if (1 == rowEf)
            {
                return Content("ok");
            }
            return Content("新增出错,请联系管理员!");
        }
        #endregion

        #endregion

        #region ----  设置 -----

        #region ----  所有类别 ----
        public ActionResult AllCategory()
        {
            ViewBag.排序方式 = "desc";
            return View();
        }

        #region ---- 获取设置下所有类别 信息 ----
        public ActionResult GetAllCategoryJson()
        {
            AdminDALController adminDAL = new AdminDALController(_db);
            return Json(new { rows = adminDAL.GetAllCategoryDetail() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ---- 新增类别信息 ----
        public ActionResult NewCategory()
        {
            var data = new Category
            {
                CategoryName = Request.Form["类别名"]
            };
            AdminDALController adminDal = new AdminDALController(_db);
            int rowEf = adminDal.AddCategory(data);
            if (1 == rowEf)
            {
                return Content("ok");
            }

            return Content("no");
        }
        #endregion

        #region ---- 更新类别信息 ----
        public ActionResult EditCategory()
        {
            var data = new Category
            {
                Id = Convert.ToInt32(Request.Form["主键"]),
                CategoryName = Request.Form["类别名"]
            };
            AdminDALController adminDal = new AdminDALController(_db);
            int rowEf = adminDal.UpdateCategory(data);
            if (1 == rowEf)
            {
                return Content("ok");
            }
            return Content("no");
        }
        #endregion

        #region ---- 删除类别信息 ----
        public ActionResult  RemoveCategory()
        {
            int id = -1;
            int.TryParse(Request.Form["主键"],out id);
            AdminDALController adminDal = new AdminDALController(_db);
            int rowEf = adminDal.DeleteCategory(new Category { Id = id });
            if (1 == rowEf)
            {
                return Content("ok");
            }
            return Content("no");
        }
        #endregion

        #endregion

        #region ---- 所有报表 ----
        public ActionResult AllReport()
        {

            return View();
        }


        #region ---- 获取所有报表的信息 ----
        public ActionResult GetAllReportJson()
        {
            AdminDALController adminDAL = new AdminDALController(_db);
            return Json(new { rows = adminDAL.GetAllReport() }, JsonRequestBehavior.AllowGet);
        }

        #region ---- 获取类别下拉框 ----
        public ActionResult getCategoryDropDownList()
        {
            return PartialView("_dictDropDownList", new AdminDALController(_db).GetCategoryDropDownList());
        }
        #endregion

        #region --- 新增报表 ----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Newreport()
        {
            var data = new AVGD.Rpt.Areas.Admin.Models.FormValue
            {
                reportname = Request.Form["报表名称"],
                sqlvalue = Request.Form["报表sql值"],
                totalcolumn = Request.Form["统计字段"],
                categoryId = Convert.ToInt32(Request.Form["报表类别"]),
                sortcolumn = Request.Form["排序字段"],
                sorttype = Request.Form["排序方式"],
                report = Request.Form["报表名称"].GetMD5String()
            };
            #region 检验输入的sql语句是否安全
            if (!checkSql(data.sqlvalue))
            {
                return Content("错误!");
            }
            #endregion
            int rowEf = new AdminDALController(_db).AddReport(data);
            if (1 == rowEf)
            {
                return Content("ok");
            }

            return Content("no");
        }
        #endregion

        #region ---- 编辑报表 ----
        [HttpPost]
        [ValidateAntiForgeryToken]
        [BackMangerMYSQLInject]
        public ActionResult Editreport()
        {
            var data = new AVGD.Rpt.Areas.Admin.Models.FormValue
            {
                reportname = Request.Form["报表名称"],
                sqlvalue = Request.Form["报表sql值"],
                categoryId = Convert.ToInt32(Request.Form["报表类别"]),
                totalcolumn = Request.Form["统计字段"],
                sortcolumn = Request.Form["排序字段"],
                sorttype = Request.Form["排序方式"],
                report = Request.Form["report"]
            };
            #region 检验输入的sql语句是否安全
            if (!checkSql(data.sqlvalue))
            {
                return Content("错误!");
            }
            #endregion
            int rowEf = new AdminDALController(_db).EditReport(data);
            if (1 == rowEf)
            {
                return Content("ok");
            }
            return Content("no");
        }
        #endregion

        #region ---- 删除报表 ----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Removereport()
        {
            var report = Request.Form["report"];
            int rowEf = new AdminDALController(_db).RemoveReport(report);
            if (1 == rowEf)
            {
                return Content("ok");
            }
            return Content("no");
        }
        #endregion

        #endregion
        #endregion

        #endregion
    }
}
