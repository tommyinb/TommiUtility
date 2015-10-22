using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Text
{
    public class EscapeJoin
    {
        public EscapeJoin()
        {
            Seperator = ", ";
            Escaper = @"\";
        }

        public string Seperator { get; set; }
        public string Escaper { get; set; }

        public string Join(params string[] items)
        {
            return Join((IEnumerable<string>)items);
        }
        public string Join(IEnumerable<string> items)
        {
            if (Seperator == Escaper)
            {
                throw new InvalidOperationException();
            }

            var regexEscaper = Regex.Escape(Escaper);
            var regexSeperator = Regex.Escape(Seperator);

            var escapedParts = items.Select(Escape).ToArray();
            return string.Join(Seperator, escapedParts);
        }
        public IEnumerable<string> Split(string text)
        {
            if (Seperator == Escaper)
            {
                throw new InvalidOperationException();
            }

            var regexSeperator = Regex.Escape(Seperator);
            var regexEscapedEscaper = Regex.Escape(Escaper + Escaper);
            var regexEscapedSeperator = Regex.Escape(Escaper + Seperator);

            var seperatorMatches = Regex.Matches(text,
                "(" + regexEscapedEscaper + "|" + regexEscapedSeperator + "|" + regexSeperator + ")");

            var seperatorIndexes = seperatorMatches.Cast<Match>()
                .Where(t => t.Value == Seperator)
                .Select(t => t.Index).ToArray();

            var itemStartIndex = 0;
            foreach (var seperatorIndex in seperatorIndexes)
            {
                var beforeSeperatorText = text.Substring(itemStartIndex, seperatorIndex - itemStartIndex);
                var beforeSeperatorItem = Unescape(beforeSeperatorText);
                yield return beforeSeperatorItem;

                itemStartIndex = seperatorIndex + Seperator.Length;
            }

            var lastItemText = text.Substring(itemStartIndex);
            var lastItem = Unescape(lastItemText);
            yield return lastItem;
        }

        public string Escape(string plainText)
        {
            if (Seperator == Escaper)
            {
                throw new InvalidOperationException();
            }

            if (plainText != null)
            {
                var regexEscaper = Regex.Escape(Escaper);
                var regexSeperator = Regex.Escape(Seperator);

                return Regex.Replace(plainText,
                    "(" + regexEscaper + "|" + regexSeperator + ")",
                    s => Escaper + s.Value);
            }
            else
            {
                return string.Empty;
            }
        }
        public string Unescape(string escapedText)
        {
            if (Seperator == Escaper)
            {
                throw new InvalidOperationException();
            }

            var regexEscapedEscaper = Regex.Escape(Escaper + Escaper);
            var regexEscapedSeperator = Regex.Escape(Escaper + Seperator);

            return Regex.Replace(escapedText,
                "(" + regexEscapedEscaper + "|" + regexEscapedSeperator + ")",
                s => s.Value.Substring(Escaper.Length));
        }
    }

    [TestClass]
    public class EscapeJoinTest
    {
        [TestMethod]
        public void Test()
        {
            var escapeJoin = new EscapeJoin { Seperator = ", ", Escaper = @"\" };

            var text = escapeJoin.Join("123", "234", "345, ", ", ,456", null, @"5\67");

            Assert.AreEqual(@"123, 234, 345\, , \, ,456, , 5\\67", text);

            var values = escapeJoin.Split(text).ToArray();

            Assert.IsTrue(new[] { "123", "234", "345, ", ", ,456", string.Empty, @"5\67" }
                .SequenceEqual(values));
        }
    }
}
