using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AVGD.Common
{
    /// <summary>
    /// web.config操作类
    /// </summary>
    public sealed class ConfigHelper
    {
        /// <summary>
        /// 得到AppSettings中的配置字符串信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetConfigString(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 得到AppSettings中的配置bool信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool GetConfigBool(string key)
        {
            bool result = false;
            string cfgVal = ConfigHelper.GetConfigString(key);
            if (cfgVal != null && string.Empty != cfgVal)
            {
                try
                {
                    result = bool.Parse(cfgVal);
                }
                catch (FormatException)
                {
                }
            }
            return result;
        }

        /// <summary>
        /// 得到AppSettings中的配置decimal信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static decimal GetConfigDecimal(string key)
        {
            decimal result = 0m;
            string cfgVal = ConfigHelper.GetConfigString(key);
            if (cfgVal != null && string.Empty != cfgVal)
            {
                try
                {
                    result = decimal.Parse(cfgVal);
                }
                catch (FormatException)
                {
                }
            }
            return result;
        }

        /// <summary>
        /// 得到AppSettings中的配置int信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static int GetConfigInt(string key)
        {
            int result = 0;
            string cfgVal = ConfigHelper.GetConfigString(key);
            if (cfgVal != null && string.Empty != cfgVal)
            {
                try
                {
                    result = int.Parse(cfgVal);
                }
                catch (FormatException)
                {
                }
            }
            return result;
        }

        /// <summary>
        /// 得到ConnectionStrings中的连接字符串
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ToString();
        }

        #region ---- 字段 ----

        public static string RptAppKey = GetConfigString("RptAppKey");
        public static string RptAppSecret = GetConfigString("RptAppSecret");
        public static string SendMessageUrl = GetConfigString("SendMessageUrl");

        #endregion
    }
}