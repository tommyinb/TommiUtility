using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Windows
{
    public class RecycleBin
    {
        private const int FO_DELETE = 3;
        private const int FOF_ALLOWUNDO = 0x40;
        private const int FOF_NOCONFIRMATION = 0x10;
        private const int FOF_NOERRORUI = 0x400;

        public static void MoveFileToBin(params string[] filePaths)
        {
            var shf = new SHFILEOPSTRUCT();
            shf.wFunc = FO_DELETE;
            shf.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI;

            var fullPaths = filePaths.Select(Path.GetFullPath);
            shf.pFrom = string.Join("\0", fullPaths) + "\0\0";

            var result = NativeMethods.SHFileOperation(ref shf);
            if (result != 0)
            {
                throw new ExternalException("Deletion failed with " + result + ".");
            }
        }
    }

    internal static partial class NativeMethods
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern int SHFileOperation([In] ref SHFILEOPSTRUCT FileOp);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
    internal struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        [MarshalAs(UnmanagedType.U4)]
        public int wFunc;
        public string pFrom;
        public string pTo;
        public short fFlags;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string lpszProgressTitle;
    }

    [TestClass]
    public class RecycleBinTest
    {
        [TestMethod]
        public void Test()
        {
            var testFile = "Test RecycleBin.txt";
            File.WriteAllText(testFile, "abc");

            var testFolder = "Test RecycleBin";
            Directory.CreateDirectory("Test RecycleBin");

            RecycleBin.MoveFileToBin(testFile, testFolder);

            Assert.IsFalse(File.Exists(testFile));

            Assert.IsFalse(Directory.Exists(testFolder));
        }
    }
}
