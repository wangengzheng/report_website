using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AVGD.Common;
using AVGD.Rpt.Models;
using System.Data;
using AVGD.Rpt.DAL;
using AVGD.Rpt.ViewModel;

namespace AVGD.Rpt.Controllers
{
    [MyAuthorize]
    public class LinkController : Controller
    {
        private DbReport _db = new DbReport();

        #region ---- 查看 E 淘 审核照片 ----


        public ActionResult ETaoPhoto(string Id /*= "040427cf-0cb9-4ef2-8379-5b63df38e98a"*/)
        {
            if (string.IsNullOrEmpty(Id))
                return View();
            MYSQLInit Sql = new MYSQLInit();
            Sql.Append("select  idCardImg1 as 'F_idCard',idCardImg2 as 'B_idCard' ,license as 'License' , storeImg1 as 'Store_1', storeImg2  as 'Store_2' ,storeImg3 as 'Store_3' ,`name` ,phone,authenticId from etao_authentic");
            Sql.Where("authenticId =", Id);

            DataTable T = new CommondController(_db).GetDataTableWithParam(Sql.GetCurrentSQL(), Sql.GetCurrentPara());

            ETaoPhoto model = T.ConvertTo<ETaoPhoto>().FirstOrDefault();

            return View(model);
        }

        #endregion

    }
}
