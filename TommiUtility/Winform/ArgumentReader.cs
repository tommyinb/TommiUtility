using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Winform
{
    internal interface ArgumentReader
    {
        ProgramFlow.ArgumentReader Reference { get; }
    }
}
