using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.FileSystem
{
    public static class FileUtil
    {
        public static void WriteFile(string path, string text)
        {
            EnsureDirectoryForFile(path);

            File.WriteAllText(path, text);
        }
        public static void WriteFile(string path, string text, Encoding encoding)
        {
            EnsureDirectoryForFile(path);

            File.WriteAllText(path, text, encoding);
        }
        public static void WriteFile(string path, IEnumerable<string> lines)
        {
            EnsureDirectoryForFile(path);

            File.WriteAllLines(path, lines);
        }
        public static void WriteFile(string path, IEnumerable<string> lines, Encoding encoding)
        {
            EnsureDirectoryForFile(path);

            File.WriteAllLines(path, lines, encoding);
        }
        public static void WriteFile(string path, byte[] bytes)
        {
            EnsureDirectoryForFile(path);

            File.WriteAllBytes(path, bytes);
        }

        private static void EnsureDirectoryForFile(string filePath)
        {
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
            Assert.AreEqual(2, text3.Length);
            Assert.AreEqual("abc", text3[0]);
            Assert.AreEqual("bcd", text3[1]);

            FileUtil.WriteFile(filePath, new[] { "我", "你" });
            var text4 = File.ReadAllLines(filePath);
            Assert.AreEqual(2, text4.Length);
            Assert.AreEqual("我", text4[0]);
            Assert.AreEqual("你", text4[1]);

            FileUtil.WriteFile(filePath, new byte[] { 1, 2, 3 });
            var bytes = File.ReadAllBytes(filePath);
            Assert.AreEqual(3, bytes.Length);
            Assert.AreEqual(1, bytes[0]);
            Assert.AreEqual(2, bytes[1]);
            Assert.AreEqual(3, bytes[2]);

            File.Delete(filePath);
        }
    }
}
