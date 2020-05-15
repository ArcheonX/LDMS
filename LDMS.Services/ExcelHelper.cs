using System;
using System.Text;

namespace LDMS.Services
{
    public static class ExcelHelper
    {
        public const int rowLimit = 65000;

        private static string GetWorkbookTemplate()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<?xml version=\"1.0\"?>{0}", Environment.NewLine);
            sb.AppendFormat("<?mso-application progid=\"Excel.Sheet\"?>{0}", Environment.NewLine);
            sb.AppendFormat("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"{0}", Environment.NewLine);
            sb.AppendFormat(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"{0}", Environment.NewLine);
            sb.AppendFormat(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"{0}", Environment.NewLine);
            sb.AppendFormat(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"{0}", Environment.NewLine);
            sb.AppendFormat(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">{0}", Environment.NewLine);
            sb.AppendFormat(" <Styles>{0}", Environment.NewLine);
            sb.AppendFormat(" <Style ss:ID=\"Default\" ss:Name=\"Normal\">{0}", Environment.NewLine);
            sb.AppendFormat(" <Alignment ss:Vertical=\"Bottom\"/>{0}", Environment.NewLine);
            sb.AppendFormat(" <Borders/>{0}", Environment.NewLine);
            sb.AppendFormat(" <Font ss:FontName=\"Calibri\" x:Family=\"Swiss\" ss:Size=\"11\" ss:Color=\"#000000\"/>{0}", Environment.NewLine);
            sb.AppendFormat(" <Interior/>{0}", Environment.NewLine);
            sb.AppendFormat(" <NumberFormat/>{0}", Environment.NewLine);
            sb.AppendFormat(" <Protection/>{0}", Environment.NewLine);
            sb.AppendFormat(" </Style>{0}", Environment.NewLine);
            sb.AppendFormat(" <Style ss:ID=\"s62\">{0}", Environment.NewLine);
            sb.AppendFormat(" <Font ss:FontName=\"Calibri\" x:Family=\"Swiss\" ss:Size=\"11\" ss:Color=\"#000000\"{0}", Environment.NewLine);
            sb.AppendFormat(" ss:Bold=\"1\"/>{0}", Environment.NewLine);
            sb.AppendFormat(" </Style>{0}", Environment.NewLine);
            sb.AppendFormat(" <Style ss:ID=\"s63\">{0}", Environment.NewLine);
            sb.AppendFormat(" <NumberFormat ss:Format=\"Short Date\"/>{0}", Environment.NewLine);
            sb.AppendFormat(" </Style>{0}", Environment.NewLine);
            sb.AppendFormat(" <Style ss:ID=\"s64\">{0}", Environment.NewLine);
            sb.AppendFormat(" <Font ss:FontName=\"Calibri\" x:Family=\"Swiss\" ss:Size=\"11\" ss:Color=\"#000000\" ss:Bold=\"1\"/>{0}", Environment.NewLine);
            sb.AppendFormat(" <Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Top\" ss:WrapText=\"0\"/>{0}", Environment.NewLine);
            sb.AppendFormat(" </Style>{0}", Environment.NewLine);
            sb.AppendFormat(" <Style ss:ID=\"s65\">{0}", Environment.NewLine);
            sb.AppendFormat(" <Alignment ss:Horizontal=\"Left\" ss:Vertical=\"Top\" ss:WrapText=\"1\"/>{0}", Environment.NewLine);
            sb.AppendFormat(" </Style>{0}", Environment.NewLine);
            sb.AppendFormat(" </Styles>{0}", Environment.NewLine);
            sb.Append("{0}\r\n</Workbook>");
            return sb.ToString();
        }
        public static string ReplaceXmlChar(string input)
        {
            input = input.Replace("&", "&amp;");
            input = input.Replace("<", "&lt;");
            input = input.Replace(">", "&gt;");
            input = input.Replace("\"", "&quot;");
            input = input.Replace("'", "&#39;");
            input = input.Replace(Environment.NewLine, "&#10;");
            return input;
        }
        private static string GetCell(Type type, Object cellData)
        {
            string data;
            if (cellData == DBNull.Value)
            {
                data = "";
            }
            else
            {
                data = cellData.ToString();
            }

            if (type.Name.Contains("Int") || type.Name.Contains("Double") || type.Name.Contains("Decimal"))
            {
                return String.Format("<Cell><Data ss:Type=\"Number\">{0}</Data></Cell>", data);
            }

            if (type.Name.Contains("Date") && data.ToString() != String.Empty)
            {
                return String.Format("<Cell ss:StyleID=\"s63\"><Data ss:Type=\"DateTime\">{0}</Data></Cell>", Convert.ToDateTime(data).ToString("yyyy-MM-dd"));
            }

            return String.Format("<Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ReplaceXmlChar(data.ToString()));
        }
        private static string GetWorksheets(System.Data.DataSet source, String Comment)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            if (source == null || source.Tables.Count == 0)
            {
                sw.Write("<Worksheet ss:Name=\"Sheet1\">" + Environment.NewLine + "<Table>" + Environment.NewLine + "<Row><Cell><Data ss:Type=\"String\"></Data></Cell></Row>" + Environment.NewLine + "</Table>" + Environment.NewLine + "</Worksheet>");
                return sw.ToString();
            }

            foreach (System.Data.DataTable dt in source.Tables)
            {
                if (dt.Rows.Count == 0)
                {
                    string sheetName = dt.TableName.SetTextLimit(25);
                    sw.Write("<Worksheet ss:Name=\"" + ReplaceXmlChar(sheetName) + "\">" + Environment.NewLine + "<Table>" + Environment.NewLine + "<Row><Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\"></Data></Cell></Row>" + Environment.NewLine + "</Table>" + Environment.NewLine + "</Worksheet>");
                }
                else
                { 
                    //write each row data 
                    int sheetCount = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if ((i % rowLimit) == 0)
                        {
                            //add close tags for previous sheet of the same data table 
                            if ((i / rowLimit) > sheetCount)
                            {
                                sw.Write(Environment.NewLine + "</Table>" + Environment.NewLine + "</Worksheet>");
                                sheetCount = (i / rowLimit);
                            }
                            string inlineLimitValue;
                            if ((i / rowLimit) == 0)
                            {
                                inlineLimitValue = "";
                            }
                            else
                            {
                                inlineLimitValue = Convert.ToString(i / rowLimit);
                            }


                            string sheetName = dt.TableName.SetTextLimit(100);
                            sw.Write((Environment.NewLine + "<Worksheet ss:Name=\"" + ReplaceXmlChar(sheetName)) + inlineLimitValue + "\">" + Environment.NewLine + "<Table>");
                            //write comment
                            if (!string.IsNullOrEmpty(Comment))
                            {
                                int totalCol = dt.Columns.Count - 1;
                                sw.Write(Environment.NewLine + "<Row>");
                                sw.Write("<Cell ss:StyleID=\"s64\"><Data ss:Type=\"String\">Comment:</Data></Cell>");
                                sw.Write(string.Format("<Cell ss:MergeAcross=\"{1}\" ss:StyleID=\"s65\"><Data ss:Type=\"String\">{0}</Data></Cell>", Comment, totalCol - 1));
                                sw.Write("</Row>");

                                sw.Write(Environment.NewLine + "<Row>");
                                sw.Write(string.Format("<Cell ss:MergeAcross=\"{0}\" ><Data ss:Type=\"String\"></Data></Cell>", totalCol));
                                sw.Write("</Row>");
                            }

                            //write column name row 
                            sw.Write(Environment.NewLine + "<Row>");
                            foreach (System.Data.DataColumn dc in dt.Columns)
                            {
                                sw.Write(String.Format("<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">{0}</Data></Cell>", ReplaceXmlChar(dc.ColumnName.Trim())));
                            }
                            sw.Write("</Row>");
                        }
                        sw.Write(Environment.NewLine + "<Row>");
                        foreach (System.Data.DataColumn dc in dt.Columns)
                        {
                            sw.Write(GetCell(dc.DataType, dt.Rows[i][dc.ColumnName]));
                        }
                        sw.Write("</Row>");
                    }
                    sw.Write(Environment.NewLine + "</Table>" + Environment.NewLine + "</Worksheet>");
                }
            }

            return sw.ToString();
        }
        private static NPOI.HSSF.UserModel.HSSFWorkbook GenerateWorkBook(System.Data.DataSet dsInput, String Comment)
        {

            NPOI.HSSF.UserModel.HSSFWorkbook workbook = new NPOI.HSSF.UserModel.HSSFWorkbook();
            NPOI.SS.UserModel.ISheet sheet = null;
            int rowIndex = 0;

            var fontBold = workbook.CreateFont();
            fontBold.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;

            var fontNormal = workbook.CreateFont();
            fontNormal.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;

            var currencyFormat = workbook.CreateCellStyle();
            currencyFormat.DataFormat = NPOI.HSSF.UserModel.HSSFDataFormat.GetBuiltinFormat("($#,##0.00_);[Red]($#,##0.00)");

            var dateTimeFormat = workbook.CreateCellStyle();
            dateTimeFormat.DataFormat = NPOI.HSSF.UserModel.HSSFDataFormat.GetBuiltinFormat("m/d/yy");

            int cellCount = 0;
            bool invalid1 = true;
            bool invalid2 = true;
            bool invalid3 = true;

            // Validate condition
            if (dsInput != null)
            {
                invalid1 = false;

                if (dsInput.Tables.Count != 0)
                {
                    invalid2 = false;

                    if (dsInput.Tables[0].Rows.Count != 0)
                    {
                        invalid3 = false;
                    }
                }
            }


            if (invalid1 | invalid2 | invalid3)
            {
                sheet = workbook.CreateSheet("Sheet1"); 
                // Auto size the column widths
                for (int columnIndex = 0; columnIndex < 20; columnIndex++)
                {
                    sheet.AutoSizeColumn(columnIndex); 
                }
            }
            else
            {
                foreach (System.Data.DataTable dt in dsInput.Tables)
                {
                    rowIndex = 0;
                    cellCount = dt.Rows[0].ItemArray.Length;
                    sheet = workbook.CreateSheet(dt.TableName);//Globals.SetTextLimit(dt.TableName, 25));
                    if (!string.IsNullOrEmpty(Comment))
                    {
                        NPOI.SS.UserModel.IRow rowComment = sheet.CreateRow(rowIndex);
                        var cellCommentTitle = rowComment.CreateCell(0);
                        cellCommentTitle.SetCellValue("Comment:");
                        cellCommentTitle.CellStyle = workbook.CreateCellStyle();
                        cellCommentTitle.CellStyle.SetFont(fontBold);

                        var cellComment = rowComment.CreateCell(1);
                        cellComment.SetCellValue(Comment);
                        cellComment.CellStyle = workbook.CreateCellStyle();
                        cellComment.CellStyle.SetFont(fontNormal); 
                        rowIndex++;
                    }

                    int[] maxColumnsWidth = new int[cellCount];
                    //for (int i = 0; i < dt.Columns.Count; i++)
                    //{
                    //    maxColumnsWidth[i] = (dt.Columns[i].ToString().Length) * 2 * 256;
                    //}
                    //Header
                    var rowHeader = GetHeaderColumns(dt, fontBold, rowIndex, sheet, workbook, cellCount, maxColumnsWidth);
                    rowHeader.HeightInPoints = 15;
                    rowIndex++;

                    //Data
                    NPOI.SS.UserModel.IRow row = null;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        row = sheet.CreateRow(rowIndex);
                        for (int j = 0; j < cellCount; j++)
                        {
                            object value = dt.Rows[i].ItemArray[j];
                            if (value is bool)
                            {
                                row.CreateCell(j).SetCellValue(value.BNZ());
                            }
                            else if ((value is double) | (value is decimal) | (value is int))
                            {
                                bool IsCurrency = (value is double);

                                double dblVar = value.ToString().DoubleNZ(0.0d);
                                var numberCell = row.CreateCell(j);
                                numberCell.SetCellValue(dblVar);

                                if (IsCurrency)
                                {
                                    numberCell.CellStyle = currencyFormat;
                                }
                            }
                            else if (value is DateTime)
                            {
                                var dtCell = row.CreateCell(j);
                                dtCell.SetCellValue(value.DTNZ());
                                dtCell.CellStyle = dateTimeFormat;
                            }
                            else
                            {
                                row.CreateCell(j).SetCellValue(value.ToString());
                            }
                        }
                        rowIndex++;
                    }
                    rowIndex++;
                }
            }

            if (!string.IsNullOrEmpty(Comment))
            {
                NPOI.SS.Util.CellRangeAddress cra = new NPOI.SS.Util.CellRangeAddress(0, 0, 1, cellCount - 1);
                sheet.AddMergedRegion(cra);
            }
            using (System.IO.MemoryStream fileData = new System.IO.MemoryStream())
            {
                workbook.Write(fileData);
            } 
            return workbook;
        }
        public static NPOI.SS.UserModel.IRow GetHeaderColumns(System.Data.DataTable dt, NPOI.SS.UserModel.IFont fontBold, int rowIndex,
            NPOI.SS.UserModel.ISheet sheet, NPOI.HSSF.UserModel.HSSFWorkbook workbook, int cellCount, int[] maxColumnsWidth)
        {
            var row = sheet.CreateRow(rowIndex);
            row.HeightInPoints = 15;
            if (cellCount > 0)
            {
                for (int i = 0; i < cellCount; i++)
                {
                    var cell = row.CreateCell(i);
                    cell.SetCellValue(dt.Columns[i].ColumnName);
                    cell.CellStyle = workbook.CreateCellStyle();
                    cell.CellStyle.SetFont(fontBold);
                    sheet.AutoSizeColumn(i);
                    sheet.SetColumnWidth(i, (dt.Columns[i].ToString().Length) * 2 * 256);
                    int valueSize = dt.Columns[i].ColumnName.Length;
                    if (maxColumnsWidth[i] < valueSize)
                    {
                        maxColumnsWidth[i] = valueSize + 1;
                    }
                }
            }
            else
            {
                var cell = row.CreateCell(0);
                cell.SetCellValue(dt.Columns[0].ColumnName);
                cell.CellStyle = workbook.CreateCellStyle();
                cell.CellStyle.SetFont(fontBold);
            }
            return row;
        }
        private static NPOI.HSSF.UserModel.HSSFWorkbook GenerateWorkBook(System.Data.DataSet dsInput, String Header, String Footer)
        {
            NPOI.HSSF.UserModel.HSSFWorkbook workbook = new NPOI.HSSF.UserModel.HSSFWorkbook();
            NPOI.SS.UserModel.ISheet sheet = null;
            int rowIndex = 0;
            var fontBold = workbook.CreateFont();
            fontBold.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            var fontNormal = workbook.CreateFont();
            fontNormal.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            var currencyFormat = workbook.CreateCellStyle();
            currencyFormat.DataFormat = NPOI.HSSF.UserModel.HSSFDataFormat.GetBuiltinFormat("($#,##0.00_);[Red]($#,##0.00)");
            var dateTimeFormat = workbook.CreateCellStyle();
            dateTimeFormat.DataFormat = NPOI.HSSF.UserModel.HSSFDataFormat.GetBuiltinFormat("m/d/yy");
            int[] maxColumnsWidth = null;
            int cellCount = 0;
            bool invalid1 = true;
            bool invalid2 = true;
            bool invalid3 = true;

            // Validate condition
            if (dsInput != null)
            {
                invalid1 = false;

                if (dsInput.Tables.Count != 0)
                {
                    invalid2 = false;

                    if (dsInput.Tables[0].Rows.Count != 0)
                    {
                        invalid3 = false;
                    }
                }
            } 
            if (invalid1 | invalid2 | invalid3)
            {
                sheet = workbook.CreateSheet("Sheet1"); 
            }
            else
            {
                foreach (System.Data.DataTable dt in dsInput.Tables)
                {
                    rowIndex = 0;
                    cellCount = dt.Rows[0].ItemArray.Length;
                    sheet = workbook.CreateSheet(dt.TableName);
                    maxColumnsWidth = new int[cellCount];
                    // Auto size the column widths
                    for (int columnIndex = 0; columnIndex < cellCount; columnIndex++)
                    {
                        sheet.AutoSizeColumn(columnIndex);
                        sheet.SetColumnWidth(columnIndex, (dt.Columns[columnIndex].ToString().Length) * 2 * 256);
                    }

                    if (!string.IsNullOrEmpty(Header))
                    {
                        var header = sheet.CreateRow(rowIndex);
                        header.HeightInPoints = 15;

                        var cellComment = header.CreateCell(0);
                        cellComment.SetCellValue(Header);
                        cellComment.CellStyle = workbook.CreateCellStyle();
                        cellComment.CellStyle.SetFont(fontNormal); 
                        rowIndex++; 
                        var header2 = sheet.CreateRow(rowIndex);
                        header2.HeightInPoints = 15; 
                        rowIndex++;
                    }

                    //Header
                    var rowHeader = GetHeaderColumns(dt, fontBold, rowIndex, sheet, workbook, cellCount, maxColumnsWidth);
                    rowHeader.HeightInPoints = 15;
                    rowIndex++;

                    //Data
                    NPOI.SS.UserModel.IRow row = null;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        row = sheet.CreateRow(rowIndex);
                        row.HeightInPoints = 15;

                        for (int j = 0; j < cellCount; j++)
                        {
                            object value = dt.Rows[i].ItemArray[j];
                            if (value is bool)
                            {
                                row.CreateCell(j).SetCellValue(value.BNZ());
                            }
                            else if ((value is double) | (value is decimal) | (value is int))
                            {
                                bool IsCurrency = (value is double); 
                                double dblVar = value.ToString().DoubleNZ(0.0d);
                                var numberCell = row.CreateCell(j);
                                numberCell.SetCellValue(dblVar);

                                if (IsCurrency)
                                {
                                    numberCell.CellStyle = currencyFormat;
                                }
                            }
                            else if (value is DateTime)
                            {
                                var dtCell = row.CreateCell(j);
                                dtCell.SetCellValue(value.DTNZ());
                                dtCell.CellStyle = dateTimeFormat;
                            }
                            else
                            {
                                var cell = row.CreateCell(j);
                                cell.CellStyle.SetFont(fontNormal);
                                cell.SetCellValue(value.ToString());
                            } 
                            //Store value size for calculate columns width
                            int valueSize = value.ToString().Length;
                            if (maxColumnsWidth[j] < valueSize)
                            {
                                maxColumnsWidth[j] = valueSize;
                            } 
                        }
                        rowIndex++;
                    }

                    var beforeFooter = sheet.CreateRow(rowIndex);
                    beforeFooter.HeightInPoints = 15;

                    rowIndex++;

                    if (!string.IsNullOrEmpty(Footer))
                    {
                        var footer = sheet.CreateRow(rowIndex);
                        footer.HeightInPoints = 15;
                        var cellComment = footer.CreateCell(0);
                        cellComment.SetCellValue(Footer);
                        cellComment.CellStyle = workbook.CreateCellStyle();
                        cellComment.CellStyle.SetFont(fontNormal);
                        rowIndex++;
                    }
                }
            }
            int columnMultipleExpand = 275;
            for (int columnIndex = 0; columnIndex < cellCount; columnIndex++)
            {
                //var colwid = maxColumnsWidth[columnIndex] * columnMultipleExpand;
                //if (colwid > 254*275)
                //{
                //    colwid = 254 * 275;
                //}
                //sheet.SetColumnWidth(columnIndex, colwid);
                sheet.AutoSizeColumn(columnIndex);
            }
            cellCount = cellCount == 0 ? 1 : cellCount;
            if (!string.IsNullOrEmpty(Header))
            {
                NPOI.SS.Util.CellRangeAddress cra = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, cellCount - 1);
                sheet.AddMergedRegion(cra);
            }

            if (!string.IsNullOrEmpty(Footer))
            {
                rowIndex = rowIndex == 0 ? 2 : rowIndex;
                NPOI.SS.Util.CellRangeAddress cra = new NPOI.SS.Util.CellRangeAddress(rowIndex - 1, rowIndex - 1, 0, cellCount - 1);
                sheet.AddMergedRegion(cra);
            } 
            using (System.IO.MemoryStream fileData = new System.IO.MemoryStream())
            {
                workbook.Write(fileData);
            }

            return workbook;
        }
        public static string GetExcelXml(System.Data.DataTable dtInput, String filename, String Comment)
        {
            string excelTemplate = GetWorkbookTemplate();
            System.Data.DataSet ds = new System.Data.DataSet();
            ds.Tables.Add(dtInput.Copy());
            string worksheets = GetWorksheets(ds, Comment);
            string excelXml = String.Format(excelTemplate, worksheets);
            return excelXml;
        }
        public static string GetExcelXml(System.Data.DataSet dsInput, String filename, String Comment)
        {
            string excelTemplate = GetWorkbookTemplate();
            string worksheets = GetWorksheets(dsInput, Comment);
            string excelXml = String.Format(excelTemplate, worksheets);
            return excelXml;
        } 
        public static string GetWorkSheet()
        {
            return GetWorkbookTemplate();
        }


        #region Dynamic Report 
        public static byte[] ToExcel(this  System.Data.DataSet dsInput, String Comment)
        {
            return ToExcelByNPOI(dsInput,Comment);
        }
        public static byte[] ToExcel(this  System.Data.DataTable dtInput, String Comment)
        {
            using (System.Data.DataSet ds = new System.Data.DataSet())
            {
                ds.Tables.Add(dtInput.Copy());
                return ToExcel(ds, Comment);
            }
        } 
        private static byte[] ToExcelByNPOI(this  System.Data.DataSet dsInput, String Comment)
        {
            using (System.IO.MemoryStream exportData = new System.IO.MemoryStream())
            {
                NPOI.HSSF.UserModel.HSSFWorkbook workbook = GenerateWorkBook(dsInput, Comment);
                workbook.Write(exportData);
                return exportData.ToArray();
            }
        } 
        public static byte[] ToExcelDynamicReport(this System.Data.DataSet dsInput, String Header, String Footer)
        {
            using (System.IO.MemoryStream exportData = new System.IO.MemoryStream())
            {
                NPOI.HSSF.UserModel.HSSFWorkbook workbook = GenerateWorkBook(dsInput, Header, Footer);
                workbook.Write(exportData);
                return exportData.ToArray();
            }
        }
        #endregion

    } 
}
