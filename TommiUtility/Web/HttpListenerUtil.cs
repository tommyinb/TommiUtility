using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Web
{
    public static class HttpListenerUtil
    {
        public static void WriteText(this HttpListenerResponse response, string text)
        {
            WriteText(response, text, Encoding.UTF8);
        }

        public static void WriteText(this HttpListenerResponse response, string text, Encoding encoding)
        {
            response.ContentEncoding = encoding;

            var bytes = encoding.GetBytes(text);

            response.ContentLength64 = bytes.LongLength;

            response.OutputStream.Write(bytes, 0, bytes.Length);
        }
    }
}
