using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.FileSystem
{
    public static class StreamUtil
    {
        public static byte[] ReadToEnd(this Stream stream)
        {
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
    }

    [TestClass]
    public class StreamUtilTest
    {
        [TestMethod]
        public void Test()
        {
            var fromBytes = Enumerable.Repeat<byte>(1, 1024 * 1024)
                .Concat(Enumerable.Repeat<byte>(2, 1024 * 1024))
                .Concat(Enumerable.Repeat<byte>(3, 1024 * 1024)).ToArray();

            var stream = new MemoryStream(fromBytes);
            
            var toBytes = stream.ReadToEnd();

            Assert.IsTrue(fromBytes.SequenceEqual(toBytes));
        }
    }
}
