using System;
using System.Configuration;
using System.IO.Compression;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace AVGD.Common
{

    #region ---- GZipOrDeflateAttribute ----    

    /// <summary>
    /// 压缩
    /// </summary>
    public class GZipOrDeflateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string acceptencoding = filterContext.HttpContext.Request.Headers["Accept-Encoding"];

            if (!string.IsNullOrEmpty(acceptencoding))
            {
                acceptencoding = acceptencoding.ToLower();
                var response = filterContext.HttpContext.Response;

                if (acceptencoding.Contains("gzip"))
                {
                    response.AppendHeader("Content-Encoding", "gzip");
                    //response.AppendHeader("Content-Transfer-Encoding", "chunked");
                    response.Filter = new GZipStream(response.Filter,
                                          CompressionMode.Compress);
                }
                else if (acceptencoding.Contains("deflate"))
                {
                    response.AppendHeader("Content-Encoding", "deflate");
                    response.Filter = new DeflateStream(response.Filter,
                                      CompressionMode.Compress);
                }
            }
        }
    }

    #endregion

    #region ---- AjaxOnlyAttribute ----
    

    /// <summary>
    /// 处理只能ajax请求的 attributter
    /// </summary>
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Called by the ASP.NET MVC framework before the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new RedirectResult("/SiteStatus/HtmlError500/");
            }
            base.OnActionExecuting(filterContext);
        }
        
    }

    #endregion

    #region ---- AjaxSessionTimeoutAttribute ----


    /// <summary>
    /// 对于Ajax请求的中，Session失效的处理
    /// </summary>
    /// http://www.tuicool.com/articles/UbIrqa
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AjaxSessionTimeoutAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var loginUser = filterContext.HttpContext.Session["User"];
            if (loginUser == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new RedirectResult("/login/index");
                }
                return;
            }
        }
    }

    #endregion

    #region ----LoginActionFilter ----
    

    public class LoginActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool isAuthenticated = HttpContext.Current.Session["User"] == null ? false : true;
            if (!isAuthenticated)
            {
                //对于Ajax请求的中，Session失效的处理
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            rows = "[]",
                            total = 1,
                            Success = false,
                            Message = string.Empty,
                            Redirect = "/login/index"
                        }
                    };
                    return;
                }

                if ("login".Equals(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName, StringComparison.CurrentCultureIgnoreCase) &&
                    "index".Equals(filterContext.ActionDescriptor.ActionName, StringComparison.CurrentCultureIgnoreCase))
                {

                }
                else
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "index", returnUrl = filterContext.HttpContext.Request.Url, returnMessage = "您需要登录后才能进行操作." }));

                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }

    #endregion
    
    #region ---- MyAuthorizeAttribute ----    

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class MyAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {

            bool isAuthenticated = HttpContext.Current.Session["User"] == null ? false : true;
            //When user has not login yet
            if (!isAuthenticated)
            {
                var redirectUrl = "/login/index";
                if (!filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new RedirectResult(redirectUrl);
                }
                else
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            rows = "[]",
                            total = 1,
                            Success = false,
                            Message = string.Empty,
                            Redirect = redirectUrl
                        }
                    };
                }
            }
        }


    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AdminAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {

            bool isAuthenticated = HttpContext.Current.Session["User"] == null ? false : true;
            //When user has not login yet
            if (!isAuthenticated)
            {
                var redirectUrl = "/login/index";
                if (!filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new RedirectResult(redirectUrl);
                }
                else
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            rows = "[]",
                            total = 1,
                            Success = false,
                            Message = string.Empty,
                            Redirect = redirectUrl
                        }
                    };
                }
            }
            else { 
            //  is Admin
                string adminStr= ConfigurationManager.AppSettings["Admin"].ToString();
                var adminArray = adminStr.Split(',');
                var isAdmin=false;
                foreach (var str in adminArray)
                {
                    if (HttpContext.Current.Session["User"].ToString() == str)
                    {
                        isAdmin = true;
                        break;
                    }
                }
                if (!isAdmin)
                {               
                    filterContext.Result = new RedirectResult("/Report/Index");
                }
            }
        }


    }

     #endregion

    #region ---- MYSQL SQL Inject ----
     /// <summary>
    /// http://stackoverflow.com/questions/13162681/this-code-for-preventing-mysql-injection-is-good
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class MYSQLInjectAttribute:ActionFilterAttribute
    {
        //public static string[] blackList = {"--",";--",";","/*","*/","@@","@","%",
        //                               "char","nchar","varchar","nvarchar",
        //                               "alter","begin","cast","create","cursor","declare","delete","drop","end","exec","execute",
        //                               "fetch","insert","kill","open",
        //                               "select", "sys","sysobjects","syscolumns",
        //                               "table","update"};
    
        public static string[] blackList = {"--",";--",";","/*","*/","@@","@","%"};
                                      
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request=filterContext.HttpContext.Request;
            bool isTrue = false;
            foreach (string key in request.QueryString)
            {
                   if(string.Equals(key,"__RequestVerificationToken",StringComparison.OrdinalIgnoreCase))
                    continue;
                CheckInput(request.QueryString[key],ref isTrue);
                if (isTrue)
                {
                    filterContext.Result = filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            rows = "[]",
                            total = 1,
                            Success = false,
                            Message = "sqlError",
                            Redirect = ""
                        }
                    };
                    break;
                }
            }
            foreach (string key in request.Form)
            {
                if(string.Equals(key,"__RequestVerificationToken",StringComparison.OrdinalIgnoreCase))
                    continue;
                CheckInput(request.Form[key],ref isTrue);
                if (isTrue)
                {
                    filterContext.Result = filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            rows = "[]",
                            total = 1,
                            Success = false,
                            Message = "sqlError",
                            Redirect = ""
                        }
                    };
                    break;
                }
            }
            //foreach (string key in request.Cookies)
            //{
            //    CheckInput(request.Cookies[key].Value);
            //}
        }
        private void CheckInput(string parameter,ref bool isTrue)
        {
            for (int i = 0; i < blackList.Length; i++)
            {
                if ((parameter.IndexOf(blackList[i], StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    BugLog.Write("输入非法 MYSQLInjectAttribute " + parameter + "包含了 " + blackList[i]);
                    isTrue = true;
                    break;
                }
            }
        }

    }

    /// <summary>
    /// http://stackoverflow.com/questions/13162681/this-code-for-preventing-mysql-injection-is-good
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class BackMangerMYSQLInjectAttribute : ActionFilterAttribute
    {
        public static string[] blackList = { "grant", "alter", "drop", "delete", "update", "database", "exec", "xp_cmdshell", "sysadmin", "dbcreator", "diskadmin", "processadmin", "server", "admin", "setupadmin", "securityadmin", "bulkadmin", "declare", "alert ", "create table", "mysqladmin", "mysqldump", "insert ", "create ", "use ", "show ", "tables ", "information_schema", "cursor", "char", "nchar", "varchar", "nvarchar", "execute", "fetch", "kill", "sysobjects", "syscolumns", "--", ";--", ";", "/*", "*/", "@@", "@" };
        

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            bool isTrue = false;
            foreach (string key in request.QueryString)
            {
                if (string.Equals(key, "__RequestVerificationToken", StringComparison.OrdinalIgnoreCase))
                    continue;
                CheckInput(request.QueryString[key], ref isTrue);
                if (isTrue)
                {
                    filterContext.Result = new RedirectResult("/Report/Error");
                    break;
                }
            }
            foreach (string key in request.Form)
            {
                if (string.Equals(key, "__RequestVerificationToken", StringComparison.OrdinalIgnoreCase))
                    continue;
                CheckInput(request.Form[key], ref isTrue);
                if (isTrue)
                {
                    filterContext.Result = new RedirectResult("/Report/Error");
                    break;
                }
            }
            //foreach (string key in request.Cookies)
            //{
            //    CheckInput(request.Cookies[key].Value);
            //}
        }
        private void CheckInput(string parameter, ref bool isTrue)
        {
            for (int i = 0; i < blackList.Length; i++)
            {
                if ((parameter.IndexOf(blackList[i], StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    isTrue = true;
                    BugLog.Write("输入非法 MYSQLInjectAttribute " + parameter+"包含了 "+blackList[i]);
                    //throw new Exception();
                }
            }
        }

    }
    #endregion


    #region  ---- AppError ----

    /// <summary>
    /// 错误日志（Controller发生异常时会执行这里）
    /// </summary>
    public class AppHandleErrorAttribute : HandleErrorAttribute
    {
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            Exception Error = filterContext.Exception;
            //string Message = Error.Message;//错误信息
            string Url = HttpContext.Current.Request.RawUrl;//错误发生地址

            BugLog.Write(Error.Message + "\n\t" + Url);

            filterContext.ExceptionHandled = true;
            filterContext.Result = new RedirectResult("/Report/Error/");
            //跳转至错误提示页面

        }
    }

    #endregion
}