using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Windows
{
    public static class Shortcut
    {
        public static void Create(string shortcutPath, string targetPath)
        {
            Contract.Requires<ArgumentNullException>(shortcutPath != null);
            Contract.Requires<ArgumentException>(shortcutPath.Length > 0);
            Contract.Requires<ArgumentNullException>(targetPath != null);
            Contract.Requires<ArgumentException>(targetPath.Length > 0);

            var shortcutfullPath = Path.GetFullPath(shortcutPath);

            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) throw new InvalidOperationException();
            dynamic shell = Activator.CreateInstance(shellType);

            dynamic shortcut = shell.CreateShortcut(shortcutfullPath);
            shortcut.TargetPath = Path.GetFullPath(targetPath);
            shortcut.WorkingDirectory = Path.GetDirectoryName(shortcut.TargetPath);
            shortcut.Save();
        }
    }
}
