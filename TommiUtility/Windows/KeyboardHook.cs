using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Windows
{
    public class KeyboardHook : IDisposable
    {
        private static event KeyPressEventHandler keyPress;
        public static event KeyPressEventHandler KeyPress
        {
            add
            {
                lock (locker)
                {
                    keyPress += value;

                    if (HasHookEvents())
                    {
                        TryStartHook();
                    }
                }
            }
            remove
            {
                lock (locker)
                {
                    keyPress -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static event KeyEventHandler keyUp;
        public static event KeyEventHandler KeyUp
        {
            add
            {
                lock (locker)
                {
                    keyUp += value;

                    if (HasHookEvents())
                    {
                        TryStartHook();
                    }
                }
            }
            remove
            {
                lock (locker)
                {
                    keyUp -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static event KeyEventHandler keyDown;
        public static event KeyEventHandler KeyDown
        {
            add
            {
                lock (locker)
                {
                    keyDown += value;

                    if (HasHookEvents())
                    {
                        TryStartHook();
                    }
                }
            }
            remove
            {
                lock (locker)
                {
                    keyDown -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static bool HasHookEvents()
        {
            return keyPress != null || keyUp != null || keyDown != null;
        }
        private static volatile KeyboardHook keyboardHook = null;
        private static void TryStartHook()
        {
            lock (locker)
            {
                if (keyboardHook != null) return;

                keyboardHook = new KeyboardHook();
            }
        }
        private static void TryStopHook()
        {
            lock (locker)
            {
                if (keyboardHook == null) return;

                keyboardHook.Dispose();
                keyboardHook = null;
            }
        }

        private KeyboardHook()
        {
            var thread = new Thread(RunApplication);

            thread.IsBackground = true;

            thread.Start();
        }
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            applicationContext.ExitThread();

            if (disposing)
            {
                applicationContext.Dispose();
            }
        }
        ~KeyboardHook()
        {
            Dispose(false);
        }
        
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(applicationContext != null);
        }

        private readonly ApplicationContext applicationContext = new ApplicationContext();
        private void RunApplication()
        {
            var moduleHandler = NativeMethods.KeyboardHookGetModuleHandle(typeof(KeyboardHook).Module.FullyQualifiedName);

            hookHandle = NativeMethods.KeyboardHookSetWindowsHookEx(13, HookProc, moduleHandler, 0);

            if (hookHandle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();

                throw new Win32Exception(errorCode);
            }

            Application.Run(applicationContext);

            var unhookResult = NativeMethods.KeyboardHookUnhookWindowsHookEx(hookHandle);

            if (unhookResult == 0)
            {
                var errorCode = Marshal.GetLastWin32Error();

                throw new Win32Exception(errorCode);
            }
        }

        private IntPtr hookHandle;
        [ContractVerification(false)]
        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //indicates if any of underlaing events set e.Handled flag
            bool handled = false;

            if (nCode >= 0)
            {
                //read structure KeyboardHookStruct at lParam
                KeyboardHookStruct keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                //raise KeyDown
                var message = (KeyboardHookMessage)wParam;

                if (message == KeyboardHookMessage.KeyDown || message == KeyboardHookMessage.SystemKeyDown)
                {
                    Keys keyData = (Keys)keyboardHookStruct.VirtualKeyCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    TryInvoke(keyDown, typeof(KeyboardHook), e);
                    handled = e.Handled;
                }

                // raise KeyPress
                if (message == KeyboardHookMessage.KeyDown)
                {
                    bool isDownShift = ((NativeMethods.GetKeyState((byte)KeyboardHookVirtualKey.Shift) & 0x80) == 0x80 ? true : false);
                    bool isDownCapslock = (NativeMethods.GetKeyState((byte)KeyboardHookVirtualKey.Capital) != 0 ? true : false);

                    byte[] keyState = new byte[256];
                    NativeMethods.GetKeyboardState(keyState);
                    byte[] inBuffer = new byte[2];
                    if (NativeMethods.ToAscii(
                        keyboardHookStruct.VirtualKeyCode,
                        keyboardHookStruct.ScanCode,
                        keyState,
                        inBuffer,
                        keyboardHookStruct.Flags) == 1)
                    {
                        char key = (char)inBuffer[0];
                        if ((isDownCapslock ^ isDownShift) && Char.IsLetter(key)) key = Char.ToUpper(key);
                        KeyPressEventArgs e = new KeyPressEventArgs(key);
                        TryInvoke(keyPress, typeof(KeyboardHook), e);
                        handled |= e.Handled;
                    }
                }

                // raise KeyUp
                if (message == KeyboardHookMessage.KeyUp || message == KeyboardHookMessage.SystemKeyUp)
                {
                    Keys keyData = (Keys)keyboardHookStruct.VirtualKeyCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    TryInvoke(keyUp, typeof(KeyboardHook), e);
                    handled |= e.Handled;
                }

            }

            //if event handled in application do not handoff to other listeners
            if (handled)
                return new IntPtr(-1);

            //forward to other application
            return NativeMethods.KeyboardHookCallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        private static object locker = new object();
        private static void TryInvoke(Delegate @event, params object[] args)
        {
            Delegate[] delegates;

            lock (locker)
            {
                if (@event == null)
                {
                    return;
                }

                delegates = @event.GetInvocationList();
            }

            Contract.Assume(delegates != null);

            foreach (var @delegate in delegates)
            {
                if (@delegate != null)
                {
                    @delegate.DynamicInvoke(args);
                }
            }
        }
    }

    internal static partial class NativeMethods
    {
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandle", CharSet = CharSet.Unicode)]
        internal static extern IntPtr KeyboardHookGetModuleHandle(string name);

        [DllImport("user32.dll", EntryPoint = "CallNextHookEx", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr KeyboardHookCallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr KeyboardHookSetWindowsHookEx(int idHook, KeyboardHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", EntryPoint = "UnhookWindowsHookEx", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern int KeyboardHookUnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        internal static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern short GetKeyState(int vKey);
    }

    internal delegate IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyboardHookStruct
    {
        public int VirtualKeyCode;
        public int ScanCode;
        public int Flags;
        public int Time;
        public int ExtraInfo;
    }

    internal enum KeyboardHookMessage : int
    {
        KeyDown = 0x100,
        KeyUp = 0x101,
        SystemKeyDown = 0x104,
        SystemKeyUp = 0x105
    }

    internal enum KeyboardHookVirtualKey : uint
    {
        Shift = 0x10,
        Capital = 0x14
    }

    [TestClass]
    public class KeyboardHookTest
    {
        [TestMethod]
        public void Test()
        {
            var keyDowns = new List<Keys>();
            var keyPresses = new List<char>();
            var keyUps = new List<Keys>();

            KeyboardHook.KeyDown += (sender, e) => keyDowns.Add(e.KeyCode);
            KeyboardHook.KeyPress += (sender, e) => keyPresses.Add(e.KeyChar);
            KeyboardHook.KeyUp += (sender, e) => keyUps.Add(e.KeyCode);

            Thread.Sleep(3);

            var keyboard = new Keyboard();
            keyboard.KeyPress(Keys.A);
            keyboard.KeyPress(Keys.B);
            keyboard.KeyPress(Keys.C);

            Thread.Sleep(3);

            Assert.IsTrue(keyDowns.Contains(Keys.A));
            Assert.IsTrue(keyDowns.Contains(Keys.B));
            Assert.IsTrue(keyDowns.Contains(Keys.C));

            Assert.IsTrue(keyPresses.Contains('a'));
            Assert.IsTrue(keyPresses.Contains('b'));
            Assert.IsTrue(keyPresses.Contains('c'));

            Assert.IsTrue(keyUps.Contains(Keys.A));
            Assert.IsTrue(keyUps.Contains(Keys.B));
            Assert.IsTrue(keyUps.Contains(Keys.C));
        }
    }
}
