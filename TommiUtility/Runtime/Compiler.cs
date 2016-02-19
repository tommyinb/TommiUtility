using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Runtime
{
    public class Compiler
    {
        public string[] ReferencedAssemblies { get; set; }

        public Assembly Compile(string code)
        {
            Contract.Requires<ArgumentNullException>(code != null);
            Contract.Ensures(Contract.Result<Assembly>() != null);

            var options = new CompilerParameters();
            options.GenerateExecutable = false;
            options.GenerateInMemory = false;

            if (ReferencedAssemblies != null)
            {
                Contract.Assume(options.ReferencedAssemblies != null);
                options.ReferencedAssemblies.AddRange(ReferencedAssemblies);
            }

            var provider = new CSharpCodeProvider();
            var result = provider.CompileAssemblyFromSource(options, code);
            Contract.Assume(result != null);

            Contract.Assume(result.Errors != null);
            if (result.Errors.Count > 0) throw new CompileException { Errors = result.Errors };

            Contract.Assume(result.CompiledAssembly != null);
            return result.CompiledAssembly;
        }
    }

    [Serializable]
    public class CompileException : Exception
    {
        public CompilerErrorCollection Errors { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Errors", Errors);
        }
    }

    [TestClass]
    public class CompileTest
    {
        [TestMethod]
        public void TestCompile()
        {
            var compiler = new Compiler
            {
                ReferencedAssemblies = new[] { "System.dll" }
            };

            var code = "namespace Test"
                + "{"
                + "    public class Abc"
                + "    {"
                + "        public string Get()"
                + "        {"
                + "            return \"abc\";"
                + "        }"
                + "    }"
                + "}";

            var assembly = compiler.Compile(code);
            var type = assembly.GetType("Test.Abc");
            Assert.IsNotNull(type);
            Contract.Assume(type != null);

            var abc = Activator.CreateInstance(type);

            var method = type.GetMethod("Get");
            Assert.IsNotNull(method);
            Contract.Assume(method != null);

            var result = method.Invoke(abc, null);
            Assert.AreEqual("abc", result);
        }
    }
}
