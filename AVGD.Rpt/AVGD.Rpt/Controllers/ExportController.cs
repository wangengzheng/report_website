using System;
using System.IO;
using System.Web.Mvc;
# if NPOI
using NPOI.HSSF.UserModel;
using NPOI.HPSF;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
# endif
using System.Configuration;
using System.Data;

using AVGD.Rpt;
using AVGD.Rpt.Models;
using AVGD.Rpt.DAL;
using AVGD.Common;
using Aspose.Cells;
using System.Web;

namespace AVGD.Rpt.Controllers
{
    [MyAuthorize]
    public class ExportController : Controller
    {
        private DbReport _db = new DbReport();
		# if NPOI

        HSSFWorkbook hssfworkbook;

		#endif
        #region  ---- 导出 execl 文件 ----
        /// <summary>
        /// NPOI版本
        /// </summary>
        /// <param name="pagelist"></param>
        /// <returns></returns>
        [GZipOrDeflate]
        public ActionResult Execl(PageList pagelist)
        {
            string downKey = (pagelist.report + pagelist.sort + pagelist.title + pagelist.order + pagelist.limitvalue + pagelist.sql).GetMD5String();
            if (Session["downKey"] != null)
            {
                if (downKey == Session["downKey"].ToString())
                {
                    pagelist.sql = Session["downSql"].ToString();
                    Session.Remove("downSql");
                    Session.Remove("downKey");
                }
            }
            else
            {
                BugLog.Write("downKey为空 导出数据出错！");
                return Content("no Key to DownLoad");
            }



			DataTable T;
			ExportDALController commond = new ExportDALController(_db);
			T = commond.GetDataTable(pagelist.sql);

			string exportFileName = HttpContext.Server.MapPath("~/ExportTemp/") + pagelist.title + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmsssfff") + System.Guid.NewGuid() + ".xls";

			OutFileToDisk(T, pagelist.title, string.Empty,exportFileName);

			Action<string> deleteFile = path =>
			{
				try
				{
					var filePath = path;
					System.Threading.Thread.Sleep(TimeSpan.FromSeconds(20.0d));
					if (System.IO.File.Exists(filePath))
					{
						System.IO.File.Delete(filePath);
					}
				}
				catch (Exception ex)
				{
					BugLog.Write("删除 xls 文件出错！" + ex.ToString());
				}
			};
			deleteFile.BeginInvoke(exportFileName, null, null);
			//开线程 清除文件

			return File(exportFileName, "application/octet-stream", string.Concat(pagelist.title, "_", DateTime.Now.ToLongDateString(), ".xls"));
			//return File(exportFileName, "application/vnd.ms-excel",pagelist.title+".xls");
			//InitializeWorkbook();
			//GenerateData(pagelist);
			//return File(WriteToStream().GetBuffer(), "application/vnd.ms-excel", pagelist.title + DateTime.Now.ToLongDateString() + ".xls");
        }

		#if NPOI

        #region  将 sheet 表用文件流的方式输出  WriteToStream
        MemoryStream WriteToStream()
        {
            //Write the stream data of workbook to the root directory
            MemoryStream file = new MemoryStream();
            hssfworkbook.Write(file);
            return file;
        }
        #endregion

        #region 初始化 sheet 表  GenerateData


        void GenerateData(PageList pagelist)
        {
            ISheet sheet1 = hssfworkbook.CreateSheet(pagelist.title);
            //sheet1.DefaultColumnWidth = 30 * 256;
            //sheet1.DefaultRowHeight = 20 * 20;
			/*
            sheet1.CreateRow(0).CreateCell(0).SetCellValue("AVGD");
			*/
            read(pagelist, sheet1);
        }
        #endregion

        #region 将数据写进去 sheet 表中  read
        void read(PageList pagelist, ISheet sheet1)
        {
            DataTable T = null;
            string subtotal = string.Empty;
            #region  当前pagelist.total 是否为空  获取   string[] columnname
            string[] columnname = !string.IsNullOrWhiteSpace(pagelist.total) ? pagelist.total.toStringArray() : null;
            #endregion
            ExportDALController commond = new ExportDALController(_db);
            T = commond.GetDataTable(pagelist.sql);

            if (T != null)
            {
				/*
                if (!string.IsNullOrWhiteSpace(pagelist.total))
                {
                    #region 获取统计字段 (多字段 统计)

                    string sum = columnname.sumField();

                    pagelist.sql = string.Format("select {0} from ({1}) xiaoji", sum, pagelist.sql);
                    DataTable sumTable = commond.GetDataTable(pagelist.sql);
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (DataRow item in sumTable.Rows)
                    {
                        foreach (var name in columnname)
                        {
                            sb.Append(item[name] + ",");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        subtotal = sb.ToString().TrimEnd(',');
                    }
                    #endregion
                }
				 */
                //当前表的列名数组
                IRow lieming = sheet1.CreateRow(0);
                for (int i = 0; i <= T.Columns.Count - 1; i++)
                {
                    lieming.CreateCell(i).SetCellValue(T.Columns[i].ToString());
                }
                try
                {
                    HSSFPalette palette = hssfworkbook.GetCustomPalette();
                    //palette.SetColorAtIndex(HSSFColor.Pink.Index, (byte)220, (byte)235, (byte)250);
                    ICellStyle style1 = hssfworkbook.CreateCellStyle();
                    style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Pink.Index;
                    style1.FillPattern = FillPattern.SolidForeground;
                    #region 自定义颜色  webconfig
                    bool backgroudIsDefind = ConfigurationManager.AppSettings["backgroudIsDefind"].ToLower() == "yes" ? true : false;
                    int red = 0, green = 0, blue = 0;
                    if (backgroudIsDefind)
                    {
                        red = int.Parse(ConfigurationManager.AppSettings["red"]);
                        green = int.Parse(ConfigurationManager.AppSettings["green"]);
                        blue = int.Parse(ConfigurationManager.AppSettings["blue"]);
                        palette.SetColorAtIndex(HSSFColor.Pink.Index, (byte)red, (byte)green, (byte)blue);
                    }
                    #endregion

                    foreach (System.Data.DataRow rows in T.Rows)
                    {
                        int rownumber = T.Rows.IndexOf(rows);
                        IRow row = sheet1.CreateRow(rownumber + 1);  //行
                        foreach (System.Data.DataColumn columns in T.Columns) //列
                        {
                            int cellnumber = T.Columns.IndexOf(columns);
                            ICell cell1 = row.CreateCell(cellnumber);
                            //row.CreateCell(cellnumber).SetCellValue(T.Rows[rownumber].ItemArray[cellnumber].ToString());
                            if (rownumber % 2 == 0)
                            {
                                if (backgroudIsDefind)
                                {
                                    cell1.CellStyle = style1;
                                }
                            }
                            cell1.SetCellValue(T.Rows[rownumber].ItemArray[cellnumber].ToString());
                        }
                    }

					/*
                    //添加求和数据到最后行中
                    if (!string.IsNullOrWhiteSpace(pagelist.total))
                    {
                        IRow lastrow = sheet1.CreateRow(sheet1.LastRowNum + 1);
                        lastrow.CreateCell(0).SetCellValue("统计字段:");
                        IRow lastrow1 = sheet1.CreateRow(sheet1.LastRowNum + 1);
                        lastrow1.CreateCell(0).SetCellValue("合计:");
                        string[] totalname = pagelist.total.toStringArray();
                        for (int i = 0; i < totalname.Length; i++)
                        {
                            lastrow.CreateCell(i + 1).SetCellValue(totalname[i]);
                            lastrow1.CreateCell(i + 1).SetCellValue(subtotal.toStringArray()[i]);
                        }
                    }
					*/
                }
                catch (Exception ex)
                {
                    BugLog.Write(ex.ToString());
                }

            }

        }
        #endregion


		#region 初始化 execl 的信息   InitializeWorkbook
		void InitializeWorkbook()
		{
			hssfworkbook = new HSSFWorkbook();

			////create a entry of DocumentSummaryInformation
			DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
			dsi.Company = "AV-GD";

			hssfworkbook.DocumentSummaryInformation = dsi;

			////create a entry of SummaryInformation
			SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
			si.Subject = "报表统计";
			si.Author = "中山市艾文凯迪投资管理有限公司";
			si.CreateDateTime = DateTime.Now;
			si.Comments = "公司文件泄露必究";
			hssfworkbook.SummaryInformation = si;
		}
		#endregion

#endif


		#region 导出数据
		/// <summary> 
		/// 导出数据到本地 
		/// </summary> 
		/// <param name="dt">要导出的数据</param> 
		/// <param name="tableName">表格标题</param> 
		/// <param name="path">保存路径</param> 
		public void OutFileToDisk(DataTable dt, string tableName, string path, string exportFileName)
		{
			Workbook workbook = new Workbook(); //工作簿 
			Worksheet sheet = workbook.Worksheets[0]; //工作表 
			Cells cells = sheet.Cells;//单元格 

			//为标题设置样式     
			Aspose.Cells.Style styleTitle = workbook.Styles[workbook.Styles.Add()];//新增样式 
			styleTitle.HorizontalAlignment = TextAlignmentType.Center;//文字居中 
			styleTitle.Font.Name = "宋体";//文字字体 
			styleTitle.Font.Size = 18;//文字大小 
			styleTitle.Font.IsBold = true;//粗体 

			//样式2 
			Aspose.Cells.Style style2 = workbook.Styles[workbook.Styles.Add()];//新增样式 
			style2.HorizontalAlignment = TextAlignmentType.Center;//文字居中 
			style2.Font.Name = "宋体";//文字字体 
			style2.Font.Size = 14;//文字大小 
			style2.Font.IsBold = true;//粗体 
			style2.IsTextWrapped = true;//单元格内容自动换行 
			style2.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
			style2.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
			style2.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
			style2.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;

			//样式3 
			Aspose.Cells.Style style3 = workbook.Styles[workbook.Styles.Add()];//新增样式 
			style3.HorizontalAlignment = TextAlignmentType.Center;//文字居中 
			style3.Font.Name = "宋体";//文字字体 
			style3.Font.Size = 12;//文字大小 
			style3.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
			style3.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
			style3.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
			style3.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;

			int Colnum = dt.Columns.Count;//表格列数 
			int Rownum = dt.Rows.Count;//表格行数 

			//生成行1 标题行    
			cells.Merge(0, 0, 1, Colnum);//合并单元格 
			cells[0, 0].PutValue(tableName);//填写内容 
			cells[0, 0].SetStyle(styleTitle);
			cells.SetRowHeight(0, 38);

			//生成行2 列名行 
			for (int i = 0; i < Colnum; i++)
			{
				cells[1, i].PutValue(dt.Columns[i].ColumnName);
				cells[1, i].SetStyle(style2);
				cells.SetRowHeight(1, 25);
			}

			//生成数据行 
			for (int i = 0; i < Rownum; i++)
			{
				for (int k = 0; k < Colnum; k++)
				{
					cells[2 + i, k].PutValue(dt.Rows[i][k].ToString());
					cells[2 + i, k].SetStyle(style3);
				}
				cells.SetRowHeight(2 + i, 24);
			}

			//string exportFileName = HttpContext.Server.MapPath("~/ExportTemp/") + tableName + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmsssfff") + System.Guid.NewGuid() + ".xls";
			workbook.Save(exportFileName);
			FileInfo fi = new FileInfo(exportFileName);
			if (fi.Exists)
			{
				//BugLog.Write("yes");
				//HttpContext.Response.Clear();
				//HttpContext.Response.ClearContent();
				//HttpContext.Response.ClearHeaders();
				//HttpContext.Response.AddHeader("Content-Disposition", "attachment;filename=" + fi.Name);
				//HttpContext.Response.AddHeader("Content-Length", fi.Length.ToString());
				//HttpContext.Response.AddHeader("Content-Transfer-Encoding", "binary");
				//HttpContext.Response.ContentType = "application/octet-stream";
				//HttpContext.Response.ContentEncoding = System.Text.Encoding.GetEncoding("gb2312");
				//HttpContext.Response.WriteFile(fi.FullName);
				//HttpContext.Response.Flush();
				//HttpContext.Response.End();
				//fi.Delete();
			}
		}

		#endregion

        #region 获取下载 url

        [HttpPost, AjaxOnly, ValidateAntiForgeryToken]
        public ActionResult download(PageList pagelist)
        {
            string sqlkey = Guid.NewGuid().ToString().Replace("-", "");
            string downKey = (pagelist.report + pagelist.sort + pagelist.title + pagelist.order + pagelist.limitvalue + sqlkey).GetMD5String();
            Session["downKey"] = downKey;

            #region 获取 sql
            ExportDALController commond = new ExportDALController(_db);
            pagelist.sql = commond.GetSqlValue(pagelist.report, isFillter: true);/*TODO: isFillter:true download*/
            if (!string.IsNullOrWhiteSpace(pagelist.sort))
            {
                if (!string.IsNullOrWhiteSpace(pagelist.order))
                {
                    pagelist.sql = string.Format(" {0} order by {1} {2}", pagelist.sql, pagelist.sql.GetFieldSqlByName(pagelist.sort), pagelist.order);
                }
            }
            Session["downSql"] = pagelist.sql;
            #endregion

            string url = "/Export/Execl?report={0}&sort={1}&order={2}&title={3}&total={4}&limitvalue={5}&sql={6}";
            url = string.Format(url, pagelist.report, pagelist.sort, pagelist.order, pagelist.title, pagelist.total, pagelist.limitvalue, sqlkey);

            return Content(url);
        }

        #endregion

        #endregion
    }
}
