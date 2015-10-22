using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Web
{
    public static class CookieUtil
    {
        public static IEnumerable<Cookie> GetCookies(this CookieContainer cookieContainer)
        {
            var domainTableField = cookieContainer.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == "m_domainTable");
            var domainTable = (IDictionary)domainTableField.GetValue(cookieContainer);
            
            foreach (var domain in domainTable.Values)
            {
                var listField = domain.GetType().GetRuntimeFields().First(x => x.Name == "m_list");
                var list = (IDictionary)listField.GetValue(domain);

                foreach (CookieCollection cookies in list.Values)
                {
                    foreach (Cookie cookie in cookies)
                    {
                        yield return cookie;
                    }
                }
            }
        }
        
        public static void RemoveCookie(this CookieContainer cookieContainer, string name)
        {
            var domainTableField = cookieContainer.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == "m_domainTable");
            var domainTable = (IDictionary)domainTableField.GetValue(cookieContainer);

            foreach (var domain in domainTable.Values)
            {
                var listField = domain.GetType().GetRuntimeFields().First(x => x.Name == "m_list");
                var list = (IDictionary)listField.GetValue(domain);
                var keys = list.Keys.Cast<object>().ToArray();

                foreach (var key in keys)
                {
                    var cookies = (CookieCollection)list[key];
                    if (cookies.Cast<Cookie>().Any(t => t.Name == name))
                    {
                        var newCookies = new CookieCollection();
                        foreach (Cookie cookie in cookies)
                        {
                            if (cookie.Name != name)
                            {
                                newCookies.Add(cookie);
                            }
                        }

                        list[key] = newCookies;
                    }
                }
            }
        }
    }

    [TestClass]
    public class CookieUtilTest
    {
        [TestMethod]
        public void TestGetCookies()
        {
            var cookieContainer = new CookieContainer();

            var uri1 = new Uri("http://localhost/");
            var cookie1 = new Cookie("abc1", "123");
            var cookie2 = new Cookie("abc2", "234");
            cookieContainer.Add(uri1, cookie1);
            cookieContainer.Add(uri1, cookie2);

            var uri2 = new Uri("http://google.com/");
            var cookie3 = new Cookie("abc3", "345");
            var cookie4 = new Cookie("abc4", "456");
            cookieContainer.Add(uri2, cookie3);
            cookieContainer.Add(uri2, cookie4);

            var resultCookies = cookieContainer.GetCookies().OrderBy(t => t.Name).ToArray();
            Assert.AreEqual(4, resultCookies.Length);

            Assert.AreEqual(uri1.Authority, resultCookies[0].Domain);
            Assert.AreEqual(cookie1.Name, resultCookies[0].Name);
            Assert.AreEqual(cookie1.Value, resultCookies[0].Value);

            Assert.AreEqual(uri1.Authority, resultCookies[1].Domain);
            Assert.AreEqual(cookie2.Name, resultCookies[1].Name);
            Assert.AreEqual(cookie2.Value, resultCookies[1].Value);

            Assert.AreEqual(uri2.Authority, resultCookies[2].Domain);
            Assert.AreEqual(cookie3.Name, resultCookies[2].Name);
            Assert.AreEqual(cookie3.Value, resultCookies[2].Value);

            Assert.AreEqual(uri2.Authority, resultCookies[3].Domain);
            Assert.AreEqual(cookie4.Name, resultCookies[3].Name);
            Assert.AreEqual(cookie4.Value, resultCookies[3].Value);
        }

        [TestMethod]
        public void TestRemoveCookies()
        {
            var cookieContainer = new CookieContainer();

            var uri1 = new Uri("http://localhost/");
            var cookie1 = new Cookie("abc1", "123");
            var cookie2 = new Cookie("abc2", "234");
            cookieContainer.Add(uri1, cookie1);
            cookieContainer.Add(uri1, cookie2);

            var uri2 = new Uri("http://google.com/");
            var cookie3 = new Cookie("abc3", "345");
            var cookie4 = new Cookie(cookie2.Name, "456");
            cookieContainer.Add(uri2, cookie3);
            cookieContainer.Add(uri2, cookie4);

            var addedCookies = cookieContainer.GetCookies().OrderBy(t => t.Name).ToArray();
            Assert.AreEqual(4, addedCookies.Length);
            Assert.AreEqual(cookie1.Name, addedCookies[0].Name);
            Assert.AreEqual(cookie2.Name, addedCookies[1].Name);
            Assert.AreEqual(cookie4.Name, addedCookies[2].Name);
            Assert.AreEqual(cookie3.Name, addedCookies[3].Name);

            cookieContainer.RemoveCookie(cookie2.Name);

            var resultCookies = cookieContainer.GetCookies().OrderBy(t => t.Name).ToArray();
            Assert.AreEqual(2, resultCookies.Length);
            Assert.AreEqual(cookie1.Name, resultCookies[0].Name);
            Assert.AreEqual(cookie3.Name, resultCookies[1].Name);
        }
    }
}
