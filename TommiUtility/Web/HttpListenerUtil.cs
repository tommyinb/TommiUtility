using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(response != null);
            Contract.Requires<ArgumentNullException>(response.OutputStream != null);
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Requires<ArgumentException>(text.Length > 0);

            WriteText(response, text, Encoding.UTF8);
        }

        public static void WriteText(this HttpListenerResponse response, string text, Encoding encoding)
        {
            Contract.Requires<ArgumentNullException>(response != null);
            Contract.Requires<ArgumentNullException>(response.OutputStream != null);
            Contract.Requires<ArgumentNullException>(text != null);
            Contract.Requires<ArgumentException>(text.Length > 0);
            Contract.Requires<ArgumentNullException>(encoding != null);

            response.ContentEncoding = encoding;

            var bytes = encoding.GetBytes(text);

            response.ContentLength64 = bytes.LongLength;
            response.OutputStream.Write(bytes, 0, bytes.Length);
        }
    }
}
