using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Collections
{
    public static class ListUtil
    {
        public static void SetItems<T>(this IList<T> list, IEnumerable<T> items)
        {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);

            var index = 0;
            foreach (var item in items)
            {
                if (index < list.Count)
                {
                    Contract.Assume(list.Count <= int.MaxValue);

                    var matchIndex = Enumerable.Range(index, list.Count - index)
                        .Where(t => Equals(list[t], item))
                        .Select(t => new int?(t)).FirstOrDefault();

                    if (matchIndex != null)
                    {
                        for (int j = index; j < matchIndex.Value; j++)
                        {
                            Contract.Assume(index < list.Count);

                            list.RemoveAt(index);
                        }
                    }
                    else
                    {
                        list.Insert(index, item);
                    }
                }
                else
                {
                    list.Add(item);
                }

                index++;
            }

            while (list.Count > index)
            {
                list.RemoveAt(index);
            }
        }
        public static void SetItems(this IList list, IEnumerable items)
        {
            Contract.Requires<ArgumentNullException>(list != null);
            Contract.Requires<ArgumentNullException>(items != null);

            var index = 0;
            foreach (var item in items)
            {
                if (index < list.Count)
                {
                    Contract.Assume(list.Count <= int.MaxValue);

                    var matchIndex = Enumerable.Range(index, list.Count - index)
                        .Where(t => Equals(list[t], item))
                        .Select(t => new int?(t)).FirstOrDefault();

                    if (matchIndex != null)
                    {
                        for (int j = index; j < matchIndex.Value; j++)
                        {
                            Contract.Assume(index < list.Count);

                            list.RemoveAt(index);
                        }
                    }
                    else
                    {
                        list.Insert(index, item);
                    }
                }
                else
                {
                    list.Add(item);
                }

                index++;
            }

            while (list.Count > index)
            {
                list.RemoveAt(index);
            }
        }
    }

    [TestClass]
    public class ListUtilTest
    {
        [TestMethod]
        public void TestSetItems()
        {
            var a = Tuple.Create(1, "a");
            var b = Tuple.Create(2, "b");
            var c = Tuple.Create(3, "c");
            var d = Tuple.Create(4, "d");
            var e = Tuple.Create(5, "e");

            var typedList = new List<Tuple<int, string>> { a, b, d };
            
            typedList.SetItems(new[] { b, d, e });
            Assert.IsTrue(new[] { b, d, e }.SequenceEqual(typedList));

            var newB = Tuple.Create(2, "b");
            typedList.SetItems(new[] { a, newB, c });
            Assert.IsTrue(new[] { a, b, c }.SequenceEqual(typedList));

            var newC = Tuple.Create(3, "c");
            typedList.SetItems(new[] { newB, newC, d });
            Assert.IsTrue(new[] { b, c, d }.SequenceEqual(typedList));

            typedList.SetItems(new[] { a });
            Assert.IsTrue(new[] { a }.SequenceEqual(typedList));
        }
    }
}
