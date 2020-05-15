using Dapper;
using LDMS.Daos;
using LDMS.Identity;
using LDMS.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LDMS.Services
{
    public abstract class ILDMSService
    {
        protected HttpContext HttpContext { get; private set; }
        private ILDMSConnection _LDMSConnection;
        protected const string _schema = "[dbo]";
        public ILDMSService(ILDMSConnection connection, IHttpContextAccessor httpContextAccessor)
        {
            HttpContext = httpContextAccessor.HttpContext;
            _LDMSConnection = connection;
        }
        protected System.Data.IDbConnection Connection
        {
            get
            {
                var connection = _LDMSConnection.GetConnection();
                if (connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();
                return connection;
            }
        }
        protected async Task<List<T>> All<T>(string table) where T : BaseViewModel
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var results = Connection.Query<T>(_schema + ".[usp_" + table + "_READ_ALL]", 
                      commandTimeout: 60,
                      commandType: System.Data.CommandType.StoredProcedure);
                return results.ToList();
            }
        }
        protected string CurrentUserId
        {
            get
            {
                return JwtManager.Instance.GetUserId(HttpContext.Request);
            }
        }

        protected void CreateDataLog(DataLogType dataLogType, string employeeId, string detail)
        {
            try
            {
                using (System.Data.IDbConnection conn = Connection)
                {
                    var userId = CurrentUserId;
                    if (string.IsNullOrEmpty(userId))
                    {
                        userId = employeeId;
                    }
                    DynamicParameters parameter = new DynamicParameters();
                    parameter.Add("@LogTypeId", (int)dataLogType);
                    parameter.Add("@EmployeeID", employeeId);
                    parameter.Add("@ip_address", HttpContext.Connection.RemoteIpAddress.ToString());
                    parameter.Add("@CreateBy", userId);
                    parameter.Add("@LogDetail", detail);
                    string commad = _schema + ".[usp_LDMS_T_DataLog_Create]";
                    Connection.Execute(
                            sql: commad,
                            param: parameter,
                            commandTimeout: 0,
                            commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected virtual DocumentFormat.OpenXml.Spreadsheet.Sheet GetSheetFromWorkSheet(DocumentFormat.OpenXml.Packaging.WorkbookPart workbookPart, DocumentFormat.OpenXml.Packaging.WorksheetPart worksheetPart)
        {
            string relationshipId = workbookPart.GetIdOfPart(worksheetPart);
            IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Sheet> sheets = workbookPart.Workbook.Sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>();
            return sheets.FirstOrDefault(s => s.Id.HasValue && s.Id.Value == relationshipId);
        }
        protected virtual string GetCellValue(DocumentFormat.OpenXml.Packaging.SpreadsheetDocument document, DocumentFormat.OpenXml.Spreadsheet.Cell cell)
        {
            if (cell.CellValue == null) return "";
            DocumentFormat.OpenXml.Packaging.SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == DocumentFormat.OpenXml.Spreadsheet.CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }
        protected virtual int CellReferenceToIndex(DocumentFormat.OpenXml.Spreadsheet.Cell cell)
        {
            int index = 0;
            string reference = cell.CellReference.ToString().ToUpper();
            foreach (char ch in reference)
            {
                if (Char.IsLetter(ch))
                {
                    int value = (int)ch - (int)'A';
                    index = (index == 0) ? value : ((index + 1) * 26) + value;
                }
                else
                    return index;
            }
            return index;
        }

        protected virtual DataTable ConvertStreamToDatatable(Stream stream, string sheetName)
        {
            DataTable dt = new DataTable(sheetName);
            using (DocumentFormat.OpenXml.Packaging.SpreadsheetDocument doc = DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(stream, false))
            {
                DocumentFormat.OpenXml.Packaging.WorkbookPart workbookPart = doc.WorkbookPart;
                foreach (var worksheetPart in workbookPart.WorksheetParts)
                {
                    DocumentFormat.OpenXml.Spreadsheet.Sheet workSheet = GetSheetFromWorkSheet(workbookPart, worksheetPart);
                    if (workSheet.Name.Value.ToLower() == sheetName.ToLower())
                    {
                        DocumentFormat.OpenXml.Spreadsheet.SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.SheetData>();
                        bool isCreateColumn = false;
                        foreach (DocumentFormat.OpenXml.Spreadsheet.Row row in sheetData)
                        {
                            if (!isCreateColumn)
                            {
                                foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in row)
                                {
                                    dt.Columns.Add(GetCellValue(doc, cell).ToLower());
                                }
                                isCreateColumn = true;
                            }
                            else
                            {
                                DataRow tempRow = dt.NewRow();
                                foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in row.Descendants<DocumentFormat.OpenXml.Spreadsheet.Cell>())
                                {
                                    try
                                    {
                                        int index = CellReferenceToIndex(cell);
                                        var value = GetCellValue(doc, cell);
                                        tempRow[index] = value;
                                    }
                                    catch (Exception exx)
                                    {
                                        //serviceResult.AddException(new Exception(string.Format("Import Record {0} error", (dt.Rows.Count + 1))));
                                      //  _logger.LogError(string.Format("Import Record {0}  error", (dt.Rows.Count + 1)));
                                    }
                                }
                                dt.Rows.Add(tempRow);
                            }
                        }
                    }
                }
            }
            return dt;
        }
    }
    public class SQLError
    {
        public long ErrorNumber { get; set; }
        public long ErrorSeverity { get; set; }
        public long ErrorState { get; set; }
        public string ErrorProcedure { get; set; }
        public long ErrorLine { get; set; }
        public string ErrorMessage { get; set; }
    }
    public enum DataLogType : int
    {
        LoginSuccess = 1,
        LoginFaild = 2,
        ChangePassword = 3,
        ResetPassword = 4,
        DeleteAccount = 5,
        UpdateAccount = 6,
        AcceptGPP = 7,
        CreateAccount = 8,
    }
}
