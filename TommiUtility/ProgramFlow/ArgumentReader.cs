using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TommiUtility.Test;

namespace TommiUtility.ProgramFlow
{
    public class ArgumentReader
    {
        public ArgumentReader(string[] args)
        {
            Contract.Requires<ArgumentNullException>(args != null);

            this.args = args;
        }
        private readonly string[] args;

        private const string KeyBodyPattern = @"(?<key>[^-:=]+)(?<encode>-c|-b)?";
        private const string KeyPattern = @"^-" + KeyBodyPattern + "$";
        private const string PairPattern = @"^-?" + KeyBodyPattern + "[:=](?<value>.+)$";

        public bool HasKey(string key)
        {
            Contract.Requires<ArgumentNullException>(key != null);

            return GetValues(key).Any();
        }

        public T GetValue<T>(string key)
        {
            Contract.Requires<ArgumentNullException>(key != null);

            return GetValues<T>(key).FirstOrDefault();
        }
        public IEnumerable<T> GetValues<T>(string key)
        {
            Contract.Requires<ArgumentNullException>(key != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            return GetValues(key).Select(t =>
            {
                var value = Convert.ChangeType(t, typeof(T));
                
                if (value == null) return default(T);
                return (T)value;
            });
        }

        public string GetValue(string key)
        {
            Contract.Requires<ArgumentNullException>(key != null);

            return GetValues(key).FirstOrDefault();
        }
        public IEnumerable<string> GetValues(string key)
        {
            Contract.Requires<ArgumentNullException>(key != null);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == null) continue;

                var keyMatch = Regex.Match(arg, KeyPattern, RegexOptions.Singleline);
                if (keyMatch.Success && keyMatch.Groups["key"].Value == key)
                {
                    if (i + 1 < args.Length)
                    {
                        var encode = keyMatch.Groups["encode"].Value;
                        var text = args[i + 1];
                        yield return Decode(text, encode);

                        i += 1;
                        continue;
                    }
                    else
                    {
                        yield return string.Empty;
                        continue;
                    }
                }
                else
                {
                    var pairMatch = Regex.Match(arg, PairPattern, RegexOptions.Singleline);
                    if (pairMatch.Success && pairMatch.Groups["key"].Value == key)
                    {
                        var encode = pairMatch.Groups["encode"].Value;
                        var text = pairMatch.Groups["value"].Value;
                        yield return Decode(text, encode);
                        continue;
                    }
                }
            }
        }

        private string Decode(string text, string encode)
        {
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Ensures(Contract.Result<string>() != null);

            switch (encode)
            {
                case "-c":
                    return Regex.Unescape(text);

                case "-b":
                    byte[] bytes = Convert.FromBase64String(text);
                    return Encoding.UTF8.GetString(bytes);

                default:
                    return text;
            }
        }
    }

    [TestClass]
    public class ArgumentReaderTest
    {
        [TestMethod]
        public void Test()
        {
            var reader = new ArgumentReader(new[]
            {
                "-abc", "123",
                "-abc:234",
                "-abc=345",
                "abc=456",

                "-bcd", "ppp",
                @"-bcd-c:\\",
                @"-bcd-b:YWJj",

                "-cde",
                "-def", "kkk",
                "-def"
            });

            Assert.IsTrue(reader.HasKey("abc"));
            Assert.AreEqual(123, reader.GetValue<int>("abc"));
            Assert.AreEqual("123", reader.GetValue("abc"));
            var abcs = reader.GetValues<int>("abc");
            AssertUtil.SequenceEqual(new[] { 123, 234, 345, 456 }, abcs);

            Assert.IsTrue(reader.HasKey("bcd"));
            var bcds = reader.GetValues("bcd");
            AssertUtil.SequenceEqual(new[] { "ppp", @"\", "abc" }, bcds);

            Assert.IsTrue(reader.HasKey("cde"));
            Assert.AreEqual("-def", reader.GetValue("cde"));
            var cdes = reader.GetValues("cde");
            AssertUtil.SequenceEqual(new string[] { "-def" }, cdes);

            Assert.IsTrue(reader.HasKey("def"));
            var defs = reader.GetValues("def");
            AssertUtil.SequenceEqual(new string[] { "kkk", string.Empty }, defs);

            Assert.IsFalse(reader.HasKey("xyz"));
            Assert.IsNull(reader.GetValue("xyz"));
        }
    }
}
