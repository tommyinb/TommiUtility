using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TommiUtility.Test;

namespace TommiUtility.Collections
{
    public static class EnumerableUtil
    {
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);
            Contract.Ensures(Contract.Result<int>() >= -1);

            var index = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            return source.Any(predicate) == false;
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> source, T @default)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            var result = source.Take(1).ToArray();

            if (result.Length < 1) return @default;

            return result[0];
        }
        public static T FirstOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T @default)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            var result = source.Where(predicate).Take(1).ToArray();

            if (result.Length < 1) return @default;

            return result[0];
        }
        public static T LastOrDefault<T>(this IEnumerable<T> source, T @default)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            var result = source.Reverse().Take(1).ToArray();

            if (result.Length < 1) return @default;

            return result[0];
        }
        public static T LastOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T @default)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(predicate != null);

            var result = source.Reverse().Where(predicate).Take(1).ToArray();

            if (result.Length < 1) return @default;

            return result[0];
        }

        public static IEnumerable<T> Difference<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            Contract.Requires<ArgumentNullException>(first != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            return first.Except(second).Concat(second.Except(first));
        }

        public static IEnumerable<T1> Except<T1, T2>(this IEnumerable<T1> first,
            IEnumerable<T2> second, Func<T1, T2, bool> comparison)
        {
            Contract.Requires<ArgumentNullException>(first != null);
            Contract.Requires<ArgumentNullException>(second != null);
            Contract.Requires<ArgumentNullException>(comparison != null);
            Contract.Ensures(Contract.Result<IEnumerable<T1>>() != null);

            return first.Where(t => second
                .Any(s => comparison.Invoke(t, s)) == false);
        }

        public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] sources)
        {
            Contract.Requires<ArgumentNullException>(sources != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(0, sources.Length, i => sources[i] != null));
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            var result = sources.Aggregate((IEnumerable<T>)new T[0],
                (total, curr) => total.Concat(curr));
            
            Contract.Assume(result != null);
            return result;
        }

        public static IEnumerable<T> Join<T>(this IEnumerable<T> source, T seperator)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            return Join(source, () => seperator);
        }
        public static IEnumerable<T> Join<T>(this IEnumerable<T> source, Func<T> seperator)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(seperator != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            var isFollowing = false;

            foreach (var item in source)
            {
                if (isFollowing)
                {
                    yield return seperator();
                }
                else
                {
                    isFollowing = true;
                }

                yield return item;
            }
        }

        public static TimeSpan Sum<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(selector != null);

            return source.Select(selector).Sum();
        }
        public static TimeSpan Sum(this IEnumerable<TimeSpan> source)
        {
            Contract.Requires<ArgumentNullException>(source != null);

            return source.Aggregate((x, y) => x + y);
        }
    }

    [TestClass]
    public class EnumerableUtilTest
    {
        [TestMethod]
        public void TestIndexOf()
        {
            var values = new[] { 0, 1, 2, 3 };

            for (int i = 0; i < values.Length; i++)
            {
                var index = values.IndexOf(t => t == i);

                Assert.AreEqual(i, index);
            }
        }

        [TestMethod]
        public void TestNone()
        {
            var values = new[] { 0, 1, 2, 3 };

            Assert.IsTrue(values.None(t => t > 3));
            Assert.IsFalse(values.None(t => t <= 3));
        }

        [TestMethod]
        public void TestFirstOrDefault()
        {
            Assert.AreEqual(1, new int[] { 1, 2 }
                .FirstOrDefault(@default: 3));

            Assert.AreEqual(1, new int[0]
                .FirstOrDefault(@default: 1));

            Assert.AreEqual(2, new int[] { 1, 2, 3 }
                .FirstOrDefault(t => t > 1, @default: 4));

            Assert.AreEqual(4, new int[] { 1, 2, 3 }
                .FirstOrDefault(t => t > 3, @default: 4));

            Assert.AreEqual(1, new int[0]
                .FirstOrDefault(t => t > 3, @default: 1));
        }

        [TestMethod]
        public void TestLastOrDefault()
        {
            Assert.AreEqual(2, new int[] { 1, 2 }
                .LastOrDefault(@default: 3));

            Assert.AreEqual(1, new int[0]
                .LastOrDefault(@default: 1));

            Assert.AreEqual(3, new int[] { 1, 2, 3 }
                .LastOrDefault(t => t > 1, @default: 4));

            Assert.AreEqual(4, new int[] { 1, 2, 3 }
                .LastOrDefault(t => t > 3, @default: 4));

            Assert.AreEqual(1, new int[0]
                .LastOrDefault(t => t > 3, @default: 1));
        }

        [TestMethod]
        public void TestDifference()
        {
            var abc = new[] { 1, 2, 3, 4, 5 };
            var bcd = new[] { 2, 4, 6 };

            var diff = EnumerableUtil.Difference(abc, bcd);
            Assert.IsTrue(new[] { 1, 3, 5, 6 }.SequenceEqual(diff));
        }

        [TestMethod]
        public void TestExcept()
        {
            var abc = new[] { 1, 6, 3, 4, 5 };

            var bcd = new[] { "123", "1234" };

            var result = abc.Except(bcd, (t, s) => t == s.Length).ToArray();

            Assert.IsTrue(new[] { 1, 6, 5 }.SequenceEqual(result));
        }

        [TestMethod]
        public void TestConcat()
        {
            var abc = new[] { "a", "b", "c" };
            var def = new[] { "d", "e", "f" };
            var ghi = new[] { "g", "h", "i" };

            var result = EnumerableUtil.Concat(abc, def, ghi);
            var text = string.Join(string.Empty, result);

            Assert.AreEqual("abcdefghi", text);
        }

        [TestMethod]
        public void TestJoin()
        {
            var abc = new[] { 'a', 'b', 'c' };

            Assert.IsTrue(new[] { 'a', 'k', 'b', 'k', 'c' }.SequenceEqual(abc.Join('k')));

            var seperator = 'p';
            var seperatorFunc = new Func<char>(() => seperator++);
            Assert.IsTrue(new[] { 'a', 'p', 'b', 'q', 'c' }.SequenceEqual(abc.Join(seperatorFunc)));
        }

        [TestMethod]
        public void TestSum()
        {
            var timeSpans = new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMinutes(50),
                TimeSpan.FromMinutes(20),
                TimeSpan.FromHours(1)
            };

            var sum1 = timeSpans.Sum();
            var expect = new TimeSpan(2, 10, 13);
            Assert.AreEqual(expect, sum1);

            var items = timeSpans.Select(t => new { time = t });
            var sum2 = items.Sum(t => t.time);
            Assert.AreEqual(expect, sum2);
        }
    }
}
