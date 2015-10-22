using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Web
{
    public static class StandardTime
    {
        public static DateTime Get()
        {
            using (var webClient = new WebClient())
            {
                var response = webClient.DownloadString(
                    "http://www.hko.gov.hk/cgi-bin/gts/time5a.pr");

                var match = Regex.Match(response, @"\d+=(?<value>\d+)");
                var value = long.Parse(match.Groups["value"].Value);
                var timeSpan = new TimeSpan(value * TimeSpan.TicksPerMillisecond);

                var standardStartTime = DateTime.SpecifyKind(
                    new DateTime(1970, 1, 1, 0, 0, 0, 0), DateTimeKind.Utc);
                
                var standardTime = standardStartTime + timeSpan;
                return standardTime.ToLocalTime();
            }
        }
    }

    [TestClass]
    public class StandardTimeTest
    {
        [TestMethod]
        public void Test()
        {
            DateTime standardTime = StandardTime.Get();

            TimeSpan timeSpan = DateTime.Now - standardTime;

            Assert.IsTrue(Math.Abs(timeSpan.TotalMinutes) < 5);
        }
    }
}
