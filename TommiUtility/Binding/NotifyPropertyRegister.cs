using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(propertyChangedEventHandler != null);

            this.propertyChangedEventHandler = propertyChangedEventHandler;
        }
        private readonly PropertyChangedEventHandler propertyChangedEventHandler;

        private readonly Dictionary<string, object> values = new Dictionary<string, object>();
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(propertyChangedEventHandler != null);
            Contract.Invariant(values != null);
        }

        public T GetValue<T>([CallerMemberName]string propertyName = "")
        {
            Contract.Requires<ArgumentException>(string.IsNullOrEmpty(propertyName) == false);

            if (values.ContainsKey(propertyName))
            {
                var value = values[propertyName];

                if (value == null) return default(T);
                return (T)value;
            }
            else
            {
                return default(T);
            }
        }
        public void SetValue<T>(T value, [CallerMemberName]string propertyName = "")
        {
            Contract.Requires<ArgumentException>(string.IsNullOrEmpty(propertyName) == false);

            var oldValue = values.ContainsKey(propertyName) ? values[propertyName] : default(T);
            
            if (Equals(oldValue, value))
            {
                return;
            }

            values[propertyName] = value;

            var e = new PropertyChangedEventArgs(propertyName);
            propertyChangedEventHandler(propertyChangedEventHandler.Target, e);
        }
    }

    [TestClass]
    public class NotifyPropertyRegisterTest : INotifyPropertyChanged
    {
        public NotifyPropertyRegisterTest()
        {
            propertyRegister = new NotifyPropertyRegister(OnPropertyChanged);
        }
        private readonly NotifyPropertyRegister propertyRegister;
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(propertyRegister != null);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Contract.Requires(PropertyChanged != null);
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
