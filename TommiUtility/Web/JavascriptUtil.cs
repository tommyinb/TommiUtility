using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Web
{
    public static class JavascriptUtil
    {
        public static DateTime GetDateTime(long value)
        {
            var baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var utcTime = baseTime.AddMilliseconds(value);

            return utcTime.ToLocalTime();
        }
        public static long GetTimeValue(DateTime dateTime)
        {
            var baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var timeDifference = dateTime.ToUniversalTime().Subtract(baseTime);

            return (long)timeDifference.TotalMilliseconds;
        }
    }

    [TestClass]
    public class JavascriptUtilTest
    {
        [TestMethod]
        public void TestGetDateTime()
        {
            var resultDateTime1 = JavascriptUtil.GetDateTime(1434619799688);
            var expectedUtcDateTime1 = new DateTime(2015, 6, 18, 9, 29, 59, 688, DateTimeKind.Utc).ToLocalTime();
            Assert.AreEqual(expectedUtcDateTime1, resultDateTime1);

            var resultDateTime2 = JavascriptUtil.GetDateTime(2914651801103);
            var expectedDateTime2 = new DateTime(2062, 5, 12, 9, 30, 1, 103, DateTimeKind.Utc).ToLocalTime();
            Assert.AreEqual(expectedDateTime2, resultDateTime2);
        }

        [TestMethod]
        public void TestGetTimeValue()
        {
            var dateTime1 = new DateTime(2015, 6, 18, 9, 29, 59, 688, DateTimeKind.Utc).ToLocalTime();
            var value1 = JavascriptUtil.GetTimeValue(dateTime1);
            Assert.AreEqual(1434619799688, value1);

            var dateTime2 = new DateTime(2062, 5, 12, 9, 30, 1, 103, DateTimeKind.Utc).ToLocalTime();
            var value2 = JavascriptUtil.GetTimeValue(dateTime2);
            Assert.AreEqual(2914651801103, value2);
        }
    }
}
