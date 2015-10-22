using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TommiUtility.Wpf
{
    public class TemplateSelector : DataTemplateSelector
    {
        public List<TemplateSelectType> SelectTypes { get; set; }

        public TemplateSelector()
        {
            this.SelectTypes = new List<TemplateSelectType>();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (SelectTypes == null)
                throw new InvalidOperationException();

            foreach (var selectType in SelectTypes)
            {
                if (selectType.Type.IsAssignableFrom(
                    item.GetType()) == false)
                    continue;

                return selectType.Template;
            }

            throw new InvalidOperationException();
        }
    }

    public class TemplateSelectType : DependencyObject
    {
        public Type Type { get; set; }

        public DataTemplate Template { get; set; }
    }
}
