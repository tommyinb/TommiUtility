using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.ProgramFlow
{
    public static class LazyUtil
    {
        public static void EnsureInitialized<T>(this Lazy<T> lazy)
        {
            Contract.Requires<ArgumentNullException>(lazy != null);

            var initialize = new Func<T>(() => lazy.Value);
            initialize.Invoke();
        }
    }

    [TestClass]
    public class LazyUtilTest
    {
        [TestMethod]
        public void TestEnsureInitialized()
        {
            var lazy = new Lazy<int>(() => 7);
            Assert.IsFalse(lazy.IsValueCreated);

            lazy.EnsureInitialized();

            Assert.IsTrue(lazy.IsValueCreated);
            Assert.AreEqual(7, lazy.Value);
        }
    }
}
