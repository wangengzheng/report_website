using System.Web;

namespace AVGD.Common
{
    public static class Cookies
    {
        public static void Set(string cookieName, string cookieValue, System.DateTime? cookieTime)
        {
            Cookies.Set(cookieName, cookieValue, cookieTime, null, null);
        }

        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="cookieName">Cookie Name</param>
        /// <param name="cookieValue">Cookie Value</param>
        public static void Set(string cookieName, string cookieValue)
        {
            Cookies.Set(cookieName, cookieValue, new System.DateTime?(System.DateTime.MaxValue));
        }

        public static void Set(string cookieName, string cookieValue, System.DateTime? cookieTime, string domain, string path)
        {
            if (Cookies.Get(cookieName).Length > 0)
            {
                Cookies.Remove(cookieName);
            }
            HttpCookie httpCookie = new HttpCookie(cookieName)
            {
                Value = cookieValue
            };
            if (domain != null)
            {
                httpCookie.Domain = domain;
            }
            if (path != null)
            {
                httpCookie.Path = path;
            }
            if (cookieTime.HasValue)
            {
                httpCookie.Expires = cookieTime.Value;
            }
            //TODO:  设置 sql 总条数为 httpOnly 属性为 true
            if (cookieName.IndexOf("Count") >= 0)
            {
                httpCookie.HttpOnly = true;
            }
            HttpContext.Current.Response.Cookies.Add(httpCookie);
        }

        public static void Set(string cookieName, string cookieValue, string subCookieName, string subCookieValue, System.DateTime? cookieTime, string domain)
        {
            if (cookieValue != null)
            {
                HttpCookie httpCookie = new HttpCookie(cookieName)
                {
                    Value = cookieValue
                };
                httpCookie[subCookieName] = subCookieValue;
                if (cookieTime.HasValue)
                {
                    httpCookie.Expires = cookieTime.Value;
                }
                if (domain != null)
                {
                    httpCookie.Domain = domain;
                }
                HttpContext.Current.Response.Cookies.Add(httpCookie);
            }
        }

        /// <summary>
        /// 获取Cookie
        /// </summary>
        /// <param name="cookieName">Cookie Name</param>
        /// <returns></returns>
        public static string Get(string cookieName)
        {
            return Cookies.Get(cookieName, null, null);
        }

        public static string Get(string cookieName, string domain, string path)
        {
            HttpCookie httpCookie = HttpContext.Current.Request.Cookies[cookieName];
            if (domain != null && httpCookie != null)
            {
                httpCookie.Domain = domain;
            }
            if (path != null && httpCookie != null)
            {
                httpCookie.Path = path;
            }
            return (httpCookie == null) ? "" : httpCookie.Value;
        }

        public static string Get(string cookieName, string subCookieName, string domain, string path)
        {
            string text = "";
            if (HttpContext.Current.Request.Cookies != null)
            {
                HttpCookie httpCookie = HttpContext.Current.Request.Cookies[cookieName];
                if (null == httpCookie)
                {
                    text = "";
                }
                else
                {
                    if (domain != null)
                    {
                        httpCookie.Domain = domain;
                    }
                    if (path != null)
                    {
                        httpCookie.Path = path;
                    }
                    text = httpCookie.Value;
                    string[] array = text.Split(new char[]
					{
						'&'
					});
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        string text2 = array2[i];
                        if (text2.IndexOf(subCookieName + "=") >= 0)
                        {
                            text = text2.Split(new char[]
							{
								'='
							})[1];
                        }
                    }
                }
            }
            return text;
        }

        /// <summary>
        /// 移除Cookie
        /// </summary>
        /// <param name="cookieName">Cookie Name</param>
        public static void Remove(string cookieName)
        {
            Cookies.Remove(cookieName, null);
        }

        public static void Remove(string cookieName, string domain)
        {
            if (HttpContext.Current.Request.Cookies[cookieName] != null)
            {
                HttpCookie httpCookie = new HttpCookie(cookieName);
                httpCookie.Expires = System.DateTime.Now.AddDays(-1.0);
                if (domain != null)
                {
                    httpCookie.Domain = domain;
                }
                HttpContext.Current.Response.Cookies.Add(httpCookie);
            }
        }
    }
}
