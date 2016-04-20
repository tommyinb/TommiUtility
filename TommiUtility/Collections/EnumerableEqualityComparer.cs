using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Collections
{
    public class EnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        public EnumerableEqualityComparer()
        {
            itemComparer = EqualityComparer<T>.Default;
        }
        public EnumerableEqualityComparer(IEqualityComparer<T> itemComparer)
        {
            Contract.Requires<AggregateException>(itemComparer != null);

            this.itemComparer = itemComparer;
        }
        private readonly IEqualityComparer<T> itemComparer;

        public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            if (x != null)
            {
                if (y != null)
                {
                    return x.SequenceEqual(y, itemComparer);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (y != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public int GetHashCode(IEnumerable<T> obj)
        {
            if (obj == null) return 0;

            return obj.Aggregate(0, (enumerableHashCode, item) =>
            {
                var itemHashCode = itemComparer.GetHashCode(item);
                return enumerableHashCode ^ itemHashCode;
            });
        }
    }

    [TestClass]
    public class EnumerableEqualityComparerTest
    {
        [TestMethod]
        public void TestEquals()
        {
            var defaultComparer = new EnumerableEqualityComparer<int>();
            TestEquals(defaultComparer);

            var customComparer = new EnumerableEqualityComparer<int>(EqualityComparer<int>.Default);
            TestEquals(customComparer);
        }
        private void TestEquals(EnumerableEqualityComparer<int> comparer)
        {
            Contract.Requires<ArgumentNullException>(comparer != null);

            Assert.IsTrue(comparer.Equals(new[] { 1, 2 }, new[] { 1, 2 }));
            Assert.IsTrue(comparer.Equals(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }));

            Assert.IsFalse(comparer.Equals(new[] { 1, 2 }, new[] { 2, 2 }));
            Assert.IsFalse(comparer.Equals(new[] { 1, 2 }, new[] { 1, 1 }));
            Assert.IsFalse(comparer.Equals(new[] { 1, 2 }, new[] { 1, 2, 3 }));
            Assert.IsFalse(comparer.Equals(new[] { 1, 2, 3 }, new[] { 1, 2 }));
            Assert.IsFalse(comparer.Equals(new int[0], new[] { 1 }));
            Assert.IsFalse(comparer.Equals(new[] { 1 }, new int[0]));

            Assert.IsTrue(comparer.Equals(null, null));
            Assert.IsTrue(comparer.Equals(new int[0], new int[0]));
            Assert.IsFalse(comparer.Equals(new int[0], null));
            Assert.IsFalse(comparer.Equals(null, new int[0]));
        }

        [TestMethod]
        public void TestGetHashCode()
        {
            var defaultComparer = new EnumerableEqualityComparer<int>();
            TestGetHashCode(defaultComparer);

            var customComparer = new EnumerableEqualityComparer<int>(EqualityComparer<int>.Default);
            TestGetHashCode(customComparer);
        }
        private void TestGetHashCode(EnumerableEqualityComparer<int> comparer)
        {
            Contract.Requires<ArgumentNullException>(comparer != null);

            Assert.AreEqual(1, comparer.GetHashCode(new[] { 1 }));
            Assert.AreEqual(3, comparer.GetHashCode(new[] { 1, 2 }));
            Assert.AreEqual(0, comparer.GetHashCode(new[] { 1, 2, 3 }));
            Assert.AreEqual(4, comparer.GetHashCode(new[] { 1, 2, 3, 4 }));
            Assert.AreEqual(1, comparer.GetHashCode(new[] { 1, 2, 3, 4, 5 }));

            Assert.AreEqual(0, comparer.GetHashCode(new int[0]));
            Assert.AreEqual(0, comparer.GetHashCode(null));
        }
    }
}
