using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentException>(string.IsNullOrEmpty(serviceUri) == false);
            Contract.Requires<ArgumentException>(string.IsNullOrEmpty(serviceNamespace) == false);

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
            Contract.Requires<ArgumentNullException>(action != null);
            Contract.Requires<ArgumentException>(action.Length > 0);
            Contract.Ensures(Contract.Result<SoapRequest>() != null);

            var envelopeXml = new XmlDocument();

            envelopeXml.LoadXml(@"<soapenv:Envelope "
                + @"xmlns:soapenv=" + "\"" + soapNamespace + "\" "
                + @"xmlns:service=" + "\"" + ServiceNamespace + "\"" + ">"
                + @"</soapenv:Envelope>");

            var documentElement = envelopeXml.DocumentElement;
            Contract.Assume(documentElement != null);

            var headerNode = envelopeXml.CreateElement("soapenv", "Header", soapNamespace);
            documentElement.AppendChild(headerNode);

            var bodyNode = envelopeXml.CreateElement("soapenv", "Body", soapNamespace);
            documentElement.AppendChild(bodyNode);

            var actionNode = envelopeXml.CreateElement("service", action, ServiceNamespace);
            bodyNode.AppendChild(actionNode);

            Contract.Assume(actionNode.OwnerDocument != null);
            return new SoapRequest(this, actionNode);
        }
        public XmlNode Invoke(XmlDocument request)
        {
            Contract.Requires<ArgumentNullException>(request != null);

            var responseText = UploadString(ServiceUri, request.InnerXml);
            
            var responseXml = new XmlDocument();
            responseXml.LoadXml(responseText);

            if (responseXml.DocumentElement == null) throw new InvalidOperationException();

            return FindReturnNode(responseXml.DocumentElement);
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            Contract.Ensures(Contract.Result<WebRequest>() != null);

            var webRequest = base.GetWebRequest(address);
            Contract.Assume(webRequest != null);

            webRequest.Headers.Add("SOAPAction", "\"\"");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Method = "POST";
            webRequest.Timeout = Timeout;

            return webRequest;
        }
        private XmlNode FindReturnNode(XmlNode rootNode)
        {
            Contract.Requires<ArgumentNullException>(rootNode != null);

            List<XmlNode> currNodes = new List<XmlNode>();
            currNodes.Add(rootNode);

            while (currNodes.Count > 0)
            {
                List<XmlNode> nextNodes = new List<XmlNode>();

                foreach (var currNode in currNodes)
                {
                    Contract.Assume(currNode != null);

                    if (currNode.Name == "return")
                    {
                        return currNode;
                    }

                    nextNodes.AddRange(currNode.ChildNodes.Cast<XmlNode>());
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
            Contract.Requires<ArgumentNullException>(client != null);
            Contract.Requires<ArgumentNullException>(actionNode != null);
            Contract.Requires<ArgumentNullException>(actionNode.OwnerDocument != null);

            this.Client = client;
            this.ActionNode = actionNode;
        }

        public readonly SoapClient Client;
        public readonly XmlNode ActionNode;

        public XmlNode Invoke()
        {
            return Client.Invoke(ActionNode.OwnerDocument);
        }
    }
}
