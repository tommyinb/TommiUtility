using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(control != null);
            Contract.Requires<ArgumentNullException>(action != null);

            control.Invoke(action);
        }
        public static T InvokeFunc<T>(this Control control, Func<T> func)
        {
            Contract.Requires<ArgumentNullException>(control != null);
            Contract.Requires<ArgumentNullException>(func != null);

            var result = control.Invoke(func);

            if (result == null) return default(T);
            
            return (T)result;
        }

        public static void ForceCreateHandle(this Control control)
        {
            Contract.Requires<ArgumentNullException>(control != null);

            if (control.IsHandleCreated) return;

            var controlType = typeof(System.Windows.Forms.Control);
            var methodInfo = controlType.GetMethod("CreateHandle", BindingFlags.NonPublic | BindingFlags.Instance);
            Contract.Assume(methodInfo != null);

            methodInfo.Invoke(control, null);
        }
        public static void ForceDoubleBuffered(this Control control)
        {
            Contract.Requires<ArgumentNullException>(control != null);

            var controlType = typeof(System.Windows.Forms.Control);
            var propertyInfo = controlType.GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            Contract.Assume(propertyInfo != null);

            propertyInfo.SetValue(control, true, null);
        }
    }
}
