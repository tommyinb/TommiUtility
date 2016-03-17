using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExcelApplication = Microsoft.Office.Interop.Excel.Application;

namespace TommiUtility.MicrosoftOffice
{
    public sealed class ExcelFile : IDisposable
    {
        public ExcelFile(string filePath)
        {
            Contract.Requires<ArgumentNullException>(filePath != null);
            Contract.Requires<ArgumentException>(filePath.Length > 0);

            FilePath = Path.GetFullPath(filePath);

            excel = StartExcel();
            if (excel.Workbooks == null) throw new InvalidOperationException();

            if (File.Exists(FilePath))
            {
                workbook = excel.Workbooks.Open(FilePath);
                isNewFile = false;
            }
            else
            {
                workbook = excel.Workbooks.Add();
                isNewFile = true;
            }

            excel.DisplayAlerts = false;
        }
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            var workbooks = excel.Workbooks.OfType<Workbook>();
            foreach (Workbook workbook in workbooks)
            {
                workbook.Close(SaveChanges: false);
            }

            excel.Quit();
        }
        ~ExcelFile()
        {
            Dispose(false);
        }

        private readonly ExcelApplication excel;
        private readonly Workbook workbook;

        [ContractVerification(false)]
        private static ExcelApplication StartExcel()
        {
            return new ExcelApplication();
        }

        public string FilePath { get; private set; }
        private bool isNewFile;
        public void Save()
        {
            if (isNewFile)
            {
                workbook.SaveAs(FilePath);
                isNewFile = false;
            }
            else
            {
                workbook.Save();
            }
        }

        [ContractVerification(false)]
        public ExcelSheet GetSheet(string sheetName, bool createIfAbsent = false)
        {
            Contract.Requires<ArgumentNullException>(sheetName != null);
            Contract.Requires<ArgumentException>(sheetName.Length > 0);
            Contract.Ensures(Contract.Result<ExcelSheet>() != null);

            var worksheets = excel.ActiveWorkbook.Worksheets.OfType<Worksheet>();
            var worksheet = worksheets.FirstOrDefault(t => t.Name == sheetName);

            if (worksheet == null)
            {
                if (createIfAbsent == false) throw new ArgumentException();

                worksheet = excel.ActiveWorkbook.Worksheets.Add();
                worksheet.Name = sheetName;
            }

            return new ExcelSheet(this, worksheet);
        }
    }

    public class ExcelSheet
    {
        public ExcelSheet(ExcelFile file, Worksheet sheet)
        {
            Contract.Requires<ArgumentNullException>(file != null);
            Contract.Requires<ArgumentNullException>(sheet != null);
            Contract.Requires<ArgumentNullException>(sheet.Cells != null);

            File = file;
            Sheet = sheet;
        }
        public readonly ExcelFile File;
        public readonly Worksheet Sheet;

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(File != null);
            Contract.Invariant(Sheet != null);
            Contract.Invariant(Sheet.Cells != null);
        }

        [ContractVerification(false)]
        public object this[int row, string column]
        {
            get
            {
                return Sheet.Cells[row, column].Value;
            }
            set
            {
                var cell = Sheet.Cells[row, column];
                if (cell == null) throw new ArgumentException();

                cell.Value = value;
            }
        }
    }

    [TestClass]
    public class ExcelFileTest
    {
        [TestMethod]
        public void Test()
        {
            var fileName = "test.xls";

            using (var excelFile = new ExcelFile(fileName))
            {
                var sheet = excelFile.GetSheet("abc", createIfAbsent: true);

                sheet[3, "C"] = "ABC";

                sheet[3, "D"] = "'01";

                excelFile.Save();
            }
            try
            {
                using (var excelFile = new ExcelFile(fileName))
                {
                    var sheet = excelFile.GetSheet("abc");

                    Assert.AreEqual("ABC", sheet[3, "C"]);

                    Assert.AreEqual("01", sheet[3, "D"]);
                }
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
