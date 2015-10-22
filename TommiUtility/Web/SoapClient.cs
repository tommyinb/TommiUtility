using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Web
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class SoapClient : WebClient
    {
        public SoapClient(string serviceUri, string serviceNamespace)
        {
            this.ServiceUri = serviceUri;
            this.ServiceNamespace = serviceNamespace;

            this.Timeout = 10 * 1000;

            this.Encoding = Encoding.UTF8;
        }
        
        public string ServiceUri { get; private set; }
        public string ServiceNamespace { get; private set; }
        private const string soapNamespace =
            "http://" + "schemas.xmlsoap.org/soap/envelope/";
        public int Timeout { get; set; }

        public SoapRequest CreateRequest(string action)
        {
            var envelopeXml = new XmlDocument();

            envelopeXml.LoadXml(@"<soapenv:Envelope "
                + @"xmlns:soapenv=" + "\"" + soapNamespace + "\" "
                + @"xmlns:service=" + "\"" + ServiceNamespace + "\"" + ">"
                + @"</soapenv:Envelope>");

            var headerNode = envelopeXml.CreateElement(
                "soapenv", "Header", soapNamespace);
            envelopeXml.DocumentElement.AppendChild(headerNode);

            var bodyNode = envelopeXml.CreateElement(
                "soapenv", "Body", soapNamespace);
            envelopeXml.DocumentElement.AppendChild(bodyNode);

            var actionNode = envelopeXml.CreateElement(
                "service", action, ServiceNamespace);
            bodyNode.AppendChild(actionNode);

            return new SoapRequest(this, actionNode);
        }
        public XmlNode Invoke(XmlDocument request)
        {
            var responseText = this.UploadString(
                ServiceUri, request.InnerXml);
            var responseXml = new XmlDocument();
            responseXml.LoadXml(responseText);

            return FindReturnNode(
                responseXml.DocumentElement);
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            var webRequest = base.GetWebRequest(address);

            webRequest.Headers.Add("SOAPAction", "\"\"");

            webRequest.ContentType = "text/xml;charset=\"utf-8\"";

            webRequest.Method = "POST";

            webRequest.Timeout = Timeout;

            return webRequest;
        }
        private XmlNode FindReturnNode(XmlNode rootNode)
        {
            List<XmlNode> currNodes = new List<XmlNode>();
            currNodes.Add(rootNode);

            while (currNodes.Count > 0)
            {
                List<XmlNode> nextNodes = new List<XmlNode>();

                foreach (var currNode in currNodes)
                {
                    if (currNode.Name == "return")
                    {
                        return currNode;
                    }

                    nextNodes.AddRange(currNode
                        .ChildNodes.Cast<XmlNode>());
                }

                currNodes.Clear();
                currNodes.AddRange(nextNodes);
            }

            return null;
        }
    }

    public class SoapRequest
    {
        public SoapRequest(SoapClient client, XmlNode actionNode)
        {
            this.Client = client;

            this.ActionNode = actionNode;
        }

        public SoapClient Client { get; private set; }
        public XmlNode ActionNode { get; private set; }

        public XmlNode Invoke()
        {
            return Client.Invoke(ActionNode.OwnerDocument);
        }
    }
}
