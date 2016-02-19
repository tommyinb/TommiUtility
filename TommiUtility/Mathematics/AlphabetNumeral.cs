using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Mathematics
{
    public class AlphabetNumeral
    {
        public static int Convert(string @string)
        {
            Contract.Requires<ArgumentNullException>(@string != null);
            Contract.Requires<FormatException>(@string.Length > 0);
            Contract.Ensures(Contract.Result<int>() >= 0);

            var value = 0;

            foreach (var digitText in @string)
            {
                if (digitText < 'A') throw new FormatException();
                if (digitText > 'Z') throw new FormatException();

                var digitValue = digitText - 'A';

                checked
                {
                    value = value * 26 + (digitValue + 1);
                }
            }

            return value - 1;
        }

        public static string Convert(int value)
        {
            Contract.Requires<ArgumentException>(value >= 0);
            Contract.Ensures(Contract.Result<string>() != null);
            Contract.Ensures(Contract.Result<string>().Length > 0);

            var builder = new StringBuilder();

            for (int i = value; i >= 0; i = i / 26 - 1)
            {
                var digitText = (char)('A' + (i % 26));

                builder.Insert(0, digitText);
            }

            var result = builder.ToString();
            Contract.Assume(result.Length > 0);

            return builder.ToString();
        }
    }

    [TestClass]
    public class AlphabetNumeralTest
    {
        [TestMethod]
        public void Test()
        {
            Assert.AreEqual(0, AlphabetNumeral.Convert("A"));
            Assert.AreEqual("A", AlphabetNumeral.Convert(0));

            Assert.AreEqual(1, AlphabetNumeral.Convert("B"));
            Assert.AreEqual("B", AlphabetNumeral.Convert(1));

            Assert.AreEqual(25, AlphabetNumeral.Convert("Z"));
            Assert.AreEqual("Z", AlphabetNumeral.Convert(25));

            Assert.AreEqual(26, AlphabetNumeral.Convert("AA"));
            Assert.AreEqual("AA", AlphabetNumeral.Convert(26));

            Assert.AreEqual(51, AlphabetNumeral.Convert("AZ"));
            Assert.AreEqual("AZ", AlphabetNumeral.Convert(51));

            Assert.AreEqual(52, AlphabetNumeral.Convert("BA"));
            Assert.AreEqual("BA", AlphabetNumeral.Convert(52));

            for (int i = 0; i < 26 * 26 * 26; i++)
            {
                var text = AlphabetNumeral.Convert(i);
                var value = AlphabetNumeral.Convert(text);
                Assert.AreEqual(i, value);
            }
        }
    }
}
