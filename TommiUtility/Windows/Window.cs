using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentException>(windowHandle != IntPtr.Zero);

            return NativeMethods.SetForegroundWindow(windowHandle);
        }

        public static string GetText(IntPtr windowHandle)
        {
            Contract.Requires<ArgumentException>(windowHandle != IntPtr.Zero);
            Contract.Ensures(Contract.Result<string>() != null);

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
        
        public static Rectangle GetRectangle(IntPtr windowHandle)
        {
            var rect = new NativeMethods.Rect();

            var getWindowRect = NativeMethods.GetWindowRect(windowHandle, ref rect);
            if (getWindowRect == false) throw new InvalidOperationException();

            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left + 1, rect.Bottom - rect.Top + 1);
        }

        public static bool SetRectangle(IntPtr windowHandle, Rectangle rectangle)
        {
            Contract.Requires<ArgumentException>(windowHandle != IntPtr.Zero);

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
        public static extern bool GetWindowRect(IntPtr hWnd, ref Rect rect);
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, System.UInt32 uFlags);
    }
}
