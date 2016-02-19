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
    public class MouseHook : IDisposable
    {
        private static event MouseEventHandler mouseMove;
        public static event MouseEventHandler MouseMove
        {
            add
            {
                lock (locker)
                {
                    mouseMove += value;

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
                    mouseMove -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static event EventHandler<MouseHookEventArgs> mouseMoveExt;
        public static event EventHandler<MouseHookEventArgs> MouseMoveExt
        {
            add
            {
                lock (locker)
                {
                    mouseMoveExt += value;

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
                    mouseMoveExt -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static event MouseEventHandler mouseClick;
        public static event MouseEventHandler MouseClick
        {
            add
            {
                lock (locker)
                {
                    mouseClick += value;

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
                    mouseClick -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static event EventHandler<MouseHookEventArgs> mouseClickExt;
        public static event EventHandler<MouseHookEventArgs> MouseClickExt
        {
            add
            {
                lock (locker)
                {
                    mouseClickExt += value;

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
                    mouseClickExt -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static event MouseEventHandler mouseDown;
        public static event MouseEventHandler  MouseDown
        {
            add 
            {
                lock (locker)
                {
                    mouseDown += value;

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
                    mouseDown -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static event MouseEventHandler mouseUp;
        public static event MouseEventHandler MouseUp
        {
            add
            {
                lock (locker)
                {
                    mouseUp += value;

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
                    mouseUp -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static event MouseEventHandler mouseWheel;
        public static event MouseEventHandler MouseWheel
        {
            add
            {
                lock (locker)
                {
                    mouseWheel += value;

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
                    mouseWheel -= value;

                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static event MouseEventHandler mouseDoubleClick;
        public static event MouseEventHandler MouseDoubleClick
        {
            add
            {
                lock (locker)
                {
                    mouseDoubleClick += value;

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
                    mouseDoubleClick -= value;


                    if (HasHookEvents() == false)
                    {
                        TryStopHook();
                    }
                }
            }
        }

        private static bool HasHookEvents()
        {
            return mouseMove != null || mouseMoveExt != null
                || mouseClick != null || mouseClickExt != null
                || mouseDown != null || mouseUp != null
                || mouseWheel != null || mouseDoubleClick != null;
        }
        private static volatile MouseHook mouseHook = null;
        private static void TryStartHook()
        {
            lock (locker)
            {
                if (mouseHook != null) return;

                mouseHook = new MouseHook();
            }
        }
        private static void TryStopHook()
        {
            lock (locker)
            {
                if (mouseHook == null) return;

                mouseHook.Dispose();
                mouseHook = null;
            }
        }

        private MouseHook()
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
        ~MouseHook()
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
            var moduleHandler = NativeMethods.MouseHookGetModuleHandle(typeof(MouseHook).Module.FullyQualifiedName);

            hookHandle = NativeMethods.MouseHookSetWindowsHookEx(14, HookProc, moduleHandler, 0);

            if (hookHandle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();

                throw new Win32Exception(errorCode);
            }

            Application.Run(applicationContext);

            var unhookResult = NativeMethods.MouseHookUnhookWindowsHookEx(hookHandle);

            if (unhookResult == 0)
            {
                var errorCode = Marshal.GetLastWin32Error();

                throw new Win32Exception(errorCode);
            }
        }

        private IntPtr hookHandle;
        private int prevX, prevY;
        [ContractVerification(false)]
        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MouseHookStruct mouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));

                MouseButtons button = MouseButtons.None;
                short mouseDelta = 0;
                int clickCount = 0;
                bool buttonDown = false;
                bool buttonUp = false;

                switch ((MouseHookMessage)wParam)
                {
                    case MouseHookMessage.LeftButtonDown:
                        buttonDown = true;
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case MouseHookMessage.LeftButtonUp:
                        buttonUp = true;
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case MouseHookMessage.LeftButtonDoubleClick:
                        button = MouseButtons.Left;
                        clickCount = 2;
                        break;
                    case MouseHookMessage.RightButtonDown:
                        buttonDown = true;
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case MouseHookMessage.RightButtonUp:
                        buttonUp = true;
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case MouseHookMessage.RightButtonDoubleClick:
                        button = MouseButtons.Right;
                        clickCount = 2;
                        break;
                    case MouseHookMessage.Wheel:
                        mouseDelta = (short)((mouseHookStruct.MouseData >> 16) & 0xffff);
                        break;
                }

                MouseHookEventArgs e = new MouseHookEventArgs(
                    button, clickCount, mouseHookStruct.Point.X, mouseHookStruct.Point.Y, mouseDelta);

                if (buttonUp)
                {
                    TryInvoke(mouseUp, typeof(MouseHook), e);
                }

                if (buttonDown)
                {
                    TryInvoke(mouseDown, typeof(MouseHook), e);
                }

                if (clickCount > 0)
                {
                    TryInvoke(mouseClick, typeof(MouseHook), e);

                    TryInvoke(mouseClickExt, typeof(MouseHook), e);

                    if (clickCount == 2)
                    {
                        TryInvoke(mouseDoubleClick, typeof(MouseHook), e);
                    }
                }

                if (mouseDelta != 0)
                {
                    TryInvoke(mouseWheel, typeof(MouseHook), e);
                }

                if (prevX != mouseHookStruct.Point.X || prevY != mouseHookStruct.Point.Y)
                {
                    prevX = mouseHookStruct.Point.X;
                    prevY = mouseHookStruct.Point.Y;

                    TryInvoke(mouseMove, typeof(MouseHook), e);
                    TryInvoke(mouseMoveExt, typeof(MouseHook), e);
                }

                if (e.Handled)
                {
                    return new IntPtr(-1);
                }
            }

            return NativeMethods.MouseHookCallNextHookEx(hookHandle, nCode, wParam, lParam);
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
        internal static extern IntPtr MouseHookGetModuleHandle(string name);

        [DllImport("user32.dll", EntryPoint = "CallNextHookEx", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr MouseHookCallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern IntPtr MouseHookSetWindowsHookEx(int idHook, MouseHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", EntryPoint = "UnhookWindowsHookEx", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern int MouseHookUnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32")]
        internal static extern int GetDoubleClickTime();
    }

    internal delegate IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseHookPoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseHookStruct
    {
        public MouseHookPoint Point;
        public int MouseData;
        public int Flags;
        public int Time;
        public int ExtraInfo;
    }

    internal enum MouseHookMessage : int
    {
        Move = 0x200,

        LeftButtonDown = 0x201,
        LeftButtonUp = 0x202,
        LeftButtonDoubleClick = 0x0203,

        RightButtonDown = 0x204,
        RightButtonUp = 0x205,
        RightButtonDoubleClick = 0x0206,

        MiddleButtonDown = 0x207,
        MiddleButtonUp = 0x208,
        MiddleButtonDoubleClick = 0x0209,

        Wheel = 0x20A
    }

    public class MouseHookEventArgs : MouseEventArgs
    {
        public MouseHookEventArgs(MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta) { }

        internal MouseHookEventArgs(MouseEventArgs e)
            : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
            Contract.Requires<ArgumentNullException>(e != null);
        }

        public bool Handled { get; set; }
    }

    [TestClass]
    public class MouseHookTest
    {
        [TestMethod]
        public void Test()
        {
            var mouseMoved = false;
            MouseHook.MouseMove += (sender, e) => mouseMoved = true;

            var mouseDowned = false;
            var mouseDownButton = MouseButtons.None;
            MouseHook.MouseDown += (sender, e) =>
            {
                mouseDowned = true;
                mouseDownButton = e.Button;
            };

            var mouseUped = false;
            var mouseUpButton = MouseButtons.None;
            MouseHook.MouseUp += (sender, e) =>
            {
                mouseUped = true;
                mouseUpButton = e.Button;
            };

            var mouseClicked = false;
            MouseHook.MouseClick += (sender, e) => mouseClicked = true;

            Thread.Sleep(3);

            Mouse.MoveBy(10, 10);
            Mouse.MoveBy(-10, -10);
            Mouse.MoveBy(10, 10);

            Mouse.MouseDown(MouseButton.Left);

            Mouse.MouseUp(MouseButton.Left);

            Thread.Sleep(3);

            Assert.IsTrue(mouseMoved);

            Assert.IsTrue(mouseDowned);
            Assert.AreEqual(MouseButtons.Left, mouseDownButton);

            Assert.IsTrue(mouseUped);
            Assert.AreEqual(MouseButtons.Left, mouseUpButton);

            Assert.IsTrue(mouseClicked);
        }
    }
}
