using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AVGD.Rpt.Models
{
    /// <summary>
    /// E家订单助手订单表
    /// </summary>
    [Table("ecash_orders")]
    public class ecash_orders
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        [Key]
        public string OrderId { get; set; }

        /// <summary>
        /// 订单流水号
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// 顾客ID
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// 导购ID
        /// </summary>
        public string SalerId { get; set; }

        /// <summary>
        /// 订单状态 0 未付款 1 已付款 2 订单处理中 3 已发货 4 交易完成 5 订单关闭
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime AddDate { get; set; }

        /// <summary>
        /// 订单确认时间
        /// </summary>
        public DateTime ConfirmDate { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// 订单完成时间
        /// </summary>
        public DateTime CompletionDate { get; set; }

        /// <summary>
        /// 结算定金支付日期
        /// </summary>
        public DateTime DepostsitDate { get; set; }

        /// <summary>
        /// 订单结算时间
        /// </summary>
        public DateTime SettleDate { get; set; }

        /// <summary>
        /// 订单关闭时间
        /// </summary>
        public DateTime CloseDate { get; set; }

        /// <summary>
        /// 定金
        /// </summary>
        public float FrontMoney { get; set; }

        /// <summary>
        /// 收货人
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// 固话
        /// </summary>
        public string Telephone { get; set; }

        /// <summary>
        /// 接收方式 0自提1快递2物流
        /// </summary>
        public int ReceiveWay { get; set; }

        /// <summary>
        /// 运输方式 POST-平邮,EXPRESS-其他快 递,EMS-EMS,DIRECT-无需物流
        /// </summary>
        public string TransportType { get; set; }

        /// <summary>
        /// 发货单号
        /// </summary>
        public string InvoiceNo { get; set; }

        /// <summary>
        /// 物流公司名称
        /// </summary>
        public string LogisticsName { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}