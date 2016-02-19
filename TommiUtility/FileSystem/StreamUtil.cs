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
    public static class StreamUtil
    {
        public static byte[] ReadAllBytes(this Stream stream)
        {
            Contract.Requires<ArgumentNullException>(stream != null);
            Contract.Ensures(Contract.Result<byte[]>() != null);

            var buffer = new byte[1024 * 1024];

            using (var memoryStream = new MemoryStream())
            {
                while (true)
                {
                    var read = stream.Read(buffer, 0, buffer.Length);

                    if (read <= 0) break;

                    memoryStream.Write(buffer, 0, read);
                }

                return memoryStream.ToArray();
            }
        }

        public static string ReadAllText(this Stream stream, Encoding encoding)
        {
            Contract.Requires<ArgumentNullException>(stream != null);
            Contract.Requires<ArgumentNullException>(encoding != null);
            Contract.Ensures(Contract.Result<string>() != null);

            using (var reader = new StreamReader(stream, encoding))
            {
                return reader.ReadToEnd();
            }
        }
    }

    [TestClass]
    public class StreamUtilTest
    {
        [TestMethod]
        public void TestReadAllBytes()
        {
            var fromBytes = Enumerable.Repeat<byte>(1, 1024 * 1024)
                .Concat(Enumerable.Repeat<byte>(2, 1024 * 1024))
                .Concat(Enumerable.Repeat<byte>(3, 1024 * 1024)).ToArray();

            var stream = new MemoryStream(fromBytes);
            
            var toBytes = stream.ReadAllBytes();

            Assert.IsTrue(fromBytes.SequenceEqual(toBytes));
        }

        [TestMethod]
        public void TestReadAllText()
        {
            var inputText = "Testing1234";
            var bytes = Encoding.UTF8.GetBytes(inputText);

            var stream = new MemoryStream(bytes);
            var outputText = stream.ReadAllText(Encoding.UTF8);

            Assert.AreEqual(inputText, outputText);
        }
    }
}
