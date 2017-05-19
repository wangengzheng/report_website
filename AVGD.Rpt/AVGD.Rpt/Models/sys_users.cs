using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVGD.Rpt.Models
{
    /// <summary>
    /// 系统用户和经销商表
    /// </summary>
    [Table("sys_users")]
    [Serializable]
    public class sys_users
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        public string seqid { get; set; }
        /// <summary>
        /// 编码Unique
        /// </summary>
        public string userlogno { get; set; }
        /// <summary>
        /// MD5
        /// </summary>
        public string userpassword { get; set; }
        /// <summary>
        /// 1：管理员 2：经销商 3：操作员	
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 父级
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string TrueName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 移动号码
        /// </summary>
        public string Telephone { get; set; }
        /// <summary>
        /// 公司名
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string headImage { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        public string registertime { get; set; }
        /// <summary>
        /// 编辑人
        /// </summary>
        public string EditBy { get; set; }
        /// <summary>
        /// 编辑日期
        /// </summary>
        public string EditTime { get; set; }
        /// <summary>
        /// 有效期
        /// </summary>
        public string ExpireDate { get; set; }
        /// <summary>
        /// 1：待审核 2：审核通过 3：审核失败 4：禁用
        /// </summary>
        public Nullable<int> Status { get; set; }
        /// <summary>
        ///	企业门户网址
        /// </summary>
        public string enterpriseportal { get; set; }
        /// <summary>
        /// 用户品牌，可存放多个品牌，用”|”符号分隔，该品牌只能由管理员指派
        /// </summary>
        public string user_brand { get; set; }
        /// <summary>
        /// 明细地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 所在城市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 1：可创建，0：不可创建品牌
        /// </summary>
        public Nullable<int> crebrand { get; set; }
        /// <summary>
        /// 1：可创建，0：不可创建产品
        /// </summary>
        public Nullable<int> creproduct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string storeId { get; set; }
        /// <summary>
        /// 用户融云token
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 发送轨迹记录时间间隔
        /// </summary>
        public Nullable<decimal> timeInterval { get; set; }
        /// <summary>
        /// 用户在EDS商城的折扣率
        /// </summary>
        public Nullable<decimal> discount { get; set; }
    }
}