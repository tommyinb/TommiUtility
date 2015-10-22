using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TommiUtility.Winform
{
    public static class ControlUtil
    {
        public static void InvokeAction(this Control control, Action action)
        {
            control.Invoke(action);
        }
        public static T InvokeFunc<T>(this Control control, Func<T> func)
        {
            return (T)control.Invoke(func);
        }

        public static void ForceCreateHandle(this Control control)
        {
            if (control.IsHandleCreated) return;

            var controlType = typeof(System.Windows.Forms.Control);
            var methodInfo = controlType.GetMethod("CreateHandle", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(control, null);
        }
        public static void ForceDoubleBuffered(this Control control)
        {
            var controlType = typeof(System.Windows.Forms.Control);
            var propertyInfo = controlType.GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);

            propertyInfo.SetValue(control, true, null);
        }
    }
}
