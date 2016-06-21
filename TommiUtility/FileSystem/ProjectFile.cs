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
    public static class ProjectFile
    {
        private static IEnumerable<string> GetPossiblePaths(string relativePath)
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentException>(relativePath.Length > 0);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            yield return relativePath;
            yield return Path.Combine(@"..\..", relativePath);
        }
        public static IEnumerable<string> GetValidPaths(string relativePath)
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentException>(relativePath.Length > 0);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            var possiblePaths = GetPossiblePaths(relativePath);
            var existPaths = possiblePaths.Where(File.Exists);
            return existPaths.OrderByDescending(File.GetLastWriteTime);
        }
        public static string GetValidPath(string relativePath)
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentException>(relativePath.Length > 0);
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(Contract.Result<string>().Length > 0);

            var validPath = GetValidPaths(relativePath).FirstOrDefault();
            if (validPath == null) throw new FileNotFoundException(null, relativePath);

            Contract.Assume(validPath.Length > 0);
            return validPath;
        }

        public static bool Exists(string relativePath)
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentException>(relativePath.Length > 0);

            var possiblePaths = GetPossiblePaths(relativePath);
            return possiblePaths.Any(File.Exists);
        }

        public static IEnumerable<string> ReadLines(string relativePath)
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentException>(relativePath.Length > 0);

            var validPath = GetValidPath(relativePath);
            return File.ReadLines(validPath);
        }
        public static string[] ReadAllLines(string relativePath)
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentException>(relativePath.Length > 0);
            Contract.Ensures(Contract.Result<string[]>() != null);

            var validPath = GetValidPath(relativePath);
            return File.ReadAllLines(validPath);
        }

        public static string ReadAllText(string relativePath)
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentException>(relativePath.Length > 0);
            Contract.Ensures(Contract.Result<string>() != null);

            var validPath = GetValidPath(relativePath);
            return File.ReadAllText(validPath);
        }
        public static byte[] ReadAllBytes(string relativePath)
        {
            Contract.Requires<ArgumentNullException>(relativePath != null);
            Contract.Requires<ArgumentException>(relativePath.Length > 0);
            Contract.Ensures(Contract.Result<byte[]>() != null);

            var validPath = GetValidPath(relativePath);
            return File.ReadAllBytes(validPath);
        }
    }

    [TestClass]
    public class RelativeFileTest
    {
        [TestMethod]
        public void Test()
        {
            Directory.CreateDirectory(@"Test");
            Directory.CreateDirectory(@"..\..\Test");
            try
            {
                var relativePath = @"Test\abc.txt";

                var testLines1 = new[] { "abc", "bcd" };
                File.WriteAllLines(relativePath, testLines1);

                var testLines2 = new[] { "cde", "def" };
                File.WriteAllLines(@"..\..\" + relativePath, testLines2);

                var readLines = ProjectFile.ReadAllLines(relativePath);
                Assert.IsTrue(testLines2.SequenceEqual(readLines));

                File.WriteAllText(relativePath, "abcd");
                var readText = ProjectFile.ReadAllText(relativePath);
                Assert.AreEqual("abcd", readText);

                var testBytes = new byte[] { 1, 2, 3, 4 };
                File.WriteAllBytes(@"..\..\" + relativePath, testBytes);
                var readBytes = ProjectFile.ReadAllBytes(relativePath);
                Assert.IsTrue(testBytes.SequenceEqual(readBytes));
            }
            finally
            {
                Directory.Delete(@"Test", recursive: true);
                Directory.Delete(@"..\..\Test", recursive: true);
            }
        }
    }
}
