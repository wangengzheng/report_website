using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVGD.Rpt.Models
{
    [Table("rpt_mobilecode")]
    [Serializable]
    public class rpt_mobilecode
    {
        /// <summary>
        /// 主键
        /// </summary>
       [Key]
        public int Id { get; set; }
        /// <summary>
        /// 手机验证码
        /// </summary>
        public string Validatecode { get; set; }
        /// <summary>
        /// 添加时间（何时）
        /// </summary>
        public DateTime Adddatetime { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime Expirdatetime { get; set; }
        /// <summary>
        /// 手机号码（何人）
        /// </summary>
        public string Phonenumber { get; set; }
        /// <summary>
        /// 登录Ip（何地）
        /// </summary>
        public string Loginip { get; set; }
        /// <summary>
        /// 验证码错误次数（大于5次验证码失败）
        /// </summary>
        public int Errortime { get; set; }


         
    }
    #region sql
//    CREATE TABLE `rpt_mobilecode` (
//  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '主键',
//  `Validatecode` varchar(10) DEFAULT NULL COMMENT '手机验证码',
//  `Adddatetime` datetime DEFAULT NULL COMMENT '添加时间（何时）',
//  `Expirdatetime` datetime DEFAULT NULL COMMENT '过期时间',
//  `Phonenumber` varchar(20) DEFAULT NULL COMMENT '手机号码（何人）',
//  `Loginip` varchar(20) DEFAULT NULL COMMENT '登录Ip（何地）',
//  `Errortime` int(11) DEFAULT '0' COMMENT '验证码错误次数（大于5次验证码失败）',
//  PRIMARY KEY (`Id`)
//) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;
    #endregion
}
