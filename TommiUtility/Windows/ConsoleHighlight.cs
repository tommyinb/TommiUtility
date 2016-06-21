using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Windows
{
    public class ConsoleHighlight : IDisposable
    {
        public ConsoleHighlight(ConsoleColor color)
        {
            originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }
        public void Dispose()
        {
            Console.ForegroundColor = originalColor;
        }

        private readonly ConsoleColor originalColor;

        public static void Write(ConsoleColor color, object value)
        {
            using (new ConsoleHighlight(color))
            {
                Console.Write(value);
            }
        }
        public static void WriteLine(ConsoleColor color, object value)
        {
            using (new ConsoleHighlight(color))
            {
                Console.WriteLine(value);
            }
        }
    }
}
