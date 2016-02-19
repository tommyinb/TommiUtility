using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Delegates
{
    public static class ActionUtil
    {
        public static void Repeat(this Action action, int times)
        {
            Contract.Requires<ArgumentNullException>(action != null);
            Contract.Requires<ArgumentException>(times >= 0);

            for (int i = 0; i < times; i++)
            {
                action.Invoke();
            }
        }
    }

    [TestClass]
    public class ActionUtilTest
    {
        [TestMethod]
        public void TestRepeat()
        {
            var value = 0;
            var increment = new Action(() => value += 1);

            increment.Repeat(5);
            Assert.AreEqual(5, value);
        }
    }
}
