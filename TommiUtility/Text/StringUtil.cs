using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Text
{
    public static class StringUtil
    {
        public static bool ContainsAny(this string text, params string[] texts)
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Requires<ArgumentNullException>(texts != null);

            return texts.Any(text.Contains);
        }

        public static string FirstLetterToUpper(this string text)
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (text.Length <= 0) return string.Empty;

            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }
        public static string FirstLetterToLower(this string text)
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (text.Length <= 0) return string.Empty;

            return text.Substring(0, 1).ToLower() + text.Substring(1);
        }

        public static IEnumerable<string> GetCamelWords(string text)
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            return Regex.Matches(text, @"[A-Z][^A-Z]*|^[^A-Z]*")
                .Cast<Match>().Select(t => t.Value);
        }
        public static string GetEnglishText(object @object)
        {
            Contract.Requires<ArgumentNullException>(@object != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var text = @object.ToString();

            var words = GetCamelWords(text);

            return string.Join(" ", words);
        }

        public static string AndJoin(params string[] items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (items.Any() == false) return string.Empty;
            if (items.Length == 1) return items.First() ?? string.Empty;

            var output = new StringBuilder();

            output.Append(items.First());

            for (int i = 1; i < items.Length - 1; i++)
            {
                output.Append(", ");
                output.Append(items[i]);
            }

            output.Append(" and ");
            output.Append(items.Last());

            return output.ToString();
        }
        public static string AndJoin(IEnumerable<string> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return AndJoin(items.ToArray());
        }

        public static string Clip(this string text, int length, string moreSymbol = "...")
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Requires<ArgumentException>(length >= 0);
            Contract.Requires<ArgumentNullException>(moreSymbol != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (text.Length <= length) return text;

            if (length < moreSymbol.Length) return moreSymbol.Substring(0, length);

            var trimLength = length - moreSymbol.Length;

            return text.Substring(0, trimLength) + moreSymbol;
        }

        public static string Trim(this string text, params string[] trimTexts)
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var trimStart = TrimStart(text, trimTexts);
            var trimBoth = TrimEnd(trimStart, trimTexts);
            return trimBoth;
        }
        public static string TrimStart(this string text, params string[] trimTexts)
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (trimTexts == null) return text.TrimStart(null);

            while (true)
            {
                var trimText = trimTexts.FirstOrDefault(t => text.StartsWith(t));
                if (trimText == null) break;

                Contract.Assume(trimText.Length <= text.Length);
                text = text.Substring(trimText.Length);
            }

            return text;
        }
        public static string TrimEnd(this string text, params string[] trimTexts)
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (trimTexts == null) return text.TrimEnd(null);

            while (true)
            {
                var trimText = trimTexts.FirstOrDefault(t => text.EndsWith(t));
                if (trimText == null) break;

                Contract.Assume(trimText.Length <= text.Length);
                text = text.Substring(0, text.Length - trimText.Length);
            }

            return text;
        }
    }

    [TestClass]
    public class StringUtilTest
    {
        [TestMethod]
        public void TestContainsAny()
        {
            Assert.IsTrue("abc123".ContainsAny("abc", "123", "ppkk"));

            Assert.IsTrue("abc123".ContainsAny("123", "777"));

            Assert.IsFalse("abc123".ContainsAny("***", "8989"));
        }

        [TestMethod]
        public void TestFirstLetter()
        {
            Assert.AreEqual("Apple", "apple".FirstLetterToUpper());
            Assert.AreEqual("Banana", "banana".FirstLetterToUpper());

            Assert.AreEqual("apple", "Apple".FirstLetterToLower());
            Assert.AreEqual("banana", "Banana".FirstLetterToLower());
        }

        [TestMethod]
        public void TestGetCamelWords()
        {
            var abc = StringUtil.GetCamelWords("AbcBcdCde").ToArray();

            Assert.IsTrue(
                StringUtil.GetCamelWords("AbcBcdCde").SequenceEqual(
                new[] { "Abc", "Bcd", "Cde" }));

            Assert.IsTrue(
                StringUtil.GetCamelWords("pppAbcBcdCde").SequenceEqual(
                new[] { "ppp", "Abc", "Bcd", "Cde" }));
        }

        [TestMethod]
        public void TestGetEnglishText()
        {
            var text = StringUtil.GetEnglishText(StringComparison
                .CurrentCultureIgnoreCase);

            Assert.AreEqual("Current Culture Ignore Case", text);
        }

        [TestMethod]
        public void TestAndJoin()
        {
            var oneWordText = StringUtil.AndJoin("apple");
            Assert.AreEqual(oneWordText, "apple");

            var twoWordsText = StringUtil.AndJoin(
                "apple", "banana");
            Assert.AreEqual(
                "apple and banana", twoWordsText);

            var threeWordsText = StringUtil.AndJoin(
                "apple", "banana", "orange");
            Assert.AreEqual(
                "apple, banana and orange", threeWordsText);
        }

        [TestMethod]
        public void TestClip()
        {
            Assert.AreEqual("123...", "123456789".Clip(6));
            Assert.AreEqual("1234...", "123456789".Clip(7));
            Assert.AreEqual("12345...", "123456789".Clip(8));

            Assert.AreEqual("123456789", "123456789".Clip(9));
            Assert.AreEqual("123456789", "123456789".Clip(10));

            Assert.AreEqual("...", "123456789".Clip(3));
        }

        [TestMethod]
        public void TestTrim()
        {
            Assert.AreEqual("123", "abc123abc".Trim("abc"));
            Assert.AreEqual("123", "abcxyz123xyzabc".Trim("abc", "xyz"));

            Assert.AreEqual("123abc", "abc123abc".TrimStart("abc"));
            Assert.AreEqual("abc123", "abc123abc".TrimEnd("abc"));
        }
    }
}
