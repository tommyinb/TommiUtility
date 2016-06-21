using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Windows
{
    public class FileCommand
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Extension { get; set; }

        public static FileCommand Get(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null);
            Contract.Ensures(Contract.Result<FileCommand>() != null);

            var key = @"HKEY_CLASSES_ROOT\*\shell\" + name;
            return new FileCommand
            {
                Name = name,
                Command = Registry.GetValue(key + @"\command", string.Empty, null) as string,
                Extension = Registry.GetValue(key, "AppliesTo", null) as string
            };
        }
        public void Save()
        {
            Contract.Requires<InvalidOperationException>(Name != null);
            Contract.Requires<InvalidOperationException>(Command != null);

            var key = @"HKEY_CLASSES_ROOT\*\shell\" + Name;
            Registry.SetValue(key + @"\command", string.Empty, Command);

            if (Extension != null)
            {
                Registry.SetValue(key, "AppliesTo", Extension);
            }
        }
        public static void Remove(string name)
        {
            var key = Registry.ClassesRoot.OpenSubKey(@"*\shell\", writable: true);
            if (key == null) return;

            key.DeleteSubKeyTree(name, throwOnMissingSubKey: false);
        }
    }
}
