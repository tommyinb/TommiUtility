using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Collections
{
    public static class CollectionUtil
    {
        public static void Remove<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            var items = collection.Where(predicate).ToArray();

            foreach (var item in items)
            {
                collection.Remove(item);
            }
        }
    }

    [TestClass]
    public class CollectionUtilTest
    {
        [TestMethod]
        public void TestRemove()
        {
            Collection<string> collection = new Collection<string>();

            collection.Add("abc");
            collection.Add("bcd");
            collection.Add("cde");
            collection.Add("def");

            collection.Remove(t => t.Contains("d"));

            Assert.IsTrue(new[] { "abc" }.SequenceEqual(collection));
        }
    }
}
