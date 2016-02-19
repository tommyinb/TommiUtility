using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Collections
{
    public static class ArrayUtil
    {
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            Contract.Requires<ArgumentNullException>(array != null);
            Contract.Requires<ArgumentNullException>(action != null);

            foreach (var item in array)
            {
                action(item);
            }
        }
    }
}
