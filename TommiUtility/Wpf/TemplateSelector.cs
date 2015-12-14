using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TommiUtility.Wpf
{
    public class TemplateSelector : DataTemplateSelector
    {
        public IEnumerable<TemplateSelectType> SelectTypes { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return null;

            if (SelectTypes == null) return null;

            var selectType = SelectTypes.FirstOrDefault(t => t.Type.IsAssignableFrom(item.GetType()));

            if (selectType == null) return null;

            return selectType.Template;
        }
    }

    public class TemplateSelectType : DependencyObject
    {
        public Type Type { get; set; }

        public DataTemplate Template { get; set; }
    }
}
