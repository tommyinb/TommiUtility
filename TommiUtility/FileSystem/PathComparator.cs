using System;
using System.Collections.Generic;
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
            var directorySeparators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            var xBlocks = x.Split(directorySeparators);
            var yBlocks = y.Split(directorySeparators);

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
            decimal xPartValue, yPartValue;
            if (decimal.TryParse(xPart, out xPartValue)
                && decimal.TryParse(yPart, out yPartValue))
            {
                return xPartValue.CompareTo(yPartValue);
            }
            else
            {
                return xPart.CompareTo(yPart);
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
