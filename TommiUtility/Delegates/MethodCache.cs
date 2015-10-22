using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Delegates
{
    public class MethodCache
    {
        private Dictionary<MethodCacheInput, object> cache = new Dictionary<MethodCacheInput, object>();

        public TResult Run<T, TResult>(Func<T, TResult> func, T param)
        {
            return Run<TResult>(new MethodCacheInput(func, param));
        }
        public TResult Run<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2)
        {
            return Run<TResult>(new MethodCacheInput(func, param1, param2));
        }
        public TResult Run<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3)
        {
            return Run<TResult>(new MethodCacheInput(func, param1, param2, param3));
        }
        public TResult Run<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            return Run<TResult>(new MethodCacheInput(func, param1, param2, param3, param4));
        }

        private T Run<T>(MethodCacheInput input)
        {
            if (cache.ContainsKey(input))
            {
                var value = cache[input];
                return (T)value;
            }
            else
            {
                var value = (T)input.Delegate.DynamicInvoke(input.Parameters);
                cache[input] = value;
                return value;
            }
        }
    }

    public class MethodCacheInput
    {
        public MethodCacheInput(Delegate Delegate, params object[] parameters)
        {
            this.Delegate = Delegate;
            this.Parameters = parameters;

            hashCode = Delegate.GetHashCode();
            foreach (var parameter in parameters)
            {
                if (parameter != null)
                {
                    hashCode ^= parameter.GetHashCode();
                }
            }
        }

        public Delegate Delegate { get; private set; }
        public object[] Parameters { get; private set; }

        private int hashCode;
        public override int GetHashCode()
        {
            return hashCode;
        }
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }
            else
            {
                if (obj is MethodCacheInput)
                {
                    var input = (MethodCacheInput)obj;

                    return Delegate.Equals(input.Delegate)
                        && Parameters.SequenceEqual(input.Parameters);
                }
                else
                {
                    return false;
                }
            }
        }
    }

    [TestClass]
    public class MethodCacheTest
    {
        [TestMethod]
        public void Test()
        {
            var testFunc1 = new Func<int, int>(t => t);
            var testFunc2 = new Func<int, int, int>((t1, t2) => t1 + t2);
            var testFunc3 = new Func<int, int, int, int>((t1, t2, t3) => t1 + t2 + t3);
            var testFunc4 = new Func<int, int, int, int, int>((t1, t2, t3, t4) => t1 + t2 + t3 + t4);

            var cache = new MethodCache();
            for (int i = 0; i < 3; i++)
			{
                Assert.AreEqual(1, cache.Run(testFunc1, 1));
                Assert.AreEqual(3, cache.Run(testFunc2, 1, 2));
                Assert.AreEqual(6, cache.Run(testFunc3, 1, 2, 3));
                Assert.AreEqual(10, cache.Run(testFunc4, 1, 2, 3, 4));
			}

            var count = 0;
            var testCache = new Func<int, int>(t => t * 1000 + ++count);

            Assert.AreEqual(1001, cache.Run(testCache, 1));
            Assert.AreEqual(1001, cache.Run(testCache, 1));
            Assert.AreEqual(1002, testCache(1));
            Assert.AreEqual(1001, cache.Run(testCache, 1));

            Assert.AreEqual(2003, testCache(2));
            Assert.AreEqual(2004, cache.Run(testCache, 2));
            Assert.AreEqual(2004, cache.Run(testCache, 2));

            Assert.AreEqual(1001, cache.Run(testCache, 1));

            Assert.AreEqual(4, count);
        }
    }
}
