using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace TommiUtility.FileSystem
{
    public static class FileUtil
    {
        public static void WriteFile(string path, string text)
        {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(path.Length > 0);
            Contract.Requires<ArgumentNullException>(text != null);

            EnsureDirectoryForFile(path);

            File.WriteAllText(path, text);
        }
        public static void WriteFile(string path, string text, Encoding encoding)
        {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(path.Length > 0);
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Requires<ArgumentNullException>(encoding != null);

            EnsureDirectoryForFile(path);

            File.WriteAllText(path, text, encoding);
        }
        public static void WriteFile(string path, IEnumerable<string> lines)
        {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(path.Length > 0);
            Contract.Requires<ArgumentNullException>(lines != null);

            EnsureDirectoryForFile(path);

            File.WriteAllLines(path, lines);
        }
        public static void WriteFile(string path, IEnumerable<string> lines, Encoding encoding)
        {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(path.Length > 0);
            Contract.Requires<ArgumentNullException>(lines != null);
            Contract.Requires<ArgumentNullException>(encoding != null);

            EnsureDirectoryForFile(path);

            File.WriteAllLines(path, lines, encoding);
        }
        public static void WriteFile(string path, byte[] bytes)
        {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(path.Length > 0);
            Contract.Requires<ArgumentNullException>(bytes != null);

            EnsureDirectoryForFile(path);

            File.WriteAllBytes(path, bytes);
        }

        private static void EnsureDirectoryForFile(string filePath)
        {
            Contract.Requires<ArgumentNullException>(filePath != null);
            Contract.Requires<ArgumentException>(filePath.Length > 0);

            var directory = Path.GetDirectoryName(filePath);
            if (directory.Length > 0)
            {
                Directory.CreateDirectory(directory);
            }
        }
    }

    [TestClass]
    public class FileUtilTest
    {
        [TestMethod]
        public void TestWriteFile()
        {
            var filePath = "test.txt";

            FileUtil.WriteFile(filePath, "abc");
            var text1 = File.ReadAllText(filePath);
            Assert.AreEqual("abc", text1);

            FileUtil.WriteFile(filePath, "我", Encoding.UTF8);
            var text2 = File.ReadAllText(filePath);
            Assert.AreEqual("我", text2);

            FileUtil.WriteFile(filePath, new[] { "abc", "bcd" });
            var text3 = File.ReadAllLines(filePath);
            Assert.IsTrue(new[] { "abc", "bcd" }.SequenceEqual(text3));

            FileUtil.WriteFile(filePath, new[] { "我", "你" });
            var text4 = File.ReadAllLines(filePath);
            Assert.IsTrue(new[] { "我", "你" }.SequenceEqual(text4));

            FileUtil.WriteFile(filePath, new byte[] { 1, 2, 3 });
            var bytes = File.ReadAllBytes(filePath);
            Assert.IsTrue(new byte[] { 1, 2, 3 }.SequenceEqual(bytes));

            File.Delete(filePath);
        }
    }
}
