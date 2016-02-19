using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Mathematics
{
    public static class Range
    {
        public static bool Contains<T>(T value, T bound1, T bound2) where T : IComparable<T>
        {
            var range = new Range<T>(bound1, bound2);
            return range.Contains(value);
        }

        public static Range<T> Get<T>(T bound1, T bound2) where T : IComparable<T>
        {
            Contract.Ensures(Contract.Result<Range<T>>() != null);

            return new Range<T>(bound1, bound2);
        }
    }

    public class Range<T> where T : IComparable<T>
    {
        public Range(T bound1, T bound2)
        {
            if (bound1.CompareTo(bound2) <= 0)
            {
                From = bound1;
                To = bound2;
            }
            else
            {
                To = bound1;
                From = bound2;
            }
        }

        public T From { get; private set; }
        public T To { get; private set; }

        public bool Contains(T value)
        {
            return Comparer<T>.Default.Compare(From, value) <= 0
                && Comparer<T>.Default.Compare(To, value) >= 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Range<T> == false)
                return false;

            var range = (Range<T>)obj;

            return object.Equals(range.From, From)
                && object.Equals(range.To, To);
        }
        public override int GetHashCode()
        {
            return From.GetHashCode()
                ^ To.GetHashCode();
        }
        public override string ToString()
        {
            return "{From=" + From.ToString() + ",To=" + To.ToString() + "}";
        }
    }

    [TestClass]
    public class RangeTest
    {
        [TestMethod]
        public void Test()
        {
            Assert.IsTrue(Range.Contains(value: 7.1, bound1: 7.0, bound2: 7.2));

            var range1 = Range.Get(1, 3);
            Assert.AreEqual(1, range1.From);
            Assert.AreEqual(3, range1.To);
            
            Assert.IsFalse(range1.Contains(0));
            Assert.IsTrue(range1.Contains(1));
            Assert.IsTrue(range1.Contains(2));
            Assert.IsTrue(range1.Contains(3));
            Assert.IsFalse(range1.Contains(4));

            var range2 = Range.Get(3, 1);
            Assert.AreEqual(1, range2.From);
            Assert.AreEqual(3, range2.To);
            Assert.AreEqual(range1, range2);
            Assert.AreEqual(range1.GetHashCode(), range2.GetHashCode());

            var range3 = new Range<int>(1, 3);
            Assert.AreEqual(range1, range3);

            Assert.AreEqual("{From=1,To=3}", range1.ToString());
        }
    }
}
