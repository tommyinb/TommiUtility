using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Logging
{
    internal interface LogWriter
    {
        FileSystem.LogWriter Reference { get; }
    }
}
