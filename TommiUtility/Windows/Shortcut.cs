using System;
using System.Collections.Generic;
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
            var shortcutfullPath = Path.GetFullPath(shortcutPath);

            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);

            dynamic shortcut = shell.CreateShortcut(shortcutfullPath);
            shortcut.TargetPath = Path.GetFullPath(targetPath);
            shortcut.WorkingDirectory = Path.GetDirectoryName(shortcut.TargetPath);
            shortcut.Save();
        }
    }
}
