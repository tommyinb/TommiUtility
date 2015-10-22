using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Wpf
{
    internal interface NotifyPropertyRegister
    {
        Binding.NotifyPropertyRegister Reference { get; }
    }
}
