using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Winform
{
    public class EventArgs<T> : EventArgs
    {
        public T Item { get; set; }
    }

    public static class EventArgsUtil
    {
        public static EventArgs<T> Create<T>(T item)
        {
            Contract.Ensures(Contract.Result<EventArgs<T>>() != null);

            return new EventArgs<T> { Item = item };
        }
    }
}
