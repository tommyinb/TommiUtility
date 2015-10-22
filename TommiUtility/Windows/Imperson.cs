using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Windows
{
    public class Imperson : IDisposable
    {
        public Imperson(string domain, string user, string password)
        {
            bool logon = NativeMethods.LogonUser(
                user, domain, password,
                2, 0,
                out userHandle);

            if (logon == false)
                throw new Win32Exception(
                    Marshal.GetLastWin32Error());

            context = WindowsIdentity
                .Impersonate(userHandle);
        }
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && context != null)
            {
                context.Undo();

                context.Dispose();
                context = null;
            }

            if (userHandle != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(userHandle);

                userHandle = IntPtr.Zero;
            }
        }
        ~Imperson()
        {
            Dispose(false);
        }

        private IntPtr userHandle = IntPtr.Zero;
        private WindowsImpersonationContext context;
    }

    internal static partial class NativeMethods
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LogonUser(
            string lpszUsername, string lpszDomain, string lpszPassword,
            int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hHandle);
    }
}
