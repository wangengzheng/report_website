using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AVGD.Rpt
{
    public class FormValue
    {
        /// <summary>
        /// 表单name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 表单name对应的值
        /// </summary>
        public string value { get; set; }

        /// <summary>
        ///数据是否存在  第一个   
        /// </summary>
        public bool DateExit { get; set; }

        /// <summary>
        /// 大的时间
        /// </summary>
        public string maxDataTime { get; set; }

        /// <summary>
        /// 数据是否存在   第二个
        /// </summary>
        public bool SecondData { get; set; }

        /// <summary>
        /// 运算符
        /// </summary>
        public string operatorstr { get; set; }
    }
}