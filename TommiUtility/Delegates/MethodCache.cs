using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Delegates
{
    public class MethodCache
    {
        private readonly Dictionary<MethodCacheInput, object> cache = new Dictionary<MethodCacheInput, object>();

        public TResult Run<T, TResult>(Func<T, TResult> func, T param)
        {
            Contract.Requires<ArgumentNullException>(func != null);

            var input = new MethodCacheInput(func, param);
            return Run<TResult>(input);
        }
        public TResult Run<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2)
        {
            Contract.Requires<ArgumentNullException>(func != null);

            var input = new MethodCacheInput(func, param1, param2);
            return Run<TResult>(input);
        }
        public TResult Run<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3)
        {
            Contract.Requires<ArgumentNullException>(func != null);

            var input = new MethodCacheInput(func, param1, param2, param3);
            return Run<TResult>(input);
        }
        public TResult Run<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            Contract.Requires<ArgumentNullException>(func != null);

            var input = new MethodCacheInput(func, param1, param2, param3, param4);
            return Run<TResult>(input);
        }

        private T Run<T>(MethodCacheInput input)
        {
            Contract.Requires<ArgumentNullException>(input != null);
            Contract.Requires<ArgumentException>(input.Delegate != null);

            if (cache.ContainsKey(input))
            {
                var value = cache[input];
                if (value == null) return default(T);
                return (T)value;
            }
            else
            {
                var value = input.Delegate.DynamicInvoke(input.Parameters); ;
                cache[input] = value;

                if (value == null) return default(T);
                return (T)value;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(cache != null);
        }
    }

    public class MethodCacheInput
    {
        public MethodCacheInput(Delegate @delegate, params object[] parameters)
        {
            Contract.Requires<ArgumentNullException>(@delegate != null);
            Contract.Requires<ArgumentNullException>(parameters != null);

            this.Delegate = @delegate;
            this.Parameters = parameters;

            var parameterObjects = parameters.Where(t => t != null);
            hashCode = parameterObjects.Aggregate(@delegate.GetHashCode(), (total, curr) => total ^ curr.GetHashCode());
        }

        public readonly Delegate Delegate;
        public readonly object[] Parameters;
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(Delegate != null);
            Contract.Invariant(Parameters != null);
        }

        private readonly int hashCode;
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

                    return Equals(Delegate, input.Delegate)
                        && input.Parameters != null
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
