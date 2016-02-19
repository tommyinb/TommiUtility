using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(host != null);
            Contract.Requires<ArgumentException>(host.Length > 0);
            Contract.Requires<ArgumentException>(port > 0);
            Contract.Requires<ArgumentException>(port <= 65535);
            Contract.Requires<ArgumentNullException>(email != null);
            Contract.Requires<ArgumentException>(email.Length > 0);

            client = new SmtpClient(host, port);
            client.EnableSsl = ssl;

            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(email, password);
            
            clientEmail = email;
        }
        public void Dispose()
        {
            client.Dispose();
        }

        private readonly SmtpClient client;
        private readonly string clientEmail;
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(client != null);

            Contract.Invariant(clientEmail != null);
            Contract.Invariant(clientEmail.Length > 0);
        }

        public void Send(string address, string subject, string content)
        {
            Contract.Requires<ArgumentException>(string.IsNullOrEmpty(address) == false);
            Contract.Requires<ArgumentNullException>(subject != null);
            Contract.Requires<ArgumentNullException>(content != null);

            var message = new MailMessage();

            message.From = new MailAddress(clientEmail);
            message.To.Add(address);

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
