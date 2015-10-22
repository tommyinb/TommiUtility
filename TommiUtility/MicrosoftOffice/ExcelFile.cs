using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            FilePath = Path.GetFullPath(filePath);

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
            if (excel != null)
            {
                foreach (Workbook workbook in excel.Workbooks)
                {
                    workbook.Close(SaveChanges: false);
                }

                excel.Quit();
                excel = null;
            }
        }
        ~ExcelFile()
        {
            Dispose(false);
        }

        private ExcelApplication excel = new ExcelApplication();
        private Workbook workbook;

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

        public ExcelSheet GetSheet(string sheetName, bool createIfAbsent = false)
        {
            var sheet = excel.ActiveWorkbook.Worksheets.Cast<Worksheet>()
                .FirstOrDefault(t => t.Name == sheetName);

            if (sheet == null)
            {
                if (createIfAbsent == false)
                    throw new ArgumentException();

                sheet = excel.ActiveWorkbook.Worksheets.Add();
                sheet.Name = sheetName;
            }

            return new ExcelSheet(this, sheet);
        }
    }

    public class ExcelSheet
    {
        public ExcelSheet(ExcelFile file, Worksheet sheet)
        {
            File = file;
            Sheet = sheet;
        }
        public ExcelFile File { get; private set; }
        public Worksheet Sheet { get; private set; }

        public object this[int row, string column]
        {
            get
            {
                return Sheet.Cells[row, column].Value;
            }
            set
            {
                Sheet.Cells[row, column].Value = value;
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
