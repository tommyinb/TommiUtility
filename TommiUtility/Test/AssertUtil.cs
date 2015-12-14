using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Test
{
    public class AssertUtil
    {
        public static void Throw<TException>(Action action) where TException : Exception
        {
            Contract.Requires<ArgumentNullException>(action != null);

            try
            {
                action();

                Assert.Fail();
            }
            catch (TException) { }
        }
    }

    [TestClass]
    public class AssertUtilTest
    {
        [TestMethod]
        public void TestThrow()
        {
            AssertUtil.Throw<Exception>(() => { throw new Exception(); });

            try
            {
                AssertUtil.Throw<ArgumentException>(() => { throw new Exception(); });
                Assert.Fail();
            }
            catch (Exception) { }

            try
            {
                AssertUtil.Throw<Exception>(() => { });
                Assert.Fail();
            }
            catch (AssertFailedException) { }

            try
            {
                AssertUtil.Throw<Exception>(action: null);
                Assert.Fail();
            }
            catch (ArgumentNullException) { }
        }
    }
}
