using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Web
{
    public sealed class EmailClient : IDisposable
    {
        public EmailClient(string host, int port, bool ssl, string email, string password)
        {
            this.client = new SmtpClient(host, port);

            this.client.EnableSsl = ssl;

            this.clientEmail = email;

            this.client.Credentials = new NetworkCredential(
                email, password);
        }
        public void Dispose()
        {
            this.client.Dispose();
        }

        private SmtpClient client;
        private string clientEmail;

        public void Send(string email, string subject, string content)
        {
            var message = new MailMessage();

            message.From = new MailAddress(clientEmail);
            message.To.Add(email);

            message.Subject = subject;

            var contentType = new ContentType("text/html");
            var alternateView = AlternateView.CreateAlternateViewFromString(
                content, contentType);
            message.AlternateViews.Add(alternateView);

            client.Send(message);
        }
    }

    [TestClass]
    public class EmailClientTest
    {
        [TestMethod]
        public void Test()
        {
            using (var client = new EmailClient(
                "smtp.gmail.com", 587, ssl: true,
                email: "testingtommiutility789@gmail.com",
                password: "TommiUtility789"))
            {
                client.Send("testingtommiutility789@gmail.com",
                    "TommiUtility Test EmailClient",
                    "ABCDEFGHIJKLMNOPQ");
            }
        }
    }
}
