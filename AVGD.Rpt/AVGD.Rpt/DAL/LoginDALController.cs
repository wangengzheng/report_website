using AVGD.Rpt.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AVGD.Rpt.DAL
{
    public class LoginDALController
    {
        public DbReport _db = null;

        public LoginDALController(DbReport db)
        {
            _db = db;
        }

        #region ---- 检验用户 ----
        public sys_users CheckUser(string account)
        {
            return _db.sys_user.Where(sys => sys.userlogno == account && sys.type == 1).FirstOrDefault();
        }
        #endregion

        #region ---- 检验用户是否三十分钟内有发送过短信 ----

        public bool checkUserSendPhoneCode30After(string phonenumber)
        {
            bool isExit = false;
            //select * from rpt_mobilecode where phoneNumber=?phoneNumber and date_add(NOW(), INTERVAL -1 HOUR) <addDateTime and DATE_ADD(NOW(),INTERVAL 1 HOUR) >addDateTime and NOW()<expirDateTime ORDER BY addDateTime DESC LIMIT 1
            DateTime begin = DateTime.Now.AddHours(-2.0);
            DateTime now = DateTime.Now;
            DateTime end = DateTime.Now.AddDays(1.0);
            rpt_mobilecode rpt_mobilecode = _db.rpt_mobilecode.Where(mobile => mobile.Phonenumber == phonenumber && mobile.Adddatetime > begin && mobile.Adddatetime < end && mobile.Expirdatetime > now).FirstOrDefault();
            if (rpt_mobilecode != null)
            {
                isExit = true;
            }
            return isExit;
        }
        #endregion

        #region ---- 新增用户手机的验证码 ----
        /// <summary>
        /// 新增用户手机的验证码
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="PhoneCode"></param>
        /// <param name="thisUrl"></param>
        /// <returns></returns>
        public int InsertPhoneCode(string phoneNumber, string PhoneCode, string thisIp)
        {
            rpt_mobilecode rpt_mobilecode = new rpt_mobilecode();
            rpt_mobilecode.Adddatetime = DateTime.Now;
            rpt_mobilecode.Phonenumber = phoneNumber;
            rpt_mobilecode.Validatecode = PhoneCode;
            rpt_mobilecode.Loginip = thisIp;
            rpt_mobilecode.Expirdatetime = DateTime.Now.AddHours(2.0);
            _db.rpt_mobilecode.Add(rpt_mobilecode);
            return _db.SaveChanges();
        }
        #endregion

        #region  ---- 验证验证码是否过期 是否尝试次数过对（最多5次） 如果验证码错误  则更新errorTime+1 ----
        /// <summary>
        /// 验证验证码是否过期 是否尝试次数过对（最多5次） 如果验证码错误  则更新errorTime+1
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="PhoneCode"></param>
        /// <returns></returns>
        public bool CkeckPhoneCode(string phoneNumber, string PhoneCode, ref int Time)
        {
            bool isExit = false;
            DateTime now = DateTime.Now;
            rpt_mobilecode rpt_mobilecode = _db.rpt_mobilecode.Where(mobile => mobile.Expirdatetime > now && mobile.Phonenumber == phoneNumber && mobile.Errortime < 5).FirstOrDefault();
            if (rpt_mobilecode != null)
            {
                string code = rpt_mobilecode.Validatecode;
                if (code == PhoneCode)
                {
                    isExit = true;
                }
                else
                {
                    _db.rpt_mobilecode.Attach(rpt_mobilecode);
                    rpt_mobilecode.Errortime = rpt_mobilecode.Errortime + 1;
                    Time = rpt_mobilecode.Errortime;
                    _db.SaveChanges();
                }
            }
            return isExit;
        }
        #endregion

        #region  ---- 删除用户手机的验证码 ----
        /// <summary>
        /// 删除用户手机的验证码
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public int DeleteVlidateCode(string phoneNumber)
        {
            rpt_mobilecode rpt_mobilecode = _db.rpt_mobilecode.Where(mobile => mobile.Phonenumber == phoneNumber).OrderByDescending(mobile => mobile.Adddatetime).FirstOrDefault();
            if (rpt_mobilecode != null)
            {
                _db.rpt_mobilecode.Remove(rpt_mobilecode);
                return _db.SaveChanges();
            }
            return 0;
        }
        #endregion
    }
}