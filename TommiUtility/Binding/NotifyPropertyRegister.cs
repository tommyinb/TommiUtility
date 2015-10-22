using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TommiUtility.Runtime;

namespace TommiUtility.Binding
{
    public class NotifyPropertyRegister
    {
        public NotifyPropertyRegister(PropertyChangedEventHandler propertyChangedEventHandler)
        {
            this.propertyChangedEventHandler = propertyChangedEventHandler;
        }
        private PropertyChangedEventHandler propertyChangedEventHandler;

        public T GetValue<T>([CallerMemberName]string propertyName = "")
        {
            if (values.ContainsKey(propertyName))
            {
                var value = values[propertyName];
                return (T)value;
            }
            else
            {
                return default(T);
            }
        }
        public void SetValue<T>(T value, [CallerMemberName]string propertyName = "")
        {
            var oldValue = values.ContainsKey(propertyName) ? values[propertyName] : default(T);
            
            if (object.Equals(oldValue, value))
            {
                return;
            }

            values[propertyName] = value;

            var e = new PropertyChangedEventArgs(propertyName);
            propertyChangedEventHandler(propertyChangedEventHandler.Target, e);
        }

        private Dictionary<string, object> values = new Dictionary<string, object>();
    }

    [TestClass]
    public class NotifyPropertyRegisterTest : INotifyPropertyChanged
    {
        public NotifyPropertyRegisterTest()
        {
            propertyRegister = new NotifyPropertyRegister(OnPropertyChanged);
        }
        private NotifyPropertyRegister propertyRegister;
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged(sender, e);
        }

        public string TestText
        {
            get { return propertyRegister.GetValue<string>(); }
            set { propertyRegister.SetValue(value); }
        }
        public int TestInteger
        {
            get { return propertyRegister.GetValue<int>(); }
            set { propertyRegister.SetValue(value); }
        }

        [TestMethod]
        public void Test()
        {
            var testObject = new NotifyPropertyRegisterTest();

            var changedProperties = new List<object>();

            testObject.PropertyChanged += (sender, e) =>
            {
                changedProperties.Add(sender);
                changedProperties.Add(e.PropertyName);
            };

            testObject.TestText = "abc";
            Assert.AreEqual("abc", testObject.TestText);
            testObject.TestText = "bcd";
            Assert.AreEqual("bcd", testObject.TestText);

            testObject.TestInteger = 3;
            Assert.AreEqual(3, testObject.TestInteger);
            testObject.TestInteger = 5;
            Assert.AreEqual(5, testObject.TestInteger);
            testObject.TestInteger = 5;
            Assert.AreEqual(5, testObject.TestInteger);

            Assert.IsTrue(changedProperties.SequenceEqual(new object[]
            {
                testObject, CodeUtil.GetName(() => testObject.TestText),
                testObject, CodeUtil.GetName(() => testObject.TestText),
                testObject, CodeUtil.GetName(() => testObject.TestInteger),
                testObject, CodeUtil.GetName(() => testObject.TestInteger)
            }));
        }
    }
}
