using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.ProgramFlow
{
    public class ArgumentReader
    {
        public ArgumentReader(string[] args)
        {
            this.args = args;
        }
        private string[] args;

        private const string KeyBodyPattern = @"(?<key>[^-:=]+)(?<encode>-c|-b)?";
        private const string KeyPattern = @"^-" + KeyBodyPattern + "$";
        private const string PairPattern = @"^-?" + KeyBodyPattern + "[:=](?<value>.+)$";

        public bool HasKey(string key)
        {
            return GetValues(key).Any();
        }

        public T GetValue<T>(string key)
        {
            return GetValues<T>(key).FirstOrDefault();
        }
        public IEnumerable<T> GetValues<T>(string key)
        {
            return GetValues(key).Select(t => (T)Convert.ChangeType(t, typeof(T)));
        }

        public string GetValue(string key)
        {
            return GetValues(key).FirstOrDefault();
        }
        public IEnumerable<string> GetValues(string key)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (i + 1 < args.Length)
                {
                    var keyMatch = Regex.Match(arg, KeyPattern, RegexOptions.Singleline);
                    if (keyMatch.Success && keyMatch.Groups["key"].Value == key)
                    {
                        var encode = keyMatch.Groups["encode"].Value;
                        var text = args[i + 1];
                        yield return Decode(text, encode);

                        i += 1;
                        continue;
                    }
                }

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

        private string Decode(string text, string encode)
        {
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

                "-bcd", "kkk",
                @"-bcd-c:\\",
                @"-bcd-b:YWJj"
            });

            Assert.IsTrue(reader.HasKey("abc"));
            Assert.IsTrue(reader.HasKey("bcd"));
            Assert.IsFalse(reader.HasKey("cde"));

            Assert.AreEqual(123, reader.GetValue<int>("abc"));
            Assert.AreEqual("123", reader.GetValue("abc"));
            
            var abcs = reader.GetValues<int>("abc");
            Assert.IsTrue(new[] { 123, 234, 345, 456 }.SequenceEqual(abcs));

            var bcds = reader.GetValues("bcd");
            Assert.IsTrue(new[] { "kkk", @"\", "abc" }.SequenceEqual(bcds));

            Assert.IsNull(reader.GetValue("cde"));
        }
    }
}
