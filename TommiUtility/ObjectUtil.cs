using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TommiUtility.Test;

namespace TommiUtility
{
    public static class ObjectUtil
    {
        public static bool EqualsAny(this object @object, params object[] objects)
        {
            Contract.Requires<ArgumentNullException>(@object != null);
            Contract.Requires<ArgumentNullException>(objects != null);

            return objects.Any(t => @object.Equals(t));
        }
        public static bool EqualsDefault<T>(this T @object) where T : struct
        {
            return @object.Equals(default(T));
        }

        public static T ChangeType<T>(object value)
        {
            var @object = ChangeType(value, typeof(T));

            if (@object == null) return default(T);
            return (T)@object;
        }
        public static object ChangeType(object value, Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            try
            {
                return Convert.ChangeType(value, type);
            }
            catch (InvalidCastException)
            {
                var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
                if (isNullable == false) throw;

                if (value != null)
                {
                    var nonNullableType = type.GetGenericArguments().Single();
                    var nonNullableValue = Convert.ChangeType(value, nonNullableType);
                    return Activator.CreateInstance(type, nonNullableValue);
                }
                else
                {
                    return null;
                }
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
            Assert.IsFalse("X".EqualsAny(new object[0]));
        }

        [TestMethod]
        public void TestEqualDefault()
        {
            Assert.IsTrue(((uint)0).EqualsDefault());
            Assert.IsTrue(((float)0).EqualsDefault());
            Assert.IsTrue(((double)0).EqualsDefault());
            Assert.IsTrue(((char)0).EqualsDefault());

            Assert.IsFalse(((uint)1).EqualsDefault());
            Assert.IsFalse(((float)1).EqualsDefault());
            Assert.IsFalse(((double)1).EqualsDefault());
            Assert.IsFalse(((char)1).EqualsDefault());
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
