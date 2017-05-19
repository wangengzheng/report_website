using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AVGD.Rpt
{
    public class PageList
    {
        /// <summary>
        /// 当前的页数
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// 当前显示的行数
        /// </summary>
        public int rows { get; set; }

        /// <summary>
        /// 总记录数 /求个的字段
        /// </summary>
        public string total { get; set; }

        /// <summary>
        /// 当前查询的字符串
        /// </summary>
        public string sql { get; set; }

        /// <summary>
        /// report
        /// </summary>
        public string report { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 当前排序列字段名
        /// </summary>
        public string sort { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string order { get; set; }

        ///// <summary>
        ///// easyui fiter功能
        ///// </summary>
        //public string filterRules { get; set; }

        /// <summary>
        /// filter对应的值
        /// </summary>
        //public List<Condition> condition { get; set; }

        public string limitvalue { get; set; }
    }
}