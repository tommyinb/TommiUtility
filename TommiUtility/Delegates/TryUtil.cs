using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TommiUtility.Test;

namespace TommiUtility.Delegates
{
    public static class TryUtil
    {
        public static bool Invoke(Action action)
        {
            Contract.Requires<ArgumentNullException>(action != null);

            try
            {
                action.Invoke();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void Invoke(this Action action, int numberOfTry)
        {
            Contract.Requires<ArgumentNullException>(action != null);
            Contract.Requires<ArgumentException>(numberOfTry >= 0);

            var exceptions = new List<Exception>();

            for (int i = 0; i < numberOfTry; i++)
            {
                try
                {
                    action.Invoke();
                    return;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }
        public static T Invoke<T>(this Func<T> func, int numberOfTry)
        {
            Contract.Requires<ArgumentNullException>(func != null);
            Contract.Requires<ArgumentException>(numberOfTry >= 0);

            var exceptions = new List<Exception>();

            for (int i = 0; i < numberOfTry; i++)
            {
                try
                {
                    return func.Invoke();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }

        public static T Invoke<T>(this Func<T> func, int numberOfTry, T @default)
        {
            Contract.Requires<ArgumentNullException>(func != null);
            Contract.Requires<ArgumentException>(numberOfTry >= 0);

            for (int i = 0; i < numberOfTry; i++)
            {
                try
                {
                    return func.Invoke();
                }
                catch (Exception) { }
            }

            return @default;
        }
    }

    [TestClass]
    public class TryUtilTest
    {
        [TestMethod]
        public void TestTryActionInvoke()
        {
            var tryMessages = new List<string>();
            TryUtil.Invoke(() => tryMessages.Add("Good"), numberOfTry: 3);
            AssertUtil.SequenceEqual(new[] { "Good" }, tryMessages);

            try
            {
                var action = new Action(() => { throw new ArgumentException(); });
                action.Invoke(numberOfTry: 3);
                Assert.Fail();
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(3, ex.InnerExceptions.Count);
            }
        }

        [TestMethod]
        public void TestTryFuncInvoke()
        {
            var successResult = TryUtil.Invoke(() => 7, 3);
            Assert.AreEqual(7, successResult);

            var failFunc = new Func<int>(() => { throw new Exception(); });
            try
            {
                failFunc.Invoke(numberOfTry: 3);
                Assert.Fail();
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(3, ex.InnerExceptions.Count);
            }

            var failResult = failFunc.Invoke(3, @default: 5);
            Assert.AreEqual(5, failResult);
        }
    }
}
