using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AVGD.Rpt.ViewModel
{
    public class ETaoPhoto
    {
        /// <summary>
        /// 身份证 正面
        /// </summary>
        public string F_idCard { get; set; }

        /// <summary>
        /// 身份证 背面
        /// </summary>
        public string B_idCard { get; set; }

        /// <summary>
        /// 营业执照
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// 店铺照片1
        /// </summary>
        public string Store_1 { get; set; }

        public string Store_2 { get; set; }

        public string Store_3 { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public string  authenticId { get; set; }
    }
}