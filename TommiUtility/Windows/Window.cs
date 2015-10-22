using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Windows
{
    public static class Window
    {
        public static IntPtr GetForegroundWindow()
        {
            return NativeMethods.GetForegroundWindow();
        }

        public static bool SetForegroundWindow(IntPtr windowHandle)
        {
            return NativeMethods.SetForegroundWindow(windowHandle);
        }

        public static string GetText(IntPtr windowHandle)
        {
            int bufferSize = 256;

            while (true)
            {
                var buffer = new StringBuilder(bufferSize);

                var length = NativeMethods.GetWindowText(windowHandle, buffer, bufferSize);

                if (length >= bufferSize)
                {
                    if (bufferSize >= 1024 * 1024)
                        throw new OverflowException();

                    bufferSize *= 2;

                    continue;
                }

                return buffer.ToString();
            }
        }
        
        public static bool SetRectangle(IntPtr windowHandle, Rectangle rectangle)
        {
            return NativeMethods.SetWindowPos(
                windowHandle, IntPtr.Zero,
                rectangle.X, rectangle.Y,
                rectangle.Width, rectangle.Height, 0);
        }
    }

    internal static partial class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr windowHandle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, System.UInt32 uFlags);
    }
}
