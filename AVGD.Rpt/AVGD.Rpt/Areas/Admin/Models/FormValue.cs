using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AVGD.Rpt.Areas.Admin.Models
{
    public class FormValue
    {
        /// <summary>
        /// 报表SQL语句
        /// </summary>
        public string sqlvalue { get; set; }

        /// <summary>
        /// 统计字段
        /// </summary>
        public string totalcolumn { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string sortcolumn { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string sorttype { get; set; }

        /// <summary>
        /// report值
        /// </summary>
        public string report { get; set; }

        /// <summary>
        /// 报表名
        /// </summary>
        public string reportname { get; set; }


        /// <summary>
        /// 类别Id
        /// </summary>
        public int categoryId { get; set; }
    }

    public class SetupFormValue
    {
        /// <summary>
        /// report值
        /// </summary>
        public string report { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string reportcolumn { get; set; }
        /// <summary>
        /// 列名值
        /// </summary>
        public string reportvalue { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }
    }

    public class Category
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// 报表类型名
        /// </summary>
        public string CategoryName { get; set; }
    }
}