using System;
using System.Collections.Generic;
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
            RandomUtil.random = new Lazy<Random>(() => random);
        }

        public static int Next(int exclusiveMaximum)
        {
            return random.Value.Next(exclusiveMaximum);
        }
        public static int Next(int minimum, int maximum)
        {
            if (minimum > maximum)
            {
                throw new ArgumentException();
            }
            if (minimum == maximum)
            {
                return minimum;
            }

            var range = maximum - minimum + 1;
            
            var value = random.Value.Next(range);

            return minimum + value;
        }

        public static bool Next(double probability)
        {
            if (probability >= 1)
                return true;

            var value = random.Value.NextDouble();
            return value < probability;
        }

        public static T Next<T>(T[] items)
        {
            var index = random.Value.Next(
                items.Length);

            return items[index];
        }
        public static T[] Randomize<T>(this T[] items)
        {
            return items.OrderBy(t =>
                random.Value.Next(items.Length))
                .ToArray();
        }
    }
}
