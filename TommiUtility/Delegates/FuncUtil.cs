using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Delegates
{
    public static class FuncUtil
    {
        public static T Repeat<T>(this Func<T, T> func, T input, int times)
        {
            var result = input;

            for (int i = 0; i < times; i++)
            {
                result = func.Invoke(result);
            }

            return result;
        }

        public static IEnumerable<TResult> InvokeAll<TResult>(this Func<TResult> func)
        {
            if (func == null)
                yield break;

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                yield return (TResult)invocation.DynamicInvoke();
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, TResult>(this Func<T1, TResult> func, T1 arg1)
        {
            if (func == null)
                yield break;

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                yield return (TResult)invocation.DynamicInvoke(arg1);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, TResult>(this Func<T1, T2, TResult> func, T1 arg1, T2 arg2)
        {
            if (func == null)
                yield break;

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                yield return (TResult)invocation.DynamicInvoke(arg1, arg2);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, T1 arg1, T2 arg2, T3 arg3)
        {
            if (func == null)
                yield break;

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                yield return (TResult)invocation.DynamicInvoke(arg1, arg2, arg3);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (func == null)
                yield break;

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                yield return (TResult)invocation.DynamicInvoke(arg1, arg2, arg3, arg4);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (func == null)
                yield break;

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                yield return (TResult)invocation.DynamicInvoke(arg1, arg2, arg3, arg4, arg5);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, T3, T4, T5, T6, TResult>(this Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if (func == null)
                yield break;

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                yield return (TResult)invocation.DynamicInvoke(arg1, arg2, arg3, arg4, arg5, arg6);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, T3, T4, T5, T6, T7, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            if (func == null)
                yield break;

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                yield return (TResult)invocation.DynamicInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
        }
        public static IEnumerable<TResult> InvokeAll<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            if (func == null)
                yield break;

            var invocations = func.GetInvocationList();
            foreach (var invocation in invocations)
            {
                yield return (TResult)invocation.DynamicInvoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
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
            TestInvokeAll5();
            TestInvokeAll6();
            TestInvokeAll7();
            TestInvokeAll8();
        }
        private void TestInvokeAll0()
        {
            Func<int> func = null;
            func += () => 0;
            func += () => 1;
            func += () => 2;

            var result = func.InvokeAll().ToArray();

            Assert.AreEqual(3, result.Length);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
        private void TestInvokeAll1()
        {
            Func<int, int> func = null;
            func += (t1) => 0;
            func += (t1) => 1;
            func += (t1) => 2;

            var result = func.InvokeAll(0).ToArray();

            Assert.AreEqual(3, result.Length);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
        private void TestInvokeAll2()
        {
            Func<int, int, int> func = null;
            func += (t1, t2) => 0;
            func += (t1, t2) => 1;
            func += (t1, t2) => 2;

            var result = func.InvokeAll(0, 1).ToArray();

            Assert.AreEqual(3, result.Length);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
        private void TestInvokeAll3()
        {
            Func<int, int, int, int> func = null;
            func += (t1, t2, t3) => 0;
            func += (t1, t2, t3) => 1;
            func += (t1, t2, t3) => 2;

            var result = func.InvokeAll(0, 1, 2).ToArray();

            Assert.AreEqual(3, result.Length);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
        private void TestInvokeAll4()
        {
            Func<int, int, int, int, int> func = null;
            func += (t1, t2, t3, t4) => 0;
            func += (t1, t2, t3, t4) => 1;
            func += (t1, t2, t3, t4) => 2;

            var result = func.InvokeAll(0, 1, 2, 3).ToArray();

            Assert.AreEqual(3, result.Length);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
        private void TestInvokeAll5()
        {
            Func<int, int, int, int, int, int> func = null;
            func += (t1, t2, t3, t4, t5) => 0;
            func += (t1, t2, t3, t4, t5) => 1;
            func += (t1, t2, t3, t4, t5) => 2;

            var result = func.InvokeAll(0, 1, 2, 3, 4).ToArray();

            Assert.AreEqual(3, result.Length);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
        private void TestInvokeAll6()
        {
            Func<int, int, int, int, int, int, int> func = null;
            func += (t1, t2, t3, t4, t5, t6) => 0;
            func += (t1, t2, t3, t4, t5, t6) => 1;
            func += (t1, t2, t3, t4, t5, t6) => 2;

            var result = func.InvokeAll(0, 1, 2, 3, 4, 5).ToArray();

            Assert.AreEqual(3, result.Length);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
        private void TestInvokeAll7()
        {
            Func<int, int, int, int, int, int, int, int> func = null;
            func += (t1, t2, t3, t4, t5, t6, t7) => 0;
            func += (t1, t2, t3, t4, t5, t6, t7) => 1;
            func += (t1, t2, t3, t4, t5, t6, t7) => 2;

            var result = func.InvokeAll(0, 1, 2, 3, 4, 5, 6).ToArray();

            Assert.AreEqual(3, result.Length);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
        private void TestInvokeAll8()
        {
            Func<int, int, int, int, int, int, int, int, int> func = null;
            func += (t1, t2, t3, t4, t5, t6, t7, t8) => 0;
            func += (t1, t2, t3, t4, t5, t6, t7, t8) => 1;
            func += (t1, t2, t3, t4, t5, t6, t7, t8) => 2;

            var result = func.InvokeAll(0, 1, 2, 3, 4, 5, 6, 7).ToArray();

            Assert.AreEqual(3, result.Length);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }
    }
}
