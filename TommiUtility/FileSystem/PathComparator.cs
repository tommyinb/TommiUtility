using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.FileSystem
{
    public class PathComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x != null)
            {
                if (y != null)
                {
                    return ComparePath(x, y);
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (y != null)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
        private int ComparePath(string xPath, string yPath)
        {
            Contract.Requires<ArgumentNullException>(xPath != null);
            Contract.Requires<ArgumentNullException>(yPath != null);

            var directorySeparators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            var xBlocks = xPath.Split(directorySeparators);
            var yBlocks = yPath.Split(directorySeparators);

            var commonBlockCount = Math.Min(xBlocks.Length, yBlocks.Length);
            for (int i = 0; i < commonBlockCount; i++)
            {
                var xBlock = xBlocks[i];
                var yBlock = yBlocks[i];

                var compareBlock = CompareBlock(xBlock, yBlock);
                if (compareBlock != 0)
                {
                    return compareBlock;
                }
            }

            return xBlocks.Length - yBlocks.Length;
        }
        private int CompareBlock(string xBlock, string yBlock)
        {
            Contract.Requires<ArgumentNullException>(xBlock != null);
            Contract.Requires<ArgumentNullException>(yBlock != null);

            var xParts = Regex.Matches(xBlock, @"\d+(.\d+)?|[^\d]+");
            var yParts = Regex.Matches(yBlock, @"\d+(.\d+)?|[^\d]+");

            var commonPartCount = Math.Min(xParts.Count, yParts.Count);
            for (int j = 0; j < commonPartCount; j++)
            {
                var xPart = xParts[j].Value;
                var yPart = yParts[j].Value;

                var comparePart = ComparePart(xPart, yPart);
                if (comparePart != 0)
                {
                    return comparePart;
                }
            }

            return xParts.Count - yParts.Count;
        }
        private int ComparePart(string xPart, string yPart)
        {
            Contract.Requires<ArgumentNullException>(xPart != null);
            Contract.Requires<ArgumentNullException>(yPart != null);

            decimal xPartValue, yPartValue;
            if (decimal.TryParse(xPart, out xPartValue)
                && decimal.TryParse(yPart, out yPartValue))
            {
                return xPartValue.CompareTo(yPartValue);
            }
            else
            {
                return string.Compare(xPart, yPart);
            }
        }
    }

    [TestClass]
    public class PathComparerTest
    {
        [TestMethod]
        public void Test()
        {
            var comparer = new PathComparer();

            Assert.AreEqual(-1, comparer.Compare(@"C:\Abc1", @"C:\Abc9"));
            Assert.AreEqual(1, comparer.Compare(@"C:\Abc10", @"C:\Abc9"));

            Assert.AreEqual(-1, comparer.Compare(@"C:\Abc10", @"C:\Abc99"));
            Assert.AreEqual(1, comparer.Compare(@"C:\Abc100", @"C:\Abc99"));

            Assert.AreEqual(0, comparer.Compare(@"C:\Abc10", @"C:\Abc10"));

            Assert.AreEqual(-1, comparer.Compare(@"C:\Abc10\1Abc", @"C:\Abc10\2-Abc"));
            Assert.AreEqual(1, comparer.Compare(@"C:\Abc10\10Abc", @"C:\Abc10\2-Abc"));

            Assert.AreEqual(1, comparer.Compare(@"C:\Abc1.3", @"C:\Abc1.1"));
            Assert.AreEqual(-1, comparer.Compare(@"C:\Abc1.3", @"C:\Abc2.1"));
        }
    }
}
