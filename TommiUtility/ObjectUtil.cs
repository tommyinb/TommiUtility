using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility
{
    public static class ObjectUtil
    {
        public static bool EqualsAny(this object @object, params object[] objects)
        {
            if (objects.Length <= 0)
                return false;

            return objects.Any(t => @object.Equals(t));
        }

        public static T ChangeType<T>(object value)
        {
            var @object = ChangeType(value, typeof(T));
            return (T)@object;
        }
        public static object ChangeType(object value, Type type)
        {
            try
            {
                return Convert.ChangeType(value, type);
            }
            catch (InvalidCastException)
            {
                var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
                if (isNullable == false) throw;

                if (value == null) return null;

                var nonNullableType = type.GetGenericArguments().Single();
                var nonNullableValue = Convert.ChangeType(value, nonNullableType);
                return Activator.CreateInstance(type, nonNullableValue);
            }
        }
    }

    [TestClass]
    public class ObjectUtilTest
    {
        [TestMethod]
        public void TestEqualsAny()
        {
            Assert.IsTrue("A".EqualsAny("A"));
            Assert.IsTrue("A".EqualsAny("A", "B"));
            Assert.IsFalse("A".EqualsAny("B"));

            Assert.IsTrue(1.EqualsAny(1));
            Assert.IsTrue(1.EqualsAny(1, 2));
            Assert.IsFalse(1.EqualsAny(2));

            Assert.IsFalse("X".EqualsAny());
        }

        [TestMethod]
        public void TestChangeType()
        {
            Assert.AreEqual(3, ObjectUtil.ChangeType<int>("3"));
            Assert.AreEqual(3, ObjectUtil.ChangeType<int?>("3"));
            Assert.AreEqual(null, ObjectUtil.ChangeType<int?>(null));

            Assert.AreEqual("3", ObjectUtil.ChangeType<string>("3"));
            Assert.AreEqual(null, ObjectUtil.ChangeType<string>(null));
        }
    }
}
