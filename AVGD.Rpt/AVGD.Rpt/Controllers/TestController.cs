using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AVGD.Rpt.Controllers
{
    using AVGD.Common;
    public class TestController : Controller
    {
        //
        // GET: /Test/

        //https://github.com/jquery/jquery
        public ActionResult Jquery()
        {

            return View();
        }
        //https://github.com/t4t5/sweetalert
        public ActionResult SweetAlert()
        {

            return View();
        }
        //https://github.com/davidshimjs/qrcodejs
        public ActionResult QrCodeJs()
        {


            return View();
                
        }

        //https://github.com/js-cookie/js-cookie
        public ActionResult CookiesJs()
        {
            Cookies.Set("zhengwangeng", "name");
            Cookies.Set("z", "n", DateTime.Now.AddDays(1));
            Cookies.Set("sub", "age=10&name=z&value=w&price=1000");



            Cookies.Get("sub", "price", null, null);

            return View();
        }

        //https://github.com/blueimp/JavaScript-MD5
        public ActionResult Md5Js()
        {

            return View();
            
        }

        //https://github.com/airbnb/javascript
        public ActionResult javascript()
        {

            return View();
        }


        public ActionResult LinqJs()
        {

            return View();
        }

    }
}
