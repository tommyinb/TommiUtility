using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Test
{
    public class AssertUtil
    {
        public static void Throw<TException>(Action action) where TException : Exception
        {
            Contract.Requires<ArgumentNullException>(action != null);

            try
            {
                action();

                Assert.Fail();
            }
            catch (TException) { }
        }

        public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> result)
        {
            Contract.Requires<ArgumentNullException>(expected != null);
            Contract.Requires<ArgumentNullException>(result != null);

            Assert.IsTrue(expected.SequenceEqual(result));
        }
        public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> result, Func<T, T, bool> comparison)
        {
            Contract.Requires<ArgumentNullException>(expected != null);
            Contract.Requires<ArgumentNullException>(result != null);
            Contract.Requires<ArgumentNullException>(comparison != null);

            var expectedEnumerator = expected.GetEnumerator();
            var resultEnumerator = result.GetEnumerator();

            while (expectedEnumerator.MoveNext())
            {
                Assert.IsTrue(resultEnumerator.MoveNext());

                var itemsEqual = comparison(expectedEnumerator.Current, resultEnumerator.Current);
                Assert.IsTrue(itemsEqual);
            }

            Assert.IsFalse(resultEnumerator.MoveNext());
        }
    }
}
