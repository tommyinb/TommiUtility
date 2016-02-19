using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Web
{
    public sealed class SimpleServer : IDisposable
    {
        public SimpleServer(int port, Action<HttpListenerContext> action)
        {
            Contract.Requires<ArgumentException>(port > 0);
            Contract.Requires<ArgumentException>(port <= 65535);
            Contract.Requires<ArgumentNullException>(action != null);

            this.action = action;

            Contract.Assume(listener.Prefixes != null);
            listener.Prefixes.Add("http://*:" + port + "/");
            listener.Start();

            var thread = new Thread(this.Run);
            thread.IsBackground = true;
            thread.Start();
        }
        public void Dispose()
        {
            listener.Close();
        }

        private readonly Action<HttpListenerContext> action;
        private readonly HttpListener listener = new HttpListener();
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(action != null);
            Contract.Invariant(listener != null);
        }

        private void Run()
        {
            while (true)
            {
                HttpListenerContext context;
                try
                {
                    context = listener.GetContext();
                    Contract.Assume(context != null);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (InvalidOperationException)
                {
                    break;
                }
                catch (Exception e)
                {
                    OnError(e);
                    continue;
                }

                try
                {
                    action.Invoke(context);
                }
                catch (Exception e)
                {
                    OnError(e);
                }

                try
                {
                    Contract.Assume(context.Response != null);
                    context.Response.Close();
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            }
        }
        public event ThreadExceptionEventHandler Error;
        private void OnError(Exception exception)
        {
            if (Error != null)
            {
                Error(this, new ThreadExceptionEventArgs(exception));
            }
        }
    }

    [TestClass]
    public class SimpleServerTest
    {
        [TestMethod]
        public void Test()
        {
            var server = new SimpleServer(port: 8123, action: t =>
            {
                t.Response.ContentEncoding = Encoding.UTF8;

                var bytes = Encoding.UTF8.GetBytes(
                    t.Request.Url.PathAndQuery);

                t.Response.ContentLength64 = bytes.Length;

                t.Response.OutputStream.Write(bytes, 0, bytes.Length);
            });

            using (var client = new WebClient())
            {
                var path = client.DownloadString(
                    "http://" + "localhost:8123/abcdef?a=123");

                Assert.AreEqual("/abcdef?a=123", path);
            }

            server.Dispose();
        }
    }
}
