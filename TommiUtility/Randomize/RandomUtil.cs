using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Randomize
{
    public static class RandomUtil
    {
        private static Lazy<Random> random = new Lazy<Random>();
        public static Random GetRandom()
        {
            return random.Value;
        }
        public static void SetRandom(Random random)
        {
            Contract.Requires<ArgumentNullException>(random != null);

            RandomUtil.random = new Lazy<Random>(() => random);
        }

        public static int Next(int exclusiveMaximum)
        {
            Contract.Requires<ArgumentException>(exclusiveMaximum >= 0);

            var random = RandomUtil.random.Value;
            Contract.Assume(random != null);

            return random.Next(exclusiveMaximum);
        }
        public static int Next(int minimum, int maximum)
        {
            Contract.Requires<ArgumentException>(minimum <= maximum);
            Contract.Ensures(Contract.Result<int>() >= minimum);

            if (minimum == maximum) return minimum;

            var range = maximum - minimum + 1;

            var random = RandomUtil.random.Value;
            Contract.Assume(random != null);

            var value = random.Next(range);

            return minimum + value;
        }

        public static bool Next(double probability)
        {
            Contract.Requires<ArgumentException>(probability >= 0);
            Contract.Requires<ArgumentException>(probability <= 1);

            var random = RandomUtil.random.Value;
            Contract.Assume(random != null);

            var value = random.NextDouble();
            return value < probability;
        }

        public static T Next<T>(T[] items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Requires<ArgumentException>(items.Length > 0);
            
            var random = RandomUtil.random.Value;
            Contract.Assume(random != null);

            var index = random.Next(items.Length);
            return items[index];
        }
        public static T[] Randomize<T>(this T[] items)
        {
            Contract.Requires<ArgumentNullException>(items != null);
            Contract.Ensures(Contract.Result<T[]>().Length == items.Length);

            var result = items.OrderBy(t => random.Value.Next(items.Length)).ToArray();

            Contract.Assume(result.Length == items.Length);
            return result;
        }
    }
}
