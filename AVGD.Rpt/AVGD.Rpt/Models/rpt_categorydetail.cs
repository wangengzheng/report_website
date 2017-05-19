using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVGD.Rpt.Models
{
    [Table("rpt_categorydetail")]
    [Serializable]
    public class rpt_categorydetail
    {
        /// <summary>
        /// 主键rport
        /// </summary>        
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        /// <summary>
        /// rpt_category 主键
        /// </summary>        
        //[ForeignKey("Cateoryid")]
        public int Cateoryid { get; set; }
        /// <summary>
        /// 报表名
        /// </summary>
        public string Detailedname { get; set; }
        /// <summary>
        /// sql语句
        /// </summary>
        public string Sqlvalue { get; set; }
        /// <summary>
        /// 求和的字段
        /// </summary>
        public string Total { get; set; }
        /// <summary>
        /// 排序的字段
        /// </summary>
        public string Sort { get; set; }
        /// <summary>
        /// 排序的方式  desc esc
        /// </summary>
        public string Order { get; set; }

        //public virtual rpt_category rpt_category { get; set; }
    }
    #region sql
//    CREATE TABLE `rpt_categorydetail` (
//  `Id` varchar(20) NOT NULL COMMENT '主键rport',
//  `Cateoryid` int(11) NOT NULL COMMENT 'rpt_category 主键',
//  `Detailedname` varchar(20) NOT NULL COMMENT '报表名',
//  `Sqlvalue` text NOT NULL COMMENT 'sql语句',
//  `Total` varchar(50) DEFAULT NULL COMMENT '求和的字段',
//  `Sort` varchar(20) DEFAULT NULL COMMENT '排序的字段',
//  `Order` varchar(10) DEFAULT NULL COMMENT '排序的方式  desc esc',
//  PRIMARY KEY (`Id`)
//) ENGINE=InnoDB DEFAULT CHARSET=utf8;

    #endregion
}