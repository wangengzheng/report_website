using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVGD.Rpt.Models
{
    [Table("rpt_category")]
    [Serializable]
    public class rpt_category
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 类别名
        /// </summary>
        public string Catrgoryname { get; set; }


        //public virtual ICollection<rpt_categorydetail> rpt_categorydetails { get; set; }
    }


    #region sql
    //    CREATE TABLE `rpt_category` (
    //  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '主键',
    //  `Catrgoryname` varchar(30) NOT NULL COMMENT '类别名',
    //  PRIMARY KEY (`Id`)
    //) ENGINE=InnoDB DEFAULT CHARSET=utf8;
    #endregion


}