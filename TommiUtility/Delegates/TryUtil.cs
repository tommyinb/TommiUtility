using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Delegates
{
    public static class TryUtil
    {
        public static bool Invoke(Action action)
        {
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
            List<string> tryMessages = new List<string>();

            TryUtil.Invoke(() => tryMessages.Add("Good"), 3);

            Assert.AreEqual(1, tryMessages.Count);
            Assert.AreEqual("Good", tryMessages.First());

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
