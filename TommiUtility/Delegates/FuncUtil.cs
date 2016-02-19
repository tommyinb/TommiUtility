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
    public static class FuncUtil
    {
        public static T Repeat<T>(this Func<T, T> func, T input, int times)
        {
            Contract.Requires<ArgumentNullException>(func != null);
            Contract.Requires<ArgumentException>(times >= 0);

            return Enumerable.Repeat(func, times)
                .Aggregate(input, (result, curr) => curr(result));
        }

        public static IEnumerable<TResult> InvokeAll<TResult>(this Func<TResult> func)
        {
            Contract.Requires<ArgumentNullException>(func != null);
            Contract.Ensures(Contract.Result<IEnumerable<TResult>>() != null);

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                var resultObject = invocation.DynamicInvoke();
                yield return resultObject != null ? (TResult)resultObject : default(TResult);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, TResult>(this Func<T1, TResult> func, T1 arg1)
        {
            Contract.Requires<ArgumentNullException>(func != null);
            Contract.Ensures(Contract.Result<IEnumerable<TResult>>() != null);

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                var resultObject = invocation.DynamicInvoke(arg1);
                yield return resultObject != null ? (TResult)resultObject : default(TResult);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, TResult>(this Func<T1, T2, TResult> func, T1 arg1, T2 arg2)
        {
            Contract.Requires<ArgumentNullException>(func != null);
            Contract.Ensures(Contract.Result<IEnumerable<TResult>>() != null);

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                var resultObject = invocation.DynamicInvoke(arg1, arg2);
                yield return resultObject != null ? (TResult)resultObject : default(TResult);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, T1 arg1, T2 arg2, T3 arg3)
        {
            Contract.Requires<ArgumentNullException>(func != null);
            Contract.Ensures(Contract.Result<IEnumerable<TResult>>() != null);

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                var resultObject = invocation.DynamicInvoke(arg1, arg2, arg3);
                yield return resultObject != null ? (TResult)resultObject : default(TResult);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Contract.Requires<ArgumentNullException>(func != null);
            Contract.Ensures(Contract.Result<IEnumerable<TResult>>() != null);

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                var resultObject = invocation.DynamicInvoke(arg1, arg2, arg3, arg4);
                yield return resultObject != null ? (TResult)resultObject : default(TResult);
            }
        }
    }

    [TestClass]
    public class FuncUtilTest
    {
        [TestMethod]
        public void TestRepeat()
        {
            var increment = new Func<int, int>(t => t + 1);

            var value = increment.Repeat(input: 0, times: 5);

            Assert.AreEqual(5, value);
        }

        [TestMethod]
        public void TestInvokeAll()
        {
            TestInvokeAll0();
            TestInvokeAll1();
            TestInvokeAll2();
            TestInvokeAll3();
            TestInvokeAll4();
        }
        private void TestInvokeAll0()
        {
            Func<int> func = null;
            func += () => 0;
            func += () => 1;
            func += () => 2;

            var result = func.InvokeAll().ToArray();
            AssertUtil.SequenceEqual(new[] { 0, 1, 2 }, result);
        }
        private void TestInvokeAll1()
        {
            Func<int, int> func = null;
            func += (t1) => 0;
            func += (t1) => 1;
            func += (t1) => 2;

            var result = func.InvokeAll(0).ToArray();
            AssertUtil.SequenceEqual(new[] { 0, 1, 2 }, result);
        }
        private void TestInvokeAll2()
        {
            Func<int, int, int> func = null;
            func += (t1, t2) => 0;
            func += (t1, t2) => 1;
            func += (t1, t2) => 2;

            var result = func.InvokeAll(0, 1).ToArray();
            AssertUtil.SequenceEqual(new[] { 0, 1, 2 }, result);
        }
        private void TestInvokeAll3()
        {
            Func<int, int, int, int> func = null;
            func += (t1, t2, t3) => 0;
            func += (t1, t2, t3) => 1;
            func += (t1, t2, t3) => 2;

            var result = func.InvokeAll(0, 1, 2).ToArray();
            AssertUtil.SequenceEqual(new[] { 0, 1, 2 }, result);
        }
        private void TestInvokeAll4()
        {
            Func<int, int, int, int, int> func = null;
            func += (t1, t2, t3, t4) => 0;
            func += (t1, t2, t3, t4) => 1;
            func += (t1, t2, t3, t4) => 2;

            var result = func.InvokeAll(0, 1, 2, 3).ToArray();
            AssertUtil.SequenceEqual(new[] { 0, 1, 2 }, result);
        }
    }
}
