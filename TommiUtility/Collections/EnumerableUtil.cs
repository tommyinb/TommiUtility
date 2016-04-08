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
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            var result = sources.Aggregate((IEnumerable<T>)new T[0],
                (total, curr) => total.Concat(curr ?? new T[0]));
            
            Contract.Assume(result != null);
            return result;
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, T seperator)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

            var group = new List<T>();

            foreach (var item in source)
            {
                if (Equals(item, seperator))
                {
                    yield return group;

                    group = new List<T>();
                }
                else
                {
                    group.Add(item);
                }
            }

            yield return group;
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

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            return source.Reverse().Take(count).Reverse();
        }
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            return source.Reverse().Skip(count).Reverse();
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

        public static IEnumerable<T> Flood<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> floodSelector)
        {
            Contract.Requires<ArgumentNullException>(source != null);
            Contract.Requires<ArgumentNullException>(floodSelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            var items = source;

            while (items.Any())
            {
                foreach (var item in items)
                {
                    yield return item;
                }

                items = items.SelectMany(floodSelector);
            }
        }

        public static IEnumerable<T> For<T>(T start, Func<T, bool> condition, Func<T, T> increment)
        {
            Contract.Requires<ArgumentNullException>(condition != null);
            Contract.Requires<ArgumentNullException>(increment != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            for (var value = start; condition(value); value = increment(value))
            {
                yield return value;
            }
        }
        public static IEnumerable<T> Infinite<T>(T start, Func<T, T> increment)
        {
            Contract.Requires<ArgumentNullException>(increment != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            yield return start;

            var value = start;
            while (true)
            {
                value = increment(value);
                yield return value;
            }
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
        public void TestSplit()
        {
            var source = new[] { 1, 2, 0, 3, 4, 5, 0, 7 };

            var result1 = source.Split(0);
            var expected1 = new[] { new[] { 1, 2 }, new[] { 3, 4, 5 }, new[] { 7 } };
            AssertUtil.SequenceEqual(expected1, result1, (x, y) => x.SequenceEqual(y));

            var result2 = source.Split(1);
            var expected2 = new[] { new int[0], new[] { 2, 0, 3, 4, 5, 0, 7 } };
            AssertUtil.SequenceEqual(result2, expected2, (x, y) => x.SequenceEqual(y));

            var result3 = source.Split(7);
            var expected3 = new[] { new[] { 1, 2, 0, 3, 4, 5, 0 }, new int[0] }; 
            AssertUtil.SequenceEqual(result3, expected3, (x, y) => x.SequenceEqual(y));
        }

        [TestMethod]
        public void TestJoin()
        {
            var abc = new[] { 'a', 'b', 'c' };
            AssertUtil.SequenceEqual(abc.Join('k'), new[] { 'a', 'k', 'b', 'k', 'c' });

            var seperator = 'p';
            var seperatorFunc = new Func<char>(() => seperator++);
            AssertUtil.SequenceEqual(abc.Join(seperatorFunc), new[] { 'a', 'p', 'b', 'q', 'c' });
        }

        [TestMethod]
        public void TestTakeLast()
        {
            var abc = new[] { 1, 2, 3 };

            AssertUtil.SequenceEqual(new int[0], abc.TakeLast(-1));
            AssertUtil.SequenceEqual(new int[0], abc.TakeLast(0));
            AssertUtil.SequenceEqual(new[] { 3 }, abc.TakeLast(1));
            AssertUtil.SequenceEqual(new[] { 2, 3 }, abc.TakeLast(2));
            AssertUtil.SequenceEqual(new[] { 1, 2, 3 }, abc.TakeLast(3));
            AssertUtil.SequenceEqual(new[] { 1, 2, 3 }, abc.TakeLast(4));
        }

        [TestMethod]
        public void TestSkipLast()
        {
            var abc = new[] { 1, 2, 3 };

            AssertUtil.SequenceEqual(new[] { 1, 2, 3 }, abc.SkipLast(-1));
            AssertUtil.SequenceEqual(new[] { 1, 2, 3 }, abc.SkipLast(0));
            AssertUtil.SequenceEqual(new[] { 1, 2 }, abc.SkipLast(1));
            AssertUtil.SequenceEqual(new[] { 1 }, abc.SkipLast(2));
            AssertUtil.SequenceEqual(new int[0], abc.SkipLast(3));
            AssertUtil.SequenceEqual(new int[0], abc.SkipLast(4));
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

        [TestMethod]
        public void TestFlood()
        {
            var source = new[]
            {
                "1234567",
                "ABCD"
            };

            var divideHalf = new Func<string, string[]>(t => t.Length > 1 ?
                new[] { t.Substring(0, t.Length / 2), t.Substring(t.Length / 2) } : new string[0]);
            var flood = source.Flood(divideHalf);

            AssertUtil.SequenceEqual(new[]
            {
                "1234567", "ABCD",
                "123", "4567", "AB", "CD",
                "1", "23", "45", "67", "A", "B", "C", "D",
                "2", "3", "4", "5", "6", "7"
            }, flood);
        }

        [TestMethod]
        public void TestFor()
        {
            var numbers = EnumerableUtil.For(0, i => i < 5, i => i + 1).ToArray();
            AssertUtil.SequenceEqual(new[] { 0, 1, 2, 3, 4 }, numbers);

            var texts = EnumerableUtil.For("A",
                t => t.Length <= 3,
                t => t + (char)(t.Last() + 1)).ToArray();
            AssertUtil.SequenceEqual(new[] { "A", "AB", "ABC" }, texts);
        }

        [TestMethod]
        public void TestInfinite()
        {
            var infinite = EnumerableUtil.Infinite(0, i => i + 2);
            var resultValues = infinite.Take(100).ToArray();

            var expectedValues = Enumerable.Range(0, 100).Select(i => i * 2);
            AssertUtil.SequenceEqual(expectedValues, resultValues);

            Contract.Assume(infinite.Count() > 0);
            var firstHundred = infinite.First(t => t >= 100);
            Assert.AreEqual(100, firstHundred);
        }
    }
}
