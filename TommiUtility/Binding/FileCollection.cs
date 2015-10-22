using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TommiUtility.Binding
{
    internal interface FileCollectionReference<T>
    {
        FileSystem.FileCollection<T> Reference { get; }
    }
}
