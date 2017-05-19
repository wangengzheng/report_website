using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVGD.Rpt.Models
{
    [Table("rpt_column")]
    [Serializable]
    public class rpt_column
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// rpt_categorydetail主键
        /// </summary>
        public string Columnreport { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string Columnname { get; set; }
        /// <summary>
        /// 列名对应的sql
        /// </summary>
        public string Columnvalue { get; set; }
    }
    #region sql
//    CREATE TABLE `rpt_column` (
//  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '主键',
//  `Columnreport` varchar(20) DEFAULT NULL COMMENT 'rpt_categorydetail 主键',
//  `Columnname` varchar(20) DEFAULT NULL COMMENT '列名',
//  `Columnvalue` text COMMENT '列名对应的sql',
//  PRIMARY KEY (`Id`)
//) ENGINE=InnoDB DEFAULT CHARSET=utf8;
    #endregion
}