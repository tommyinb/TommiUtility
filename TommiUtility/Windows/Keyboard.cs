using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TommiUtility.Windows
{
    public class Keyboard
    {
        public void KeyDown(Keys keys)
        {
            KeyboardEvent((byte)keys, 0);
            if (keys == Keys.ShiftKey)
                keyingShift = true;
        }
        public void KeyUp(Keys keys)
        {
            if (keys == Keys.ShiftKey)
                keyingShift = false;
            KeyboardEvent((byte)keys, 0x02);
        }
        public void KeyPress(Keys keys, int sleep = 1)
        {
            KeyDown(keys);

            Thread.Sleep(sleep);

            KeyUp(keys);
        }

        private bool keyingShift = false;

        private void KeyboardEvent(byte bVk, int dwFlags)
        {
            if (keyingShift)
            {
                NativeMethods.keybd_event(bVk, 0, dwFlags | 1, UIntPtr.Zero);
            }
            else
            {
                NativeMethods.keybd_event(bVk, 0, dwFlags, UIntPtr.Zero);
            }
        }
    }

    internal static partial class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern void keybd_event(byte bVk, byte bScan, int dwFlags, UIntPtr dwExtraInfo);
    }
}
