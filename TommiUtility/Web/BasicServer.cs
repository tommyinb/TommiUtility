using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Web
{
    public sealed class BasicServer : IDisposable
    {
        public readonly List<IServerResponse> Responses = new List<IServerResponse>();

        public BasicServer(int port, string directory = ".")
        {
            Contract.Requires<ArgumentException>(port > 0);
            Contract.Requires<ArgumentException>(port <= 65535);
            Contract.Requires<ArgumentException>(string.IsNullOrEmpty(directory) == false);

            Responses.Add(new IndexResponse(directory));
            Responses.Add(new FileResponse(directory));
            Responses.Add(new BadRequestResponse());

            Contract.Assume(listener.Prefixes != null);
            listener.Prefixes.Add("http://*:" + port + "/");
            listener.Start();

            var thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Start();
        }
        public void Dispose()
        {
            listener.Close();
        }

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(listener != null);
        }

        private readonly HttpListener listener = new HttpListener();
        private void Listen()
        {
            while (true)
            {
                HttpListenerContext context;
                try
                {
                    context = listener.GetContext();
                    Contract.Assume(context != null);

                    try
                    {
                        foreach (var response in Responses)
                        {
                            if (response != null)
                            {
                                Contract.Assume(context.Request != null);
                                Contract.Assume(context.Request.Url != null);

                                if (response.IsValid(context.Request))
                                {
                                    Contract.Assume(context.Request.Url != null);
                                    Contract.Assume(context.Response != null);
                                    Contract.Assume(context.Response.OutputStream != null);

                                    response.Response(context);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception) { }

                    try
                    {
                        Contract.Assume(context.Response != null);

                        context.Response.Close();
                    }
                    catch (Exception) { }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }
    }

    [ContractClass(typeof(IServerResponseContract))]
    public interface IServerResponse
    {
        bool IsValid(HttpListenerRequest request);

        void Response(HttpListenerContext context);
    }

    [ContractClassFor(typeof(IServerResponse))]
    public abstract class IServerResponseContract : IServerResponse
    {
        public bool IsValid(HttpListenerRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null);
            Contract.Requires<ArgumentException>(request.Url != null);
            return false;
        }
        public void Response(HttpListenerContext context)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentException>(context.Request != null);
            Contract.Requires<ArgumentException>(context.Request.Url != null);
            Contract.Requires<ArgumentException>(context.Response != null);
            Contract.Requires<ArgumentException>(context.Response.OutputStream != null);
        }
    }

    public class IndexResponse : IServerResponse
    {
        public IndexResponse(string directory)
        {
            Contract.Requires<ArgumentNullException>(directory != null);
            Contract.Requires<ArgumentException>(directory.Length > 0);

            this.directory = directory;
        }
        private readonly string directory;

        private readonly string[] indexFiles = new[] { "index.html", "index.htm" };
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(indexFiles != null);
            Contract.Invariant(indexFiles.Any());
        }

        public bool IsValid(HttpListenerRequest request)
        {
            if (request.Url.AbsolutePath == "/")
            {
                var indexPaths = indexFiles.Select(t => Path.Combine(directory, t));
                return indexPaths.Any(File.Exists);
            }
            else
            {
                return false;
            }
        }
        public void Response(HttpListenerContext context)
        {
            var indexFile = indexFiles.First(t =>
            {
                var indexPath = Path.Combine(directory, t);
                return File.Exists(indexPath);
            });

            context.Response.Redirect(indexFile);
        }
    }

    public class FileResponse : IServerResponse
    {
        public FileResponse(string directory)
        {
            Contract.Requires<ArgumentNullException>(directory != null);
            Contract.Requires<ArgumentException>(directory.Length > 0);

            this.directory = directory;
        }
        private readonly string directory;

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(directory != null);
        }

        public bool IsValid(HttpListenerRequest request)
        {
            var relativeFilePath = request.Url.AbsolutePath.TrimStart('/').Replace("/", @"\");
            var localFilePath = Path.Combine(directory, relativeFilePath);

            if (localFilePath.Length <= 0) return false;

            return File.Exists(localFilePath);
        }
        public void Response(HttpListenerContext context)
        {
            var relativeFilePath = context.Request.Url.AbsolutePath.TrimStart('/').Replace("/", @"\");
            var localFilePath = Path.Combine(directory, relativeFilePath);
            if (localFilePath.Length <= 0) throw new ArgumentException();

            var fileExtension = Path.GetExtension(localFilePath);
            var extensionType = GetFileType(fileExtension);

            switch (extensionType.Item1)
            {
                case FileType.Text:
                    var text = File.ReadAllText(localFilePath);
                    context.Response.ContentEncoding = Encoding.UTF8;
                    var textBytes = Encoding.UTF8.GetBytes(text);

                    context.Response.ContentType = extensionType.Item2;
                    context.Response.ContentLength64 = textBytes.LongLength;
                    context.Response.OutputStream.Write(textBytes, 0, textBytes.Length);
                    break;

                case FileType.Bytes:
                default:
                    var fileBytes = File.ReadAllBytes(localFilePath);

                    context.Response.ContentType = extensionType.Item2;
                    context.Response.ContentLength64 = fileBytes.LongLength;
                    context.Response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                    break;
            }
        }

        private enum FileType { Text, Bytes };
        [ContractVerification(false)]
        private Tuple<FileType, string> GetFileType(string extension)
        {
            Contract.Ensures(Contract.Result<Tuple<FileType, string>>() != null);

            switch (extension)
            {
                case ".html": return Tuple.Create(FileType.Text, MediaTypeNames.Text.Html);
                case ".txt": return Tuple.Create(FileType.Text, MediaTypeNames.Text.Plain);
                case ".xml": return Tuple.Create(FileType.Text, MediaTypeNames.Text.Xml);
                case ".css": return Tuple.Create(FileType.Text, "text/css");
                case ".js": return Tuple.Create(FileType.Text, "text/javascript");

                case ".exe": return Tuple.Create(FileType.Bytes, MediaTypeNames.Application.Octet);
                case ".zip": return Tuple.Create(FileType.Bytes, MediaTypeNames.Application.Zip);
                case ".7z": return Tuple.Create(FileType.Bytes, "application/x-7z-compressed");
                case ".rar": return Tuple.Create(FileType.Bytes, "application/x-rar-compressed");
                case ".pdf": return Tuple.Create(FileType.Bytes, MediaTypeNames.Application.Pdf);

                case ".png": return Tuple.Create(FileType.Bytes, "image/png");
                case ".jpeg": return Tuple.Create(FileType.Bytes, MediaTypeNames.Image.Jpeg);
                case ".jpg": return Tuple.Create(FileType.Bytes, MediaTypeNames.Image.Jpeg);
                case ".gif": return Tuple.Create(FileType.Bytes, MediaTypeNames.Image.Gif);
                case ".tiff": return Tuple.Create(FileType.Bytes, MediaTypeNames.Image.Tiff);
                case ".bmp": return Tuple.Create(FileType.Bytes, "image/bmp");
                case ".svg": return Tuple.Create(FileType.Bytes, "image/svg+xml");
                case ".ico": return Tuple.Create(FileType.Bytes, "image/x-icon");

                case ".eot": return Tuple.Create(FileType.Bytes, "application/vnd.ms-fontobject");
                case ".otf": return Tuple.Create(FileType.Bytes, "application/font-sfnt");
                case ".ttf": return Tuple.Create(FileType.Bytes, "application/font-sfnt");
                case ".woff": return Tuple.Create(FileType.Bytes, "application/font-woff");
                
                default: return Tuple.Create(FileType.Bytes, "application/octet-stream");
            }
        }
    }

    public class BadRequestResponse : IServerResponse
    {
        public bool IsValid(HttpListenerRequest request)
        {
            return true;
        }

        public void Response(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            context.Response.ContentEncoding = Encoding.UTF8;

            var bytes = Encoding.UTF8.GetBytes("bad request");
            context.Response.ContentLength64 = bytes.LongLength;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }
    }

    [TestClass]
    public class BasicServerTest
    {
        [TestMethod]
        public void Test()
        {
            using (var server = new BasicServer(12345))
            using (var client = new WebClient())
            {
                File.WriteAllText("index.html", "abcd");

                var test = client.DownloadString(
                    "http://" + "localhost:12345/index.html");
                
                Assert.AreEqual("abcd", client.DownloadString(
                    "http://" + "localhost:12345/index.html"));

                Assert.AreEqual("abcd", client.DownloadString(
                    "http://" + "localhost:12345"));

                try
                {
                    client.DownloadString("http://"
                        + "localhost:12345/badbadbad");

                    Assert.Fail();
                }
                catch (WebException) { }

                File.Delete("index.html");
            }
        }
    }
}
