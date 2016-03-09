using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics.Contracts;
using TommiUtility.Test;
using iBoss2.Util;

namespace TommiUtility.Xml
{
    public static class XmlUtil
    {
        public static string Serialize<T>(T @object)
        {
            Contract.Requires<ArgumentNullException>(@object != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var serializer = new XmlSerializer(typeof(T));

            var stringWriter = new StringWriter();
            var xmlSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                CloseOutput = true
            };
            using (var xmlWriter = XmlWriter.Create(stringWriter, xmlSettings))
            {
                var xmlNamespaces = new XmlSerializerNamespaces();
                xmlNamespaces.Add(string.Empty, string.Empty);

                serializer.Serialize(xmlWriter, @object, xmlNamespaces);

                return stringWriter.ToString();
            }
        }
        public static T Deserialize<T>(string xml)
        {
            Contract.Requires<ArgumentNullException>(xml != null);

            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
            {
                var @object = serializer.Deserialize(reader);

                if (@object == null) return default(T);

                return (T)@object;
            }
        }

        public static void WriteFile<T>(T @object, string filePath)
        {
            Contract.Requires<ArgumentNullException>(@object != null);
            Contract.Requires<ArgumentNullException>(filePath != null);

            var xml = Serialize(@object);

            File.WriteAllText(filePath, xml);
        }
        public static T ReadFile<T>(string filePath)
        {
            Contract.Requires<ArgumentNullException>(filePath != null);
            Contract.Requires<ArgumentException>(filePath.Length > 0);

            var xml = File.ReadAllText(filePath);

            return Deserialize<T>(xml);
        }

        public static XmlElement AddElement(this XmlNode node, string name)
        {
            Contract.Requires<ArgumentNullException>(node != null);
            Contract.Requires<ArgumentNullException>(node.OwnerDocument != null);
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Ensures(Contract.Result<XmlElement>() != null);

            var element = node.OwnerDocument.CreateElement(name);

            node.AppendChild(element);

            return element;
        }
        public static XmlElement AddElement(this XmlNode node, string name, string value)
        {
            Contract.Requires<ArgumentNullException>(node != null);
            Contract.Requires<ArgumentException>(node.OwnerDocument != null);
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Ensures(Contract.Result<XmlElement>() != null);

            var element = node.OwnerDocument.CreateElement(name);
            element.InnerText = value;

            node.AppendChild(element);

            return element;
        }

        public static string GetIndentedXml(this XmlDocument xmlDocument)
        {
            Contract.Requires<NullReferenceException>(xmlDocument != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var stringBuilder = new StringBuilder();
            
            var writerSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };
            using (var writer = XmlWriter.Create(stringBuilder, writerSettings))
            {
                xmlDocument.Save(writer);
            }

            return stringBuilder.ToString();
        }
    }

    [TestClass]
    public class XmlUtilTest
    {
        [TestMethod]
        public void TestSerialize()
        {
            var tester = new[] { Box.Create("123"), Box.Create("234") };

            var xml = XmlUtil.Serialize(tester);
            var testee = XmlUtil.Deserialize<Box<string>[]>(xml);
            
            Assert.IsNotNull(testee);
            Contract.Assume(testee != null);
            AssertUtil.SequenceEqual(tester, testee, (x, y) => x.Item == y.Item);
        }

        [TestMethod]
        public void TestFile()
        {
            var tester = Box.Create("123", "234");

            XmlUtil.WriteFile(tester, "test.xml");

            var testee = XmlUtil.ReadFile<Box<string, string>>("test.xml");

            File.Delete("test.xml");

            Assert.IsNotNull(testee);
            Contract.Assume(testee != null);

            Assert.AreEqual(tester.Item1, testee.Item1);
            Assert.AreEqual(tester.Item2, testee.Item2);
        }

        [TestMethod]
        public void TestAddElement()
        {
            var document = new XmlDocument();
            document.LoadXml("<document></document>");

            var documentElement = document.DocumentElement;
            Contract.Assume(documentElement != null);
            
            Contract.Assume(documentElement.OwnerDocument != null);
            documentElement.AddElement("abc", "123");
            
            var abcNode = document.DocumentElement["abc"];
            Assert.IsNotNull(abcNode);
            Contract.Assume(abcNode != null);
            Assert.AreEqual("123", abcNode.InnerText);

            Contract.Assume(documentElement.OwnerDocument != null);
            documentElement.AddElement("xyz");

            var xyzNode = document.DocumentElement["xyz"];
            Assert.IsNotNull(xyzNode);
        }

        [TestMethod]
        public void TestGetIndentedXml()
        {
            var document = new XmlDocument();
            document.LoadXml("<abc><bcd>1</bcd><cde>2</cde></abc>");

            var xml = document.GetIndentedXml();
            Assert.AreEqual("<abc>\r\n  <bcd>1</bcd>\r\n  <cde>2</cde>\r\n</abc>", xml);
        }
    }
}
