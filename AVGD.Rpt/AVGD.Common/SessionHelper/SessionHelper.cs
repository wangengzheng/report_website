using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AVGD.Common
{
    public sealed class SessionHelper
    {
        public static string GetValue(string str)
        {
            return HttpContext.Current.Session[str].ToString();
        }

        public static void SetValue(string name, string value)
        {
            if (name.IsNotEmpty() && value.IsNotEmpty()) HttpContext.Current.Session[name] = value;
        }

        public static void RemoveValue(string key)
        {
            if (HttpContext.Current.Session[key] != null)
            {
                HttpContext.Current.Session.Remove(key);
            }
        }

        #region ---- sqlValueManager ----
        /// <summary>
        /// 获取sqlValue
        /// </summary>
        /// <returns></returns>
        public static string GetSqlValue()
        {
            if (HttpContext.Current.Session["SqlValue"] != null)
            {
                return HttpContext.Current.Session["SqlValue"].ToString();
            }
            return "";
        }
        /// <summary>
        /// 设置sqlValue
        /// </summary>
        /// <param name="value"></param>
        public static void SetSqlValue(string value)
        {
            if (value.IsNotEmpty()) HttpContext.Current.Session["SqlValue"] = value;
        }

        /// <summary>
        /// 移除SqlValue
        /// </summary>
        public static void RestSqlValue()
        {
            if (GetSqlValue().IsNotEmpty()) HttpContext.Current.Session.Remove("SqlValue");
        }
        #endregion

        #region ----- totalname Manager -----

        public static string GetTotalName()
        {
            if (HttpContext.Current.Session["totalname"] != null)
            {
                return HttpContext.Current.Session["totalname"].ToString();
            }
            return "";
        }


        public static void SetTotalName(string name)
        {
            HttpContext.Current.Session["totalname"] = name;        
        }


        public static void RestTotalName()
        {

            if (GetTotalName().IsNotEmpty()) HttpContext.Current.Session.Remove("totalname");
        }

        #endregion
    }
}