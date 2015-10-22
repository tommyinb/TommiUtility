using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TommiUtility.Windows
{
    public static class Mouse
    {
        public static Point GetMousePosition() { return Cursor.Position; }

        public static void MoveTo(Point point)
        {
            int dx = (int)Math.Ceiling((double)point.X
                * 65536 / (Screen.PrimaryScreen.Bounds.Width - 1));
            int dy = (int)Math.Ceiling((double)point.Y
                * 65536 / (Screen.PrimaryScreen.Bounds.Height - 1));

            NativeMethods.MouseEvent(0x0001 | 0x8000, dx, dy, 0, UIntPtr.Zero);
        }
        public static void MoveBy(int dx, int dy)
        {
            NativeMethods.MouseEvent(0x0001, dx, dy, 0, UIntPtr.Zero);
        }

        public static void MouseDown(MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButton.Left:
                    MouseEvent(0x02); break;
                case MouseButton.Middle:
                    MouseEvent(0x20); break;
                case MouseButton.Right:
                    MouseEvent(0x08); break;
                default:
                    throw new ArgumentException();
            }
        }
        public static void MouseUp(MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButton.Left:
                    MouseEvent(0x04); break;
                case MouseButton.Middle:
                    MouseEvent(0x40); break;
                case MouseButton.Right:
                    MouseEvent(0x10); break;
                default:
                    throw new ArgumentException();
            }
        }
        
        private static void MouseEvent(uint dwFlags)
        {
            NativeMethods.MouseEvent(dwFlags, 0, 0, 0, UIntPtr.Zero);
        }
    }

    internal static partial class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        internal static extern void MouseEvent(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);
    }

    public enum MouseButton
    {
        Left, Middle, Right
    }

    [TestClass]
    public class MouseTest
    {
        [TestMethod]
        public void TestMoveTo()
        {
            for (int i = 1; i <= 5; i++)
            {
                var x = i * Screen.PrimaryScreen.Bounds.Width / 10 - 1;

                for (int j = 1; j <= 5; j++)
                {
                    var y = j * Screen.PrimaryScreen.Bounds.Height / 10 - 1;

                    TestMoveTo(new Point(x, y));
                }
            }

            var customPoints = new[]
            {
                new Point(0, 0), new Point(1, 1),
                new Point(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2),
                new Point(Screen.PrimaryScreen.Bounds.Width - 1, Screen.PrimaryScreen.Bounds.Height - 1)
            };
            foreach (var point in customPoints)
            {
                TestMoveTo(point);
            }
        }
        private void TestMoveTo(Point point)
        {
            Mouse.MoveTo(point);

            Assert.AreEqual(point.X, Cursor.Position.X);
            Assert.AreEqual(point.Y, Cursor.Position.Y);
        }

        [TestMethod]
        public void TestMoveBy()
        {
            Mouse.MoveTo(new Point(200, 200));
            Mouse.MoveBy(100, 100);
            Assert.IsTrue(Cursor.Position.X > 100);
            Assert.IsTrue(Cursor.Position.Y > 100);

            Mouse.MoveTo(new Point(100, 100));
            Mouse.MoveBy(-100, -100);
            Assert.IsTrue(Cursor.Position.X < 10);
            Assert.IsTrue(Cursor.Position.Y < 10);
        }
    }
}
