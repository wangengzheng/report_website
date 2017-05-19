using AVGD.Common;
using AVGD.Rpt.DAL;
using AVGD.Rpt.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AVGD.Rpt.Controllers
{
    public class LoginController : Controller
    {
        private DbReport _db = new DbReport();

        #region ---- 登录界面 ----

        [HttpGet]
        public ActionResult Index()
        {		
			Session["User"] = "10086";
			return Redirect("/Report/Index");

			System.Web.Configuration.SessionStateSection sessionSection = (System.Web.Configuration.SessionStateSection)System.Configuration.ConfigurationManager.GetSection("system.web/sessionState");
			if (Request.Cookies[sessionSection.CookieName] != null)
			{
				var sessionCookie = Request.Cookies[sessionSection.CookieName];
				sessionCookie.Expires = DateTime.Now.AddDays(-1.0D);
				Response.Cookies.Add(sessionCookie);
			}
			Session.Clear();
			

            AVGD.Common.SessionHelper.RestSqlValue();


            return View();
        }

        public static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }  


        #region ---- 用户登录post   第一步验证账号跟密码 ----
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Index(FormCollection form)
        {
            var account = form["account"];
            var password = form["password"];
            var validatecode = form["validatecode"];

            #region 检验验证码
            if (!string.IsNullOrWhiteSpace(validatecode))
            {
                if (Session["ValidateCode"] != null)
                {
                    if (Session["ValidateCode"].ToString() != validatecode)
                    {
                        Session.Clear();
                        return Json(new
                        {
                            code = 1,
                            message = "请输入正确的验证码!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        code = 1,
                        message = "验证码失效!"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    code = 1,
                    message = "请输入验证码!"
                });
            }

            #endregion

            #region 检验用户
            if (account.IsNotEmpty())
            {
                if (password.IsNotEmpty())
                {
                    string dbpassword = string.Empty;
                    LoginDALController loginDAL = new LoginDALController(_db);
                    sys_users sysUser = loginDAL.CheckUser(account);
                    if (sysUser != null)
                    {
                        dbpassword = sysUser.userpassword;
                        dbpassword = (validatecode + ("avgd_rpt.edsmall.cn" + dbpassword).GetMD5String()).GetMD5String();
                    }          
                    if (password != dbpassword)
                    {
                        return Json(new
                        {
                            code = 1,
                            message = "用户名密码错误!"
                        });
                    }
					#region ---- 跳过短信验证 ----
					//前端验证也需要改
					//Session["User"] = sysUser.Telephone;
					//return Json(new
					//{
					//	code = 0,
					//	url = "/Report/Index"
					//});
					#endregion

                    Session["loginUser"] = sysUser.Telephone;
                    Session.Remove("ValidateCode");
                    return Json(new
                    {
                        code = 0,
                        data = new
                        {
                            phoneNumber = sysUser.Telephone
                        }
                    });
                }
            }
            else
            {
                return Json(new
                {
                    code = 1,
                    message = "用户名不能为空!"
                });
            }
            #endregion

            return View();
        }
        #endregion

        #region  ---- 发送验证码到手机号码上  第二步发送短信到手机 ----
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult sendValidationCodeByPhone(string phonenumber, string __RequestVerificationToken, string account)
        {
            ////第一步  验证用户是否通过了 第一步验证
            if (Session["loginUser"] == null)
            {
                return Json(new
                {
                    code = 1,
                    message = "你还没有通过账号密码认证哦！一步一步来"
                });
            }
            else
            {
                if (Session["loginUser"].ToString() != phonenumber)
                {
                    return Json(new
                    {
                        code = 1,
                        message = "请使用正确的手机号码！"
                    });
                }
            }
            LoginDALController loginDAL = new LoginDALController(_db);
            //第二步 2.1
            if (!loginDAL.checkUserSendPhoneCode30After(phonenumber))
            {
                //第二步 2.2
                if (!sendCodeToPhone(phonenumber, loginDAL))
                {
                    //删除新增的数据
                    loginDAL.DeleteVlidateCode(phonenumber);
                    return Json(new
                    {
                        code = 1,
                        message = "短信发送失败，请联系管理员!"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    code = 1,
                    message = "2小时内验证码有效,耐心等候短信验证码!"
                });
            }

            return Json(new
            {
                code = 0,
                message = "发送短信成功"
            });
        }

        #region MyRegion
        private static readonly Random getrandom = new Random();
        public bool sendCodeToPhone(string phoneNumber, LoginDALController loginDAL)
        {
            bool sendOk = false;
            string phoneCode = getrandom.Next(100000, 999999).ToString();
            int row = loginDAL.InsertPhoneCode(phoneNumber, phoneCode, GetIp());
            if (row == 1)
            {
                #region 由云片网发送短信到手机上

                string RptAppKey = ConfigurationManager.AppSettings["RptAppKey"].ToString();
                string RptAppSecret = ConfigurationManager.AppSettings["RptAppSecret"].ToString();
                string SendMessageUrl = ConfigurationManager.AppSettings["SendMessageUrl"].ToString();
                string timestamp = GetTimeStamp();
                string innerJson = "{'appKey':'" + RptAppKey + "','timestamp':" + timestamp + ",'appSecret':'" + (RptAppSecret + timestamp).GetMD5String() + "','data':{'templateCode':'DLYZ','mobile':'" + phoneNumber + "','params':{'code':'" + phoneCode + "','minute':120}}}";
                YPResultSet result = ReqPost.PostSendMicroMessage(innerJson, SendMessageUrl);
                if (result.code == "0")
                {
                    sendOk = true;
                }
                else
                {
                    sendOk = false;
                }

                #endregion
            }
            return sendOk;
        }

        #region 获取用户ip
        private string GetIp()
        {
            string userIP = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userIP == null || userIP == "")
            {
                userIP = Request.ServerVariables["REMOTE_ADDR"];
            }
            if (!string.IsNullOrEmpty(userIP) && IsIP(userIP))
            {
                return userIP;
            }
            return "127.0.0.1";
        }
        public static bool IsIP(string ip) { return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$"); }
        #endregion

        #region 获取时间戳
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private string GetTimeStamp()
        {
            return ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000).ToString();
        }
        #endregion

        #endregion

        #endregion

        #region  ---- 用户手机认证登录  第三步验证手机认证 ----
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult phonelogin(FormCollection form)
        {
            var phonenumber = form["phonenumber"];
            var phonecode = form["phonecode"];
            var account = form["account"];
            if (Session["loginUser"] != null)
            {
                int errorTime = 0;
                #region 检验手机短信验证码
                LoginDALController loginDAL = new LoginDALController(_db);
                if (!loginDAL.CkeckPhoneCode(phonenumber, phonecode, ref errorTime))
                {
                    int residueTime = 5 - errorTime;
                    if (residueTime == 5)
                    {
                        return Json(new
                        {
                            code = 1,
                            message = "亲，你还没发送短信呢，别调皮哦！"
                        });
                    }
                    else if (residueTime == 0)
                    {
                        return Json(new
                        {
                            code = 1,
                            message = "手机验证码输入错误次数过多,请稍后再试！"
                        });

                    }
                    return Json(new
                    {
                        code = 1,
                        message = "手机验证码输入有误请重新再输! 输入错误次数还剩 " + residueTime + " 次"
                    });
                }
                #endregion

                Session.Remove("loginUser");
                Session["User"] = phonenumber;
                return Json(new
                {
                    code = 0,
                    url = "/Report/Index"
                });
            }
            else
            {
                return Json(new
                {
                    code = 1,
                    message = ""
                });
            }


        }
        #endregion

        #region ---- 获取验证码 ----

        public ActionResult GetValidateCode()
        {
            System.Threading.Thread.Sleep(10);
            if (base.Session["ValidateCode"] != null)
            {
                base.Session["ValidateCode"] = null;
            }
            ValidateCode vCode = new ValidateCode();
            string code = string.Empty;
            Bitmap bitmap = vCode.CreateImage(5, out code, ValidateCode.ValidateCodeType.Number);
            base.Session["ValidateCode"] = code;
            string cookieName = "validatecode";
            string mD5String = ("zhengwangeng" + code).GetMD5String();
            Cookies.Set(cookieName, mD5String, DateTime.Now.AddDays(1.0));
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);
            bitmap.Dispose();
            return base.File(stream.ToArray(), "image/jpeg");
        }
        #endregion

        #endregion

    }
}
