using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Windows
{
    public class AttachConsole : IDisposable
    {
        public AttachConsole()
        {
            var attachParent = NativeMethods.AttachConsole(0xFFFFFFFF);
            if (attachParent == false)
            {
                var attachDenied = Marshal.GetLastWin32Error() == 5;
                if (attachDenied) throw new InvalidOperationException("Process is already attached to a console");

                var allocated = NativeMethods.AllocConsole();
                if (allocated == false) throw new InvalidOperationException("Process already has a console");
            }
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            NativeMethods.FreeConsole();
        }
        ~AttachConsole()
        {
            Dispose(false);
        }
    }

    internal static partial class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
    }
}
