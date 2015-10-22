using System;
using System.Collections.Generic;
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
        public List<IServerResponse> Responses { get; private set; }

        public BasicServer(int port, string directory = ".")
        {
            Responses = new List<IServerResponse>();
            Responses.Add(new IndexResponse(directory));
            Responses.Add(new FileResponse(directory));
            Responses.Add(new BadRequestResponse());

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

        private HttpListener listener = new HttpListener();
        private void Listen()
        {
            while (true)
            {
                try
                {
                    var context = listener.GetContext();

                    try
                    {
                        foreach (var response in Responses)
                        {
                            if (response.IsValid(context.Request))
                            {
                                response.Response(context);
                                break;
                            }
                        }
                    }
                    catch (Exception) { }

                    try
                    {
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

    public interface IServerResponse
    {
        bool IsValid(HttpListenerRequest request);

        void Response(HttpListenerContext context);
    }

    public class IndexResponse : IServerResponse
    {
        public IndexResponse(string directory)
        {
            indexFiles.Add(Path.Combine(directory, "index.html"));
            indexFiles.Add(Path.Combine(directory, "index.htm"));
        }
        private List<string> indexFiles = new List<string>();

        public bool IsValid(HttpListenerRequest request)
        {
            return request.Url.AbsolutePath == "/"
                && indexFiles.Any(File.Exists);
        }

        public void Response(HttpListenerContext context)
        {
            var indexFile = indexFiles.First(File.Exists);

            context.Response.Redirect(indexFile);
        }
    }

    public class FileResponse : IServerResponse
    {
        public FileResponse(string directory)
        {
            this.directory = directory;

            var addType = new Action<string, FileType, string>((extension, fileType, mimeType) =>
                extensionTypes.Add(extension, Tuple.Create(fileType, mimeType)));

            addType(".html", FileType.Text, MediaTypeNames.Text.Html);
            addType(".txt", FileType.Text, MediaTypeNames.Text.Plain);
            addType(".xml", FileType.Text, MediaTypeNames.Text.Xml);
            addType(".css", FileType.Text, "text/css");
            addType(".js", FileType.Text, "text/javascript");

            addType(".exe", FileType.Bytes, MediaTypeNames.Application.Octet);
            addType(".zip", FileType.Bytes, MediaTypeNames.Application.Zip);
            addType(".7z", FileType.Bytes, "application/x-7z-compressed");
            addType(".rar", FileType.Bytes, "application/x-rar-compressed");
            addType(".pdf", FileType.Bytes, MediaTypeNames.Application.Pdf);

            addType(".png", FileType.Bytes, "image/png");
            addType(".jpeg", FileType.Bytes, MediaTypeNames.Image.Jpeg);
            addType(".jpg", FileType.Bytes, MediaTypeNames.Image.Jpeg);
            addType(".gif", FileType.Bytes, MediaTypeNames.Image.Gif);
            addType(".tiff", FileType.Bytes, MediaTypeNames.Image.Tiff);
            addType(".bmp", FileType.Bytes, "image/bmp");
            addType(".svg", FileType.Bytes, "image/svg+xml");
            addType(".ico", FileType.Bytes, "image/x-icon");

            addType(".eot", FileType.Bytes, "application/vnd.ms-fontobject");
            addType(".otf", FileType.Bytes, "application/font-sfnt");
            addType(".ttf", FileType.Bytes, "application/font-sfnt");
            addType(".woff", FileType.Bytes, "application/font-woff");
        }
        private string directory;

        private Dictionary<string, Tuple<FileType, string>> extensionTypes =
            new Dictionary<string, Tuple<FileType, string>>();
        private enum FileType { Text, Bytes };

        public bool IsValid(HttpListenerRequest request)
        {
            var relativeFilePath = request.Url.AbsolutePath.TrimStart('/').Replace("/", @"\");
            var fileExtension = Path.GetExtension(relativeFilePath);

            var localFilePath = Path.Combine(directory, relativeFilePath);

            return extensionTypes.ContainsKey(fileExtension) && File.Exists(localFilePath);
        }
        public void Response(HttpListenerContext context)
        {
            var relativeFilePath = context.Request.Url.AbsolutePath.TrimStart('/').Replace("/", @"\");
            var localFilePath = Path.Combine(directory, relativeFilePath);

            var fileExtension = Path.GetExtension(localFilePath);
            var extensionType = extensionTypes[fileExtension];

            switch (extensionType.Item1)
            {
                case FileType.Text:
                    var text = File.ReadAllText(localFilePath);

                    context.Response.ContentType = extensionType.Item2;
                    context.Response.ContentEncoding = Encoding.UTF8;

                    var textBytes = Encoding.UTF8.GetBytes(text);
                    context.Response.ContentLength64 = textBytes.LongLength;
                    context.Response.OutputStream.Write(textBytes, 0, textBytes.Length);

                    break;

                case FileType.Bytes:
                    var fileBytes = File.ReadAllBytes(localFilePath);

                    context.Response.ContentType = extensionType.Item2;

                    context.Response.ContentLength64 = fileBytes.LongLength;
                    context.Response.OutputStream.Write(fileBytes, 0, fileBytes.Length);

                    break;
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
