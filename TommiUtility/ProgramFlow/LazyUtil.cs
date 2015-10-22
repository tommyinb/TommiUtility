using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.ProgramFlow
{
    public static class LazyUtil
    {
        public static void CreateValue<T>(this Lazy<T> lazy)
        {
            new Func<T>(() => lazy.Value).Invoke();
        }
    }
}
