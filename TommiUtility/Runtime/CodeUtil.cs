using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Runtime
{
    public static class CodeUtil
    {
        public static string GetName<T>(Expression<Func<T>> expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (expression.Body is MemberExpression == false) throw new InvalidCastException();

            var memberExpression = (MemberExpression)expression.Body;
            var memberInfo = memberExpression.Member;
            return memberInfo.Name;
        }

        public static string GetName(this Type type, CodeTypeStyle style = CodeTypeStyle.SimpleName)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            Contract.Ensures(Contract.Result<string>() != null);

            var codeDomProvider = CodeDomProvider.CreateProvider("C#");
            
            if (codeDomProvider == null)
            {
                switch (style)
                {
                    case CodeTypeStyle.SimpleName: return type.Name;
                    case CodeTypeStyle.FullName: return type.FullName;
                    default: throw new InvalidEnumArgumentException();
                }
            }

            var typeReference = new CodeTypeReference(type);
            var typeReferenceExpression = new CodeTypeReferenceExpression(typeReference);

            using (var writer = new StringWriter())
            {
                var generatorOptions = new CodeGeneratorOptions();
                codeDomProvider.GenerateCodeFromExpression(typeReferenceExpression, writer, generatorOptions);
                var fullName = writer.ToString();

                switch (style)
                {
                    case CodeTypeStyle.SimpleName:
                        return Regex.Replace(fullName, @"(?:\w+\.)+(\w+)", "$1");

                    case CodeTypeStyle.FullName:
                        return fullName;

                    default: throw new InvalidEnumArgumentException();
                }
            }
        }

        public static T GetValue<T>(this Type type, Expression<Func<T>> expression)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            Contract.Requires<ArgumentNullException>(expression != null);

            var name = GetName(expression);

            var searchType = type;
            while (searchType != null && searchType != typeof(object))
            {
                var propertyInfo = searchType.GetProperty(name,
                    BindingFlags.Public | BindingFlags.Public
                    | BindingFlags.GetProperty | BindingFlags.Static);

                if (propertyInfo == null)
                {
                    searchType = searchType.BaseType;
                    continue;
                }

                var value = propertyInfo.GetValue(null);

                if (value == null) return default(T);

                return (T)value;
            }

            throw new ArgumentException();
        }
    }

    public enum CodeTypeStyle
    {
        SimpleName, FullName
    }

    [TestClass]
    public class CodeUtilTest
    {
        [TestMethod]
        public void TestGetMemberName()
        {
            var abc = new { Name = "abc" };

            Assert.AreEqual("Name", CodeUtil.GetName(() => abc.Name));

            Assert.AreEqual("abc", CodeUtil.GetName(() => abc));
        }

        [TestMethod]
        public void TestGetTypeName()
        {
            Assert.AreEqual("string", typeof(String).GetName());
            Assert.AreEqual("string", typeof(string).GetName());
            Assert.AreEqual("string", typeof(string).GetName(CodeTypeStyle.FullName));

            Assert.AreEqual("DateTime", typeof(DateTime).GetName());
            Assert.AreEqual("System.DateTime", typeof(DateTime).GetName(CodeTypeStyle.FullName));

            Assert.AreEqual("List<string>", typeof(List<string>).GetName());
            Assert.AreEqual("System.Collections.Generic.List<string>", typeof(List<string>).GetName(CodeTypeStyle.FullName));

            Assert.AreEqual("Dictionary<string, int>", typeof(Dictionary<string, int>).GetName());
            Assert.AreEqual("System.Collections.Generic.Dictionary<string, int>", typeof(Dictionary<string, int>).GetName(CodeTypeStyle.FullName));
        }

        [TestMethod]
        public void TestGetValue()
        {
            Assert.AreEqual("A", typeof(TestGetValueA).GetValue(() => TestGetValueA.Property));
            Assert.AreEqual("B", typeof(TestGetValueB).GetValue(() => TestGetValueA.Property));
            Assert.AreEqual("A", typeof(TestGetValueC).GetValue(() => TestGetValueA.Property));

            try
            {
                typeof(string).GetValue(() => TestGetValueA.Property);
                Assert.Fail();
            }
            catch (ArgumentException) { }
        }
        public class TestGetValueA
        {
            public static string Property
            {
                get
                {
                    Contract.Ensures(Contract.Result<string>() != null);
                    return "A";
                }
            }
        }
        public class TestGetValueB : TestGetValueA
        {
            public new static string Property
            {
                get
                {
                    Contract.Ensures(Contract.Result<string>() != null);
                    return "B";
                }
            }
        }
        public class TestGetValueC : TestGetValueA { }
    }
}
