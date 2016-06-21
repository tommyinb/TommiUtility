using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.FileSystem
{
    public static class DirectoryUtil
    {
        public static void Copy(string fromPath, string toPath)
        {
            Contract.Requires<ArgumentNullException>(fromPath != null);
            Contract.Requires<ArgumentException>(fromPath.Length > 0);
            Contract.Requires<ArgumentNullException>(toPath != null);
            Contract.Requires<ArgumentException>(toPath.Length > 0);

            Copy(fromPath, toPath, t => true);
        }
        public static void Copy(string fromPath, string toPath, Func<string, bool> filter)
        {
            Contract.Requires<ArgumentNullException>(fromPath != null);
            Contract.Requires<ArgumentException>(fromPath.Length > 0);
            Contract.Requires<ArgumentNullException>(toPath != null);
            Contract.Requires<ArgumentException>(toPath.Length > 0);
            Contract.Requires<ArgumentNullException>(filter != null);

            Directory.CreateDirectory(toPath);

            var fromFiles = Directory.EnumerateFiles(fromPath);
            var validFiles = fromFiles.Where(filter).ToArray();
            foreach (var fromFile in validFiles)
            {
                Contract.Assume(string.IsNullOrEmpty(fromFile) == false);
                var fileName = Path.GetFileName(fromFile);
                Contract.Assume(fileName != null);
                var toFile = Path.Combine(toPath, fileName);

                File.Copy(fromFile, toFile, overwrite: true);
            }

            var fromDirectories = Directory.EnumerateDirectories(fromPath);
            var validDirectories = fromDirectories.Where(filter).ToArray();
            foreach (var fromDirectory in validDirectories)
            {
                Contract.Assume(fromDirectory != null);
                Contract.Assume(fromDirectory.Length > 0);

                var directoryName = Path.GetFileName(fromDirectory);
                var toDirectory = Path.Combine(toPath, directoryName);
                Copy(fromDirectory, toDirectory, filter);
            }
        }
    }

    [TestClass]
    public class DirectoryUtilTest
    {
        [TestMethod]
        public void TestCopy()
        {
            if (Directory.Exists("Test"))
            {
                Directory.Delete("Test", recursive: true);
            }

            Directory.CreateDirectory(@"Test");
            File.WriteAllText(@"Test\abc.txt", "abc");

            Directory.CreateDirectory(@"Test\Abc");
            File.WriteAllText(@"Test\Abc\bcd.txt", "bcd");
            File.WriteAllText(@"Test\Abc\cde.txt", "cde");

            Directory.CreateDirectory(@"Test\Abc\Bcd");
            File.WriteAllText(@"Test\Abc\Bcd\def.txt", "def");

            Directory.CreateDirectory(@"Test\Cde");
            File.WriteAllText(@"Test\Cde\efg.txt", "efg");
            File.WriteAllText(@"Test\Cde\fgh.txt", "fgh");
            File.WriteAllText(@"Test\Cde\ghi.txt", "ghi");

            Directory.CreateDirectory(@"Test\Def");
            File.WriteAllText(@"Test\Def\hij.txt", "hij");

            if (Directory.Exists("Test2"))
            {
                Directory.Delete("Test2", recursive: true);
            }

            DirectoryUtil.Copy(@"Test", @"Test2", t =>
                (t.EndsWith("Def") || t.EndsWith("ghi.txt")) == false);

            Assert.IsTrue(File.Exists(@"Test2\abc.txt"));
            Assert.AreEqual("abc", File.ReadAllText(@"Test2\abc.txt"));

            Assert.IsTrue(File.Exists(@"Test2\Abc\bcd.txt"));
            Assert.AreEqual("bcd", File.ReadAllText(@"Test2\Abc\bcd.txt"));
            Assert.IsTrue(File.Exists(@"Test2\Abc\cde.txt"));
            Assert.AreEqual("cde", File.ReadAllText(@"Test2\Abc\cde.txt"));

            Assert.IsTrue(File.Exists(@"Test\Abc\Bcd\def.txt"));
            Assert.AreEqual("def", File.ReadAllText(@"Test\Abc\Bcd\def.txt"));

            Assert.IsTrue(File.Exists(@"Test2\Cde\efg.txt"));
            Assert.AreEqual("efg", File.ReadAllText(@"Test2\Cde\efg.txt"));
            Assert.IsTrue(File.Exists(@"Test2\Cde\fgh.txt"));
            Assert.AreEqual("fgh", File.ReadAllText(@"Test2\Cde\fgh.txt"));
            
            Assert.IsFalse(File.Exists(@"Test2\Cde\ghi.txt"));
            Assert.IsFalse(Directory.Exists(@"Test2\Def"));

            Directory.Delete(@"Test", recursive: true);
            Directory.Delete(@"Test2", recursive: true);
        }
    }
}
