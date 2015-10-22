using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using System.IO;

namespace TommiUtility.Xml
{
    public static class XmlUtil
    {
        public static string Serialize<T>(T @object)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, @object);

                return writer.ToString();
            }
        }
        public static T Deserialize<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(xml))
            {
                var @object = serializer.Deserialize(reader);

                return (T)@object;
            }
        }

        public static void WriteFile<T>(T @object, string filePath)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var fileStream = File.Open(filePath, FileMode.Create))
            {
                serializer.Serialize(fileStream, @object);
            }
        }
        public static T ReadFile<T>(string filePath)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var fileStream = File.OpenRead(filePath))
            {
                var @object = serializer.Deserialize(fileStream);

                return (T)@object;
            }
        }

        public static XmlElement AddElement(this XmlNode node, string name)
        {
            var element = node.OwnerDocument.CreateElement(name);

            node.AppendChild(element);

            return element;
        }
        public static XmlElement AddElement(this XmlNode node, string name, string value)
        {
            var element = node.OwnerDocument.CreateElement(name);
            element.InnerText = value;

            node.AppendChild(element);

            return element;
        }
    }

    [TestClass]
    public class XmlUtilTest
    {
        public class TestClass
        {
            public string Text { get; set; }
        }

        [TestMethod]
        public void TestSerialize()
        {
            var tester = new TestClass { Text = "123" };

            var xml = XmlUtil.Serialize(tester);

            var testee = XmlUtil.Deserialize<TestClass>(xml);

            Assert.AreEqual(tester.Text, testee.Text);
        }

        [TestMethod]
        public void TestFile()
        {
            var tester = new TestClass { Text = "123" };

            XmlUtil.WriteFile(tester, "test.xml");

            var testee = XmlUtil.ReadFile<TestClass>("test.xml");

            File.Delete("test.xml");

            Assert.AreEqual(tester.Text, testee.Text);
        }

        [TestMethod]
        public void TestAddElement()
        {
            var document = new XmlDocument();
            document.LoadXml("<document></document>");

            document.DocumentElement.AddElement("abc", "123");
            Assert.IsTrue(document.DocumentElement["abc"].InnerText == "123");

            document.DocumentElement.AddElement("xyz");
            Assert.IsNotNull(document.DocumentElement["xyz"]);
        }
    }
}
