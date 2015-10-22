using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iBoss2.Util
{
    public static class Box
    {
        public static Box<T> Create<T>(T item)
        {
            return new Box<T> { Item = item };
        }
        public static Box<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Box<T1, T2> { Item1 = item1, Item2 = item2 };
        }
        public static Box<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Box<T1, T2, T3> { Item1 = item1, Item2 = item2, Item3 = item3 };
        }
        public static Box<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new Box<T1, T2, T3, T4> { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4 };
        }
        public static Box<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            return new Box<T1, T2, T3, T4, T5> { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4, Item5 = item5 };
        }
    }

    public class Box<T>
    {
        public T Item { get; set; }
    }

    public class Box<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
    }

    public class Box<T1, T2, T3>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
    }

    public class Box<T1, T2, T3, T4>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
    }

    public class Box<T1, T2, T3, T4, T5>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
        public T5 Item5 { get; set; }
    }
}
