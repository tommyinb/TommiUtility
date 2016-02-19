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
    public class EscapeJoin
    {
        public EscapeJoin(string seperator = ", ", string escaper = @"\")
        {
            Contract.Requires<ArgumentNullException>(seperator != null);
            Contract.Requires<ArgumentException>(seperator.Length > 0);
            Contract.Requires<ArgumentNullException>(escaper != null);
            Contract.Requires<ArgumentException>(escaper.Length > 0);
            Contract.Requires<ArgumentException>(seperator != escaper);

            this.seperator = seperator;
            this.escaper = escaper;
        }

        private readonly string seperator;
        private readonly string escaper;
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(seperator != null);
            Contract.Invariant(escaper != null);
        }

        public string Join(params string[] items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return Join((IEnumerable<string>)items);
        }
        public string Join(IEnumerable<string> items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var regexEscaper = Regex.Escape(escaper);
            var regexSeperator = Regex.Escape(seperator);

            var escapedParts = items.Select(Escape).ToArray();
            return string.Join(seperator, escapedParts);
        }
        public IEnumerable<string> Split(string text)
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            var regexSeperator = Regex.Escape(seperator);
            var regexEscapedEscaper = Regex.Escape(escaper + escaper);
            var regexEscapedSeperator = Regex.Escape(escaper + seperator);

            var seperatorMatches = Regex.Matches(text,
                "(" + regexEscapedEscaper + "|" + regexEscapedSeperator + "|" + regexSeperator + ")");

            var seperatorIndexes = seperatorMatches.Cast<Match>()
                .Where(t => t.Value == seperator)
                .Select(t => t.Index).ToArray();

            var itemStartIndex = 0;
            foreach (var seperatorIndex in seperatorIndexes)
            {
                var beforeSeperatorText = text.Substring(itemStartIndex, seperatorIndex - itemStartIndex);
                var beforeSeperatorItem = Unescape(beforeSeperatorText);
                yield return beforeSeperatorItem;

                itemStartIndex = seperatorIndex + seperator.Length;
            }

            var lastItemText = text.Substring(itemStartIndex);
            var lastItem = Unescape(lastItemText);
            yield return lastItem;
        }

        public string Escape(string plainText)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            if (plainText == null) return string.Empty;

            var regexEscaper = Regex.Escape(escaper);
            var regexSeperator = Regex.Escape(seperator);

            return Regex.Replace(plainText,
                "(" + regexEscaper + "|" + regexSeperator + ")",
                s => escaper + s.Value);
        }
        public string Unescape(string escapedText)
        {
            Contract.Requires<ArgumentNullException>(escapedText != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var regexEscapedEscaper = Regex.Escape(escaper + escaper);
            var regexEscapedSeperator = Regex.Escape(escaper + seperator);

            return Regex.Replace(escapedText,
                "(" + regexEscapedEscaper + "|" + regexEscapedSeperator + ")",
                s => s.Value.Substring(escaper.Length));
        }
    }

    [TestClass]
    public class EscapeJoinTest
    {
        [TestMethod]
        public void Test()
        {
            var escapeJoin = new EscapeJoin(seperator: ", ", escaper: @"\");

            var text = escapeJoin.Join("123", "234", "345, ", ", ,456", null, @"5\67");

            Assert.AreEqual(@"123, 234, 345\, , \, ,456, , 5\\67", text);

            var values = escapeJoin.Split(text).ToArray();

            Assert.IsTrue(new[] { "123", "234", "345, ", ", ,456", string.Empty, @"5\67" }
                .SequenceEqual(values));
        }
    }
}
