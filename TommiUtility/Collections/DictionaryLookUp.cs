using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Collections
{
    public class DictionaryLookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        public DictionaryLookup() { }
        public DictionaryLookup(ILookup<TKey, TElement> lookup)
        {
            foreach (var pair in lookup)
            {
                var elements = new List<TElement>(pair);
                dictionary.Add(pair.Key, elements);
            }
        }

        private Dictionary<TKey, List<TElement>> dictionary = new Dictionary<TKey, List<TElement>>();

        public void Add(TKey key, TElement element)
        {
            if (dictionary.ContainsKey(key))
            {
                var elements = dictionary[key];
                elements.Add(element);
            }
            else
            {
                var elements = new List<TElement>();
                dictionary.Add(key, elements);

                elements.Add(element);
            }
        }
        public void Add(TKey key, IEnumerable<TElement> elements)
        {
            if (dictionary.ContainsKey(key))
            {
                var list = dictionary[key];
                list.AddRange(elements);
            }
            else
            {
                var list = new List<TElement>(elements);
                dictionary.Add(key, list);
            }
        }

        public bool Remove(TKey key, TElement element)
        {
            if (dictionary.ContainsKey(key) == false) return false;

            var elements = dictionary[key];

            if (elements.Remove(element) == false) return false;

            if (elements.Any() == false)
            {
                dictionary.Remove(key);
            }

            return true;
        }
        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        public bool Contains(TKey key)
        {
            return dictionary.ContainsKey(key) && dictionary[key].Any();
        }
        public bool Contains(TKey key, TElement element)
        {
            return dictionary.ContainsKey(key) && dictionary[key].Contains(element);
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                return dictionary[key];
            }
        }
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            var groupings = dictionary.Select(t => new DictionaryLookupGrouping<TKey, TElement>(t.Key, t.Value));
            return groupings.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    internal sealed class DictionaryLookupGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        public DictionaryLookupGrouping(TKey key, List<TElement> elements)
        {
            Key = key;
            Elements = elements;
        }
        public TKey Key { get; private set; }
        public List<TElement> Elements { get; private set; }

        public IEnumerator<TElement> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    [TestClass]
    public class DictionaryLookUpTest
    {
        [TestMethod]
        public void Test()
        {
            var lookup = new DictionaryLookup<int, string>();

            lookup.Add(1, "A");
            lookup.Add(1, "B");
            lookup.Add(1, "C");
            lookup.Add(2, "D");
            lookup.Add(2, "D");
            lookup.Add(3, "E");

            var getResult = new Func<string>(() =>
                string.Join(", ", lookup.Select(t => t.Key + ":" + string.Join(string.Empty, t))));

            var result1 = getResult();
            Assert.AreEqual("1:ABC, 2:DD, 3:E", result1);

            lookup.Add(1, new[] { "F", "G" });

            lookup.Remove(1, "B");

            var result2 = getResult();
            Assert.AreEqual("1:ACFG, 2:DD, 3:E", result2);

            lookup.Remove(2);

            var result3 = getResult();
            Assert.AreEqual("1:ACFG, 3:E", result3);

            Assert.IsTrue(lookup[1].SequenceEqual(new[] { "A", "C", "F", "G" }));

            Assert.AreEqual(2, lookup.Count);
        }
    }
}
