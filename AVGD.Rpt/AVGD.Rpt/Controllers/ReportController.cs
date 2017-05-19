using AVGD.Common;
using AVGD.Rpt.DAL;
using AVGD.Rpt.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace AVGD.Rpt.Controllers
{
    [MyAuthorize]
    public class ReportController : Controller
    {
        private DbReport _db = new DbReport();

        #region ---- 报表结果显示页面 ----

        //[GZipOrDeflate]
        public ActionResult Category(string report, string title)
        {
            if (report.IsEmpty()) return Redirect("/Report/Index");
            CommondController commond = new CommondController(_db);
            rpt_categorydetail categoryDetail = commond.GetCategoryDetail(report);
            string sqlValue = null;
            if (HttpContext.Request.QueryString["CustomQuery"] != null)
            {
                sqlValue = SessionHelper.GetSqlValue().IsEmpty() ? categoryDetail.Sqlvalue : SessionHelper.GetSqlValue();
            }
            if (HttpContext.Request.QueryString["RestSetUp"] == null && HttpContext.Request.QueryString["CustomQuery"] == null)
            {
                SessionHelper.RestSqlValue();
                SessionHelper.RestTotalName();
            }

            if (categoryDetail == null)
            {
                BugLog.Write("report=------"+report);
                throw new ArgumentException("报表类别为空  请联系管理员;");
            }
            DataTable T = commond.GetDataTableOneRow(sqlValue??categoryDetail.Sqlvalue);
            #region T 不为空的时候
            if (T != null && T.Rows.Count > 0)
            {
                var CName = T.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
                var CType = T.Columns.Cast<DataColumn>().Select(x => x.DataType.FullName).ToArray();
                
                ViewBag.ColumnName = CName;
                ViewBag.ColumnType = CType;
                ViewBag.Total = categoryDetail.Total.IsEmpty() ? "" : categoryDetail.Total;
                ViewBag.zdString = string.Join(",", CName);
                ViewBag.lxString = string.Join(",", CType);
                ViewBag.排序字段 = categoryDetail.Sort.IsEmpty() ? CName[0] : CName.Contains(categoryDetail.Sort) ? categoryDetail.Sort : CName[0];
                ViewBag.排序方式 = categoryDetail.Order.IsEmpty() ? "desc" : categoryDetail.Order;
                ViewBag.不显示的类型 = "System.TimeSpan,System.Byte[]";
                ViewBag.Title = categoryDetail.Detailedname ?? title;
                ViewBag.report = categoryDetail.Id;

            }
            #endregion
            return View();
        }

        #region ---- 操作符部分视图 ----

        public ActionResult Operatorpartialview(string selectname)
        {
            ViewBag.selectname = selectname;
            return PartialView("_Operator");
        }

        #endregion

        #region ---- 报表首页 顶部部分视图 ----
        public PartialViewResult Partailveiw(string title, string report)
        {
            ViewBag.report = report;
            var viewName = string.Empty;

            if (title.IsEmpty())
            {
                throw new ArgumentNullException("title 不能为空");
            }

            viewName = "_" + title;
            if (title == "易掌管订单" || title == "财务部统计报表")
            {
                viewName = "_易掌管订单";
            }
            //解决耦合
            return PartialView(viewName:viewName);
        }
        #endregion

        #region ---- 报表首页 高级查询功能 ----
        /// <summary>
        /// 改版 2.0  2016-02-27
        /// </summary>
        /// <author>郑万庚</author>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, MYSQLInject]
        public ActionResult AdvancedQuery()
        {
            //高级查询 重新查询所有 （重要）
            string typeInt = "System.Int32System.Int64System.UInt64System.Int16System.IntSystem.DecimalSystem.SingleSystem.DoubleSystem.SByteSystem.Decima";
            string report = Request.Form["report"];

            string[] ziduan = Request.Form["ziduan"].Split(new char[] { ',' });
            string[] leixing = Request.Form["leixing"].Split(new char[] { ',' });
            Dictionary<string, FormValue> dic = new Dictionary<string, FormValue>();
            int i = 0;
            foreach (var item in ziduan)
            {
                FormValue fv = new FormValue { name = ziduan[i], DateExit = true, value = Request.Form[ziduan[i]] };

                if (leixing[i] == "System.DateTime")
                {
                    #region System.DateTime
                    if (Request.Form[ziduan[i]].Trim() == string.Empty)
                        fv.DateExit = false;

                    if ((Request.Form[ziduan[i] + ziduan[i]]).Trim() == string.Empty)
                    {
                        fv.SecondData = false;
                    }
                    else
                    {
                        fv.SecondData = true;
                        string date = Request.Form[ziduan[i] + ziduan[i]];
                        fv.maxDataTime = DateTime.Parse(date).AddDays(1).ToString();
                    }
                    #endregion
                }
                else if (typeInt.Contains(leixing[i]))
                {
                    //存在运算符
                    if (Request.Form[ziduan[i]].Trim() == string.Empty)
                        fv.DateExit = false;
                    else
                    {
                        fv.operatorstr = Request.Form[ziduan[i] + "selectname"];
                    }

                }
                else
                {
                    if (Request.Form[ziduan[i]].Trim() == string.Empty)
                        fv.DateExit = false;
                }

                dic.Add(leixing[i] + i.ToString(), fv);
                i++;
            }
            CommondController commond = new CommondController(_db);
            string sql = commond.GetSqlValue(report, isFillter: false); /*TODO: isFillter:false AdvancedQuery*/

            if (sql.IsNotEmpty())
            {
                //old method
                //处理sql拼接
                //sqlString = GetSqlValue(sql, dic, Request.Form["title"]); //getSqlByDict(sql, dic);
                //var count=commond.GetCount(sqlString);
                //if (0 == count)
                //return Content("no");
                ////保存当前多条件查询的字符串
                //Session["SqlValue"] = sqlString;
                //return Content("ok");

                MYSQLInit sqlInit = new MYSQLInit();
                SqlInjectMethod(sql, dic, sqlInit);
                var listcount = commond.GetCount(sql + sqlInit.GetCurrentSQL(), sqlInit.GetCurrentPara());
                if (0 == listcount)
                {
                    return Content("no");
                }
                else
                {
                    Session["SqlValue"] = GetSqlValue(sql, dic, Request.Form["title"]);
                    return Content("ok");
                }
            }
            else {
                return Content("no");
            }        

        }


        #region ---- 拼接sql字典（高级查询）-----

        /// <summary>
        /// 解决注入 sql 攻击  高级搜索文本输入
        /// <autor>郑万庚</autor>
        /// </summary>
        /// <param name="sqlValue"></param>
        /// <param name="dic"></param>
        /// <param name="init"></param>
        private void SqlInjectMethod(string sqlValue, Dictionary<string, FormValue> dic,MYSQLInit init)
        {
            FormValue formValue = null;
            string[] intStr = { "System.Single", "System.Double", "System.SByte", "System.Int32", "System.Int64", "System.UInt64", "System.Int16", "System.Int", "System.Decimal", "System.Single", "System.Double" };
            string stringStr = "System.String";
            string dateStr = "System.DateTime";
            foreach (KeyValuePair<string, FormValue> item in dic)
            {
                var sqlField = "";
                formValue = item.Value;
                string key = item.Key;
                #region 时间
                if (key.Contains(dateStr))
                {
                    sqlField = formValue.DateExit ? sqlValue.GetFieldSqlByName(formValue.name) : (formValue.SecondData ? sqlValue.GetFieldSqlByName(formValue.name) : formValue.name);
                    if (formValue.DateExit && formValue.SecondData)
                    {
                        init.And(sqlField + " >", formValue.value);
                        init.And(sqlField + " <", formValue.maxDataTime);
                    }
                    else
                    {
                        if (formValue.DateExit)
                        {
                            init.And(sqlField + " >", formValue.value);
                        }
                        else if (formValue.SecondData)
                        {
                            init.And(sqlField + " <", formValue.maxDataTime);
                        }
                    }
                    continue;
                }
                #endregion

                #region 字符串
                if (key.Contains(stringStr))
                {
                    sqlField = formValue.DateExit ? sqlValue.GetFieldSqlByName(formValue.name) : formValue.name;
                    if (formValue.DateExit)
                    {
                        init.And(sqlField + " like", "%" + formValue.value + "%");
                    }
                    continue;
                }
                #endregion

                #region 数字
                if (intStr.InArray(key))
                {
                    sqlField = formValue.DateExit ? sqlValue.GetFieldSqlByName(formValue.name) : formValue.name;
                    if (formValue.DateExit)
                    {
                        if (formValue.operatorstr.IsNotEmpty())
                        {
                            init.And(sqlField + formValue.operatorstr, formValue.value);
                        }
                        else
                        {
                            init.And(sqlField + " like", "%" + formValue.value + "%");
                        }
                    }
                    continue;
                }
                #endregion
            }

              if(sqlValue.IndexOf("where", StringComparison.OrdinalIgnoreCase) < 0)
              {
                  if (init.Builder.Length > 0)
                  {
                      if (init.Builder.ToString().IndexOf("and", StringComparison.OrdinalIgnoreCase) > -1)
                      {
                          //trimStart and
                          init.Builder.Remove(init.Builder.ToString().IndexOf("and", StringComparison.OrdinalIgnoreCase), 3).Insert(0, " where ");
                      }
                  }
              }
        }


        private string GetSqlValue(string sqlValue, Dictionary<string, FormValue> dic, string title)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            FormValue formValue = null;
            string[] intStr = { "System.Single", "System.Double", "System.SByte", "System.Int32", "System.Int64", "System.UInt64", "System.Int16", "System.Int", "System.Decimal", "System.Single", "System.Double" };
            string stringStr = "System.String";
            string dateStr = "System.DateTime";
            foreach (KeyValuePair<string, FormValue> item in dic)
            {
                var sqlField = "";
                formValue = item.Value;
                string key = item.Key;
                #region 时间
                if (key.Contains(dateStr))
                {
                    sqlField = formValue.DateExit ? sqlValue.GetFieldSqlByName(formValue.name) : (formValue.SecondData ? sqlValue.GetFieldSqlByName(formValue.name) : formValue.name);
                    if (formValue.DateExit && formValue.SecondData)
                    {
                        sb.AppendFormat(" and {0} > '{1}' and {0} <'{2}' ", sqlField, formValue.value, formValue.maxDataTime);
                    }
                    else
                    {
                        if (formValue.DateExit)
                        {
                            sb.AppendFormat(" and {0} > '{1}' ", sqlField, formValue.value);
                        }
                        else if (formValue.SecondData)
                        {
                            sb.AppendFormat(" and  {0} <'{1}' ", sqlField, formValue.maxDataTime);
                        }
                    }
                    continue;
                }
                #endregion

                #region 字符串
                if (key.Contains(stringStr))
                {
                    sqlField = formValue.DateExit ? sqlValue.GetFieldSqlByName(formValue.name) : formValue.name;
                    if (formValue.DateExit)
                    {
                        //if (formValue.name == "订单状态" || formValue.name == "支付类型")
                        //{
                        //    //这个方法 当我另外添加一个 case when then 的时候 有耦合现象  所以直接取值就行了的   //bug
                        //    sb.AppendFormat(" and {0} in ({1}) ", sqlField, sqlValue.GetFieldValueByValue(formValue.name, formValue.value));
                        //    continue;
                        //}
                        sb.AppendFormat(" and {0} like '%{1}%' ", sqlField, formValue.value);
                    }
                    continue;
                }
                #endregion

                #region 数字
                if (intStr.InArray(key))
                {
                    sqlField = formValue.DateExit ? sqlValue.GetFieldSqlByName(formValue.name) : formValue.name;
                    if (formValue.DateExit)
                    {
                        if (formValue.operatorstr.IsNotEmpty())
                        {
                            sb.AppendFormat(" and {0}  {1}  {2} ", sqlField, formValue.operatorstr, formValue.value);
                        }
                        else
                        {
                            sb.AppendFormat(" and {0}  like  '%{1}%' ", sqlField, formValue.value);
                        }
                    }
                    continue;
                }
                #endregion
            }
            string sbStr = sb.ToString();
            if (sbStr.IsEmpty())
                return sqlValue;
            return sqlValue = sqlValue.IndexOf("where", StringComparison.OrdinalIgnoreCase) > -1
                    ?
                    sqlValue + sbStr
                    :
                    sqlValue + sbStr.Substring(sbStr.IndexOf("and", StringComparison.OrdinalIgnoreCase)+3).Insert(0, " where ");
        }

        /// <summary>
        /// 将字典条件 拼接成sql语句
        /// </summary>
        /// <param name="sql">原先的sql语句</param>
        /// https://msdn.microsoft.com/zh-cn/library/xfhwa508(v=vs.110).aspx
        /// <returns>sql语句</returns>
        private string getSqlByDict(string sql, Dictionary<string, FormValue> dic)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            bool isFirst = true;
            int index = 0;
            string tablename = "tablename" + Guid.NewGuid().ToString().Replace("-", "");
            foreach (KeyValuePair<string, FormValue> key in dic)
            {

                sb = SplicingSQL(key.Key, sql, key, sb, isFirst, tablename, index.ToString());
                isFirst = false;
                index++;
            }
            return sb.ToString();
        }

        /// <summary>
        ///   拼接sql语句
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <param name="sql">原先的sql语句</param>
        /// <param name="keyvalue">条件的名字和对应名的值</param>
        /// <param name="stringbulider">原先System.Text.StringBuilder 的值</param>
        ///<param name="index">index为当前字典的索引、（自己设置的） 从0开始</param>
        /// <returns>System.Text.StringBuilder 新的值</returns>
        private System.Text.StringBuilder SplicingSQL(string type, string sql, KeyValuePair<string, FormValue> keyvalue, System.Text.StringBuilder stringbulider, bool isFirst, string tablename, string index)
        {
            string valueint = string.Format("System.Single{0}{1}System.Double{0}{1}System.SByte{0}{1}System.Int32{0}{1}System.Int64{0}{1}System.UInt64{0}{1}System.Int16{0}{1}System.Int{0}{1}System.Decimal{0}{1}System.Single{0}{1}System.Double{0}{1}", index, ",");
            string valuestring = "System.String" + index;
            string valuedate = "System.DateTime" + index;
            System.Text.StringBuilder sb;
            sb = stringbulider;
            bool hasData = keyvalue.Value.DateExit;

            if (valueint.Contains(type))
            {  ////////////////// //int
                #region int
                if (isFirst)
                {                         //第一次添加添加条件的处理哦
                    if (hasData)  //有数据的处理
                    {
                        #region 操作符是否存在
                        if (keyvalue.Value.operatorstr != "")
                        {
                            sb = sb.AppendFormat(" and  a{0}.{1}{2}{3} {4} {5}  ", tablename, "`", keyvalue.Value.name, "`", keyvalue.Value.operatorstr, keyvalue.Value.value);
                        }
                        else
                        {
                            sb = sb.AppendFormat("select *  from ({0}) a{5}   where  a{5}.{3}{1}{4} like \"%{2}%\"  ", sql, keyvalue.Value.name, keyvalue.Value.value, "`", "`", tablename);
                        }
                        #endregion
                    }
                    else
                        sb = sb.AppendFormat("select * from ({0}) a{1}  where 1=1 ", sql, tablename);
                }
                else
                {
                    if (hasData)  //有数据的处理
                        if (keyvalue.Value.operatorstr != "")
                        {
                            sb = sb.AppendFormat(" and  a{0}.{1}{2}{3} {4} {5}  ", tablename, "`", keyvalue.Value.name, "`", keyvalue.Value.operatorstr, keyvalue.Value.value);
                        }
                        else
                            sb = sb.AppendFormat(" and  a{0}.{3}{1}{4} like \"%{2}%\"  ", tablename, keyvalue.Value.name, keyvalue.Value.value, "`", "`");

                }
                #endregion
            }
            else if (valuestring.Contains(type))
            { /////////////////string
                #region string
                if (isFirst)   //第一次添加添加条件的处理哦
                {
                    if (hasData)    //有数据的处理
                        sb = sb.AppendFormat("select *  from ({0}) a{5}   where  a{5}.{3}{1}{4} like \"%{2}%\"  ", sql, keyvalue.Value.name, keyvalue.Value.value, "`", "`", tablename);
                    else
                        sb = sb.AppendFormat("select * from ({0}) a{1} where 1=1 ", sql, tablename);
                }
                else
                {
                    if (hasData)  //有数据的处理
                        sb = sb.AppendFormat(" and  a{0}.{3}{1}{4} like \"%{2}%\"   ", tablename, keyvalue.Value.name, keyvalue.Value.value, "`", "`");
                }
                #endregion
            }
            else if (valuedate.Contains(type))
            {  /////////////////date
                #region date
                if (isFirst)    //第一次添加添加条件的处理哦
                {
                    if (hasData && keyvalue.Value.SecondData)
                    {
                        sb = sb.AppendFormat("select *  from ({0}) a{6}   where  a{6}.{4}{1}{5} > \"{2}\" and a{6}.{4}{1}{5} <\"{3}\"  ", sql, keyvalue.Value.name, keyvalue.Value.value, keyvalue.Value.maxDataTime, "`", "`", tablename);
                    }
                    else
                    {
                        if (hasData)   //有数据的处理
                            sb = sb.AppendFormat("select *  from ({0}) a{5}   where  a{5}.{3}{1}{4}>\"{2}\"  ", sql, keyvalue.Value.name, keyvalue.Value.value, "`", "`", tablename);
                        else
                            sb = sb.AppendFormat("select *  from ({0}) a{5}   where  a{5}.{3}{1}{4}<\"{2}\"  ", sql, keyvalue.Value.name, keyvalue.Value.maxDataTime, "`", "`", tablename);
                        //sb = sb.AppendFormat("select * from ({0}) a{1} where 1=1 ", sql, tablename);
                    }

                }
                else
                {
                    if (hasData && keyvalue.Value.SecondData)
                    {
                        sb = sb.AppendFormat("and  a{5}.{3}{0}{4}>\"{1}\" and a{5}.{3}{0}{4}<\"{2}\" ", keyvalue.Value.name, keyvalue.Value.value, keyvalue.Value.maxDataTime, "`", "`", tablename);
                    }
                    else
                    {
                        if (hasData)
                            sb = sb.AppendFormat("and  a{4}.{2}{0}{3}>\"{1}\"  ", keyvalue.Value.name, keyvalue.Value.value, "`", "`", tablename);

                        if (keyvalue.Value.SecondData)
                            sb = sb.AppendFormat("and  a{4}.{2}{0}{3}<\"{1}\"  ", keyvalue.Value.name, keyvalue.Value.maxDataTime, "`".ToString(), "`", tablename);

                    }

                }
                #endregion
            }
            return sb;
        }

        #endregion

        #endregion

        #region ---- 报表首页  底部统计信息 ----

        #region ---- 底部统计信息 本页小计 ----

        public ActionResult PageTotal([Bind(Include = "report")]PageList pageList)
        {
            return TotalMethod(pageList, TotalType.PageTotal);
        }

        #endregion

        #region ---- 底部统计信息 本表小计 ----

        public ActionResult TableTotal([Bind(Include = "report")]PageList pageList)
        {
            return TotalMethod(pageList, TotalType.TableTotal);
        }

        #endregion

        private enum TotalType
        {
            PageTotal,
            TableTotal
        }

        private ActionResult TotalMethod(PageList pageList, TotalType totalTypes)
        {
            if (pageList.report.IsEmpty()) return Content("参数不能为空");

            CommondController commond = new CommondController(_db);
            rpt_categorydetail categoryDetail = commond.GetCategoryDetail(pageList.report);
            if (categoryDetail == null) return Content("参数出错");
            string sqlValue = string.Empty;
            if (totalTypes == TotalType.PageTotal)
            {
                sqlValue = Session["LimitSqlValue"].ToString() ?? "";
            }
            else if (totalTypes == TotalType.TableTotal)
            {
                sqlValue = commond.GetSqlValue(pageList.report, isFillter: true); /*TODO: isFillter:true TotalMethod*/
            }
            else
            {
                return Content("错误的请求类型");
            }

            if (sqlValue.IsNotEmpty())
            {
                string orderCountSql = commond.GetOrderCountSqlValue(sqlValue);

                pageList.total = categoryDetail.Total;
                string[] totalList = categoryDetail.Total.Split(',');
                ViewBag.columnname = totalList;
                string sum = totalList.sumField();
                sqlValue = string.Format("select {0} from ({1}) xiaoji", sum, sqlValue);

                DataSet ds = commond.GetDataSet(orderCountSql + ";" + sqlValue);

                return PartialView("_PartialTotal", ds);
            }

            return Content("sql语句为空");
        }

        #endregion

        #region ---- 报表首页 顶部搜索查询功能 ----
        [HttpPost, ValidateAntiForgeryToken, MYSQLInject]
        public ActionResult SimpleQuery2()
        {
            string startwhere = string.Empty;
            string report = Request.Form["report"];

            CommondController commond = new CommondController(_db);
            string sqlValue = commond.GetSqlValue(report, isFillter: false); /*TODO: isFillter:false SimpleQuery*/
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string[] keys = Request.Form.AllKeys;
            try
            {
                #region 遍历表单值  排除report 跟订单状态
                foreach (string name in keys)
                {
                    if ("report" == name || "订单状态" == name || "__RequestVerificationToken" == name)
                    {
                        continue;
                    }
                    if (name.Contains("日期1") && Request.Form[name].IsNotEmpty())
                    {
                        var value = sqlValue.GetFieldSqlByName(name.Substring(0, name.Length - 1));
                        sb.AppendFormat(" and {0} > '{1}' ", value, Request.Form[name]);
                        continue;
                    }
                    if (name.Contains("日期2") && Request.Form[name].IsNotEmpty())
                    {
                        DateTime endTime = DateTime.Parse(Request.Form[name]).AddDays(1);
                        var dateStr = endTime.ToString("yyyy-MM-dd");
                        var value = sqlValue.GetFieldSqlByName(name.Substring(0, name.Length - 1));
                        sb.AppendFormat(" and {0} < '{1}' ", value, dateStr);
                        continue;
                    }
                    if (Request.Form[name].IsNotEmpty())
                    {
                        var value = sqlValue.GetFieldSqlByName(name);
                        sb.AppendFormat(" and {0} like '%{1}%' ", value, Request.Form[name]);
                    }
                }
                #endregion
                #region 遍历订单状态
                if (Request.Form["订单状态"].IsNotEmpty())   // keys.toStringMergeChar(',').Contains("订单状态")  
                {
                    string[] status = Request.Form["订单状态"].toStringArray();
                    var value = sqlValue.GetFieldSqlByName("订单状态");
                    sb.AppendFormat(" and {0}  in (", value);
                    for (int i = 0; i < status.Length; i++)
                    {
                        sb.AppendFormat("'{0}',", status[i]);
                    }
                    startwhere = sb.ToString().TrimEnd(',');
                    startwhere += ")";
                }
                if (startwhere.IsEmpty())
                {
                    startwhere = sb.ToString();
                }
                if (sb.ToString().IsEmpty())
                {
                    Session["SqlValue"] = sqlValue;
                    return Content("ok");
                }
                #endregion
                sqlValue = sqlValue.IndexOf("where", StringComparison.OrdinalIgnoreCase) > -1
                    ?
                    sqlValue + startwhere
                    :
                    sqlValue + startwhere.Substring(startwhere.IndexOf(" and", StringComparison.OrdinalIgnoreCase)+" and".Length).Insert(0, " where ");

                int rowEf = commond.GetCount(sqlValue);
                if (0 == rowEf)
                {
                    return Content("no");
                }
            }
            catch (Exception ex)
            {
                BugLog.Write(ex.ToString());
                return Content("error");
            }
            Session["SqlValue"] = sqlValue;
            return Content("ok");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SimpleQuery()
        {
            string startwhere = string.Empty;
            string report = Request.Form["report"];

            CommondController commond = new CommondController(_db);
            string sqlValue = commond.GetSqlValue(report, isFillter: false); /*TODO: isFillter:false SimpleQuery*/
            if (sqlValue.IsEmpty())
            {
                return Content("no");
            }
            string[] keys = Request.Form.AllKeys;
            MYSQLInit init = new MYSQLInit();
            try
            {
                SimpleSqlInjectMethod(init, sqlValue, keys);

                int rowEf = commond.GetCount(sqlValue+init.GetCurrentSQL(),init.GetCurrentPara());
                if (0 == rowEf)
                {
                    return Content("no");
                }
            }
            catch (Exception ex)
            {
                BugLog.Write(ex.ToString());
                return Content("error");
            }

            Session["SqlValue"] = GetSimpleSql(report);
            return Content("ok");
        }

        private string GetSimpleSql(string report)
        {
            string startwhere = string.Empty;
            CommondController commond = new CommondController(_db);
            string sqlValue = commond.GetSqlValue(report, isFillter: false); /*TODO: isFillter:false SimpleQuery*/
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string[] keys = Request.Form.AllKeys;
            #region 遍历表单值  排除report 跟订单状态
            foreach (string name in keys)
            {
                if ("report" == name || "订单状态" == name || "__RequestVerificationToken" == name)
                {
                    continue;
                }
                if (name.Contains("日期1") && Request.Form[name].IsNotEmpty())
                {
                    var value = sqlValue.GetFieldSqlByName(name.Substring(0, name.Length - 1));
                    sb.AppendFormat(" and {0} > '{1}' ", value, Request.Form[name]);
                    continue;
                }
                if (name.Contains("日期2") && Request.Form[name].IsNotEmpty())
                {
                    DateTime endTime = DateTime.Parse(Request.Form[name]).AddDays(1);
                    var dateStr = endTime.ToString("yyyy-MM-dd");
                    var value = sqlValue.GetFieldSqlByName(name.Substring(0, name.Length - 1));
                    sb.AppendFormat(" and {0} < '{1}' ", value, dateStr);
                    continue;
                }
                if (Request.Form[name].IsNotEmpty())
                {
                    var value = sqlValue.GetFieldSqlByName(name);
                    sb.AppendFormat(" and {0} like '%{1}%' ", value, Request.Form[name]);
                }
            }
            #endregion
            #region 遍历订单状态
            if (Request.Form["订单状态"].IsNotEmpty())   // keys.toStringMergeChar(',').Contains("订单状态")  
            {
                string[] status = Request.Form["订单状态"].toStringArray();
                var value = sqlValue.GetFieldSqlByName("订单状态");
                sb.AppendFormat(" and {0}  in (", value);
                for (int i = 0; i < status.Length; i++)
                {
                    sb.AppendFormat("'{0}',", status[i]);
                }
                startwhere = sb.ToString().TrimEnd(',');
                startwhere += ")";
            }
            if (startwhere.IsEmpty())
            {
                startwhere = sb.ToString();
            }
            #endregion
            sqlValue = sqlValue.IndexOf("where", StringComparison.OrdinalIgnoreCase) > -1
                ?
                sqlValue + startwhere
                :
                sqlValue + startwhere.Substring(startwhere.IndexOf(" and", StringComparison.OrdinalIgnoreCase) + " and".Length).Insert(0, " where ");

            return sqlValue;
        
        }

        private void SimpleSqlInjectMethod(MYSQLInit init,string sqlValue,string[] keys)
        {

            #region 遍历表单值  排除report 跟订单状态
            foreach (string name in keys)
            {
                if ("report" == name || "订单状态" == name || "__RequestVerificationToken" == name)
                {
                    continue;
                }
                if (name.Contains("日期1") && Request.Form[name].IsNotEmpty())
                {
                    var value = sqlValue.GetFieldSqlByName(name.Substring(0, name.Length - 1));
                    init.And(value + ">", Request.Form[name]);
                    continue;
                }
                if (name.Contains("日期2") && Request.Form[name].IsNotEmpty())
                {
                    DateTime endTime = DateTime.Parse(Request.Form[name]).AddDays(1);
                    var dateStr = endTime.ToString("yyyy-MM-dd");
                    var value = sqlValue.GetFieldSqlByName(name.Substring(0, name.Length - 1));
                    init.And(value + "<", dateStr);
                    continue;
                }
                if (Request.Form[name].IsNotEmpty())
                {
                    var value = sqlValue.GetFieldSqlByName(name);
                    init.And(value + " like ", "%" + Request.Form[name] + "%");
                }
            }
            #endregion
            #region 遍历订单状态

            if (Request.Form["订单状态"].IsNotEmpty())   // keys.toStringMergeChar(',').Contains("订单状态")  
            {
                var listValue = Request.Form["订单状态"].toStringArray();
                var value = sqlValue.GetFieldSqlByName("订单状态");
                int beginIndex = 500;
                init.Builder.AppendFormat(" and {0} in (",value);
                foreach (var item in listValue)
                {
                    var indexStr = (beginIndex++).ToString();
                    init.Builder.Append("?Para" + indexStr + ",");
                    init.ParaList.Add(new MySqlParameter("?Para"+indexStr,item));
                }
                init.Builder.Length = init.Builder.Length - 1;  // trim end , (去掉最后的 逗号)
                init.Builder.Append(")");
            }

            #endregion

            if (sqlValue.IndexOf("where", StringComparison.OrdinalIgnoreCase) < 0)
            {
                if (init.Builder.Length > 0)
                {
                    if (init.Builder.ToString().IndexOf("and", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        //trimStart and
                        init.Builder.Remove(init.Builder.ToString().IndexOf("and", StringComparison.OrdinalIgnoreCase), 3).Insert(0, " where ");
                    }
                }
            }
        }

        //private ActionResult SimpleQuery1()
        //{
        //    string startwhere = string.Empty;
        //    string report = Request.Form["report"];
        //    int paraIndex = 0;
        //    List<MySqlParameter> paraList = new List<MySqlParameter>();
        //    string paraName = string.Empty;
        //    string field = string.Empty;            
        //    CommondController commond = new CommondController(_db);
        //    string sqlValue = commond.GetSqlValue(report, isFillter: false); /*TODO: isFillter:false SimpleQuery*/
        //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //    string[] keys = Request.Form.AllKeys;
        //    try
        //    {
        //        #region 遍历表单值  排除report 跟订单状态
        //        foreach (string name in keys)
        //        {
        //            if ("report" == name || "订单状态" == name || "__RequestVerificationToken" == name)
        //            {
        //                continue;
        //            }
        //            if (name.Contains("日期1") && Request.Form[name].IsNotEmpty())
        //            {
        //                field = sqlValue.GetFieldSqlByName(name.Substring(0, name.Length - 1));
        //                paraName = GetParaName(paraIndex++);
        //                paraList.Add(SetParaValue<string>(paraName,Request.Form[name],MySqlDbType.DateTime));
        //                sb.AppendFormat(" and {0} > {1} ", field, paraName);
        //                continue;
        //            }
        //            if (name.Contains("日期2") && Request.Form[name].IsNotEmpty())
        //            {
        //                DateTime endTime = DateTime.Parse(Request.Form[name]).AddDays(1);
        //                var dateStr = endTime.ToString("yyyy-MM-dd");
        //                field = sqlValue.GetFieldSqlByName(name.Substring(0, name.Length - 1));
        //                paraName = GetParaName(paraIndex++);
        //                paraList.Add(SetParaValue<string>(paraName, dateStr, MySqlDbType.DateTime));
        //                sb.AppendFormat(" and {0} < {1} ", field, paraName);
        //                continue;
        //            }
        //            if (Request.Form[name].IsNotEmpty())
        //            {
        //                field = sqlValue.GetFieldSqlByName(name);
        //                paraName = GetParaName(paraIndex++);
        //                paraList.Add(SetParaValue<string>(paraName, "%" + Request.Form[name] + "%", MySqlDbType.String));
        //                sb.AppendFormat(" and {0} like {1} ", field, paraName);
        //            }
        //        }
        //        #endregion
        //        #region 遍历订单状态
        //        if (Request.Form["订单状态"].IsNotEmpty())   // keys.toStringMergeChar(',').Contains("订单状态")  
        //        {
        //            string[] status = Request.Form["订单状态"].toStringArray();
        //            var value = sqlValue.GetFieldSqlByName("订单状态");
        //            sb.AppendFormat(" and {0}  in (", value);
        //            for (int i = 0; i < status.Length; i++)
        //            {
        //                sb.AppendFormat("'{0}',", status[i]);
        //            }
        //            startwhere = sb.ToString().TrimEnd(',');
        //            startwhere += ")";
        //        }
        //        if (startwhere.IsEmpty())
        //        {
        //            startwhere = sb.ToString();
        //        }
        //        #endregion
        //        sqlValue = sqlValue.IndexOf("where", StringComparison.OrdinalIgnoreCase) > -1
        //            ?
        //            sqlValue + startwhere
        //            :
        //            sqlValue + startwhere.Substring(startwhere.IndexOf(" and", StringComparison.OrdinalIgnoreCase)).Insert(0, " where ");

        //        int rowEf = commond.GetCount(sqlValue,paraList.ToArray());
        //        if (0 == rowEf)
        //        {
        //            return Content("no");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        BugLog.Write(ex.ToString());
        //        return Content("error");
        //    }
        //    Session["SqlValue"] = sqlValue;
        //    return Content("ok");
        //}
        #endregion

        #region ---- 报表首页 设置功能 ----


        public ActionResult SetUpQuery()
        {
            string[] keys = Request.Form.AllKeys;
            if (keys.Length == 3)
            {
                return Content("nochange");
            }
            #region 处理本页统计 本表统计  避免设置后新的字段不在本页统计里面而报错  存在session["totalname"] 里面
            if (!string.IsNullOrWhiteSpace(Request.Form["setuptotalname"]))
            {
                System.Text.StringBuilder resultcolumnname = new System.Text.StringBuilder();
                string[] setuptotalname = Request.Form["setuptotalname"].toStringArray();
                foreach (string column in setuptotalname)
                {
                    foreach (var formKey in keys)
                    {
                        if (column == formKey)
                        {
                            resultcolumnname.AppendFormat("{0},", column);
                            break;
                        }
                    }
                }
                if (resultcolumnname.ToString().IsNotEmpty())
                {
                    Session["totalname"] = resultcolumnname.ToString().TrimEnd(',');
                }
                else
                {
                    Session["totalname"] = string.Empty;
                }
            }
            #endregion

            #region  当 session 不为空的时候
            if (Session["SqlValue"] != null)
            {
                string tablename = "tablename" + Guid.NewGuid().ToString().Replace("-", "");
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string startfrom = string.Empty;
                foreach (var item in keys)
                {
                    if (item != "setupreport" && item != "startfrom" && item != "setuptotalname")
                    {
                        sb.Append(tablename + "." + item + ",");
                    }

                }
                string sqlValue = string.Format("select {0}  from ({1}){2}", sb.ToString().TrimEnd(','), Session["SqlValue"].ToString(), tablename);
                Session["SqlValue"] = sqlValue;
                return Content("ok");
            }
            #endregion

            CommondController commond = new CommondController(_db);
            List<rpt_column> rptColumn = commond.GetRptColumnEntity(Request.Form["setupreport"]);

            #region 当 rptColumn 不为 null 长度大于0

            if (rptColumn != null && rptColumn.Count() > 0)
            {
                string startfrom = string.Empty;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var item in keys)
                {
                    if (item != "setupreport" && item != "setuptotalname")
                    {
                        if ("startfrom" == item)
                        {
                            startfrom = rptColumn.Where(clo => clo.Columnname == "startfrom").Select(clo => clo.Columnvalue).FirstOrDefault();
                            continue;
                        }
                        sb.AppendFormat(" {0} ,", rptColumn.Where(clo => clo.Columnname == item).Select(clo => clo.Columnvalue).FirstOrDefault());
                    }
                }
                if (sb.ToString() == string.Empty)
                {
                    sb.Append("*");
                }
                string sqlValue = string.Format("select {0} from {1}", sb.ToString().TrimEnd(','), startfrom);
                Session["SqlValue"] = sqlValue;
                return Content("ok");
            }
            else
            {
                return Content("nothing");
            }

            #endregion

        }

        public ActionResult RestSetUp(string code)
        {
            if ("81aa9b921d18c926cdb196067124d213" == code)
            {
                SessionHelper.RestSqlValue();
                SessionHelper.RestTotalName();
                return Content("ok");
            }
            return Content("no");
        }

        #endregion


 
        public void ClearCache()
        {
            List<string> strPath = new List<string>();
            strPath.Add(AppDomain.CurrentDomain.BaseDirectory + "SysLog\\Cache_rpt_categorydetails.log");
            strPath.Add(AppDomain.CurrentDomain.BaseDirectory + "SysLog\\Cache_rpt_column.log");
            foreach (var path in strPath)
            {
                BugLog.Write("清除 缓存" + path, path);
            }
            Response.Redirect("/Report/Index",true);
          //  Session.Clear();
        }

        #endregion

        #region ---- EasyUi 前端获取 JSON 格式 ----
        //[GZipOrDeflate]
        public ActionResult getJsonFromReport(PageList pagelist)
        {
            int total = 0;
            CommondController commond = new CommondController(_db);
            string sqlValue = commond.GetSqlValue(pagelist.report, isFillter: true);/*TODO:isFiller:true getJsonFromReport*/
            List<Dictionary<string, string>> rows = null;
            if (sqlValue.IsNotEmpty())
            {
                pagelist.sql = sqlValue;
                total = commond.GetCount(sqlValue);
                rows = commond.GetJSON(LimitDataTable(pagelist));
            }
            return Json(new { total = total, rows = rows }, JsonRequestBehavior.AllowGet);
        }

        public DataTable LimitDataTable(PageList pagelist)
        {
            DataTable T = null;
            string sql = string.Empty;
            if (pagelist.sql.IsNotEmpty())
            {
                pagelist.page = pagelist.page < 1 ? 1 : pagelist.page;
                pagelist.rows = pagelist.rows < 10 ? 20 : pagelist.rows;

                if (pagelist.sort.IsNotEmpty())
                {
                    if (pagelist.order.IsEmpty()) pagelist.order = "desc";
                    //if (pagelist.sql.Contains("order by"))
                    //{
                    //    pagelist.sql = pagelist.sql.Substring(0, pagelist.sql.IndexOf("order by"));
                    //}
                    sql = string.Format(" {0}  order by  {1}  {2}  limit {3},{4}", pagelist.sql, pagelist.sort, pagelist.order, ((pagelist.page - 1) * pagelist.rows).ToString(), pagelist.rows.ToString());
                }
                else
                {
                    sql = pagelist.sql + " limit  " + ((pagelist.page - 1) * pagelist.rows).ToString() + "," + pagelist.rows.ToString();
                }
                pagelist.limitvalue = sql;
                Session["LimitSqlValue"] = sql;
                CommondController commond = new CommondController(_db);
                T = commond.GetDataTable(sql);
            }
            return T;

        }

        #endregion

        #region ---- 报表统计 入口 ----

        public ActionResult Index()
        {
            SessionHelper.RestSqlValue();
            SessionHelper.RestTotalName();
            return View();
        }

        #endregion

        #region ---- 报表统计 错误页面 ----
        public ActionResult Error()
        {
            return View();
        }
        #endregion
    }
}

//http://www.mysqltutorial.org/tryit/query/mysql-select/#2