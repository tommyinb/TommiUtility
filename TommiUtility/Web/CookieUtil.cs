using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TommiUtility.Test;

namespace TommiUtility.Web
{
    public static class CookieUtil
    {
        public static IEnumerable<Cookie> GetCookies(this CookieContainer cookieContainer)
        {
            Contract.Requires<ArgumentNullException>(cookieContainer != null);
            Contract.Ensures(Contract.Result<IEnumerable<Cookie>>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<Cookie>>(), t => t != null));

            var lists = GetCookieLists(cookieContainer);
            var cookieCollections = lists.SelectMany(t => t.Values.OfType<CookieCollection>());
            var cookies = cookieCollections.SelectMany(t => t.OfType<Cookie>());

            Contract.Assume(Contract.ForAll(cookies, t => t != null));
            return cookies;
        }

        private static IEnumerable<IDictionary> GetCookieLists(CookieContainer cookieContainer)
        {
            Contract.Requires<ArgumentNullException>(cookieContainer != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IDictionary>>(), t => t != null));

            var containerFields = cookieContainer.GetType().GetRuntimeFields();
            Contract.Assume(containerFields != null);
            var domainTableField = containerFields.FirstOrDefault(x => x.Name == "m_domainTable");
            Contract.Assume(domainTableField != null);
            var domainTable = (IDictionary)domainTableField.GetValue(cookieContainer);
            Contract.Assume(domainTable != null);

            var cookiesLists = domainTable.Values.OfType<object>().Select(domain =>
            {
                var domainFields = domain.GetType().GetRuntimeFields();
                Contract.Assume(domainFields != null);
                if (domainFields.Any() == false) return null;

                var listField = domainFields.First(x => x.Name == "m_list");
                return (IDictionary)listField.GetValue(domain);
            }).Where(t => t != null).ToArray();

            Contract.Assume(Contract.ForAll(cookiesLists, t => t != null));
            return cookiesLists;
        }

        public static void RemoveCookie(this CookieContainer cookieContainer, string name)
        {
            Contract.Requires<ArgumentNullException>(cookieContainer != null);
            Contract.Requires<ArgumentNullException>(name != null);

            var lists = GetCookieLists(cookieContainer);
            foreach (var list in lists)
            {
                var keys = list.Keys.Cast<object>().ToArray();
                foreach (var key in keys)
                {
                    if (key == null) continue;

                    var cookies = (CookieCollection)list[key];
                    if (cookies == null) continue;

                    var hasCookie = cookies.Cast<Cookie>().Any(t => t.Name == name);
                    if (hasCookie == false) continue;

                    var newCookies = new CookieCollection();

                    foreach (Cookie cookie in cookies)
                    {
                        if (cookie != null)
                        {
                            if (cookie.Name != name)
                            {
                                newCookies.Add(cookie);
                            }
                        }
                    }

                    list[key] = newCookies;
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
            Contract.Assume(resultCookies.Length == 4);

            Assert.IsNotNull(resultCookies[0]);
            Contract.Assume(resultCookies[0] != null);
            Assert.AreEqual(uri1.Authority, resultCookies[0].Domain);
            Assert.AreEqual(cookie1.Name, resultCookies[0].Name);
            Assert.AreEqual(cookie1.Value, resultCookies[0].Value);

            Assert.IsNotNull(resultCookies[1]);
            Contract.Assume(resultCookies[1] != null);
            Assert.AreEqual(uri1.Authority, resultCookies[1].Domain);
            Assert.AreEqual(cookie2.Name, resultCookies[1].Name);
            Assert.AreEqual(cookie2.Value, resultCookies[1].Value);

            Assert.IsNotNull(resultCookies[2]);
            Contract.Assume(resultCookies[2] != null);
            Assert.AreEqual(uri2.Authority, resultCookies[2].Domain);
            Assert.AreEqual(cookie3.Name, resultCookies[2].Name);
            Assert.AreEqual(cookie3.Value, resultCookies[2].Value);

            Assert.IsNotNull(resultCookies[3]);
            Contract.Assume(resultCookies[3] != null);
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

            var resultAddedCookies = cookieContainer.GetCookies().OrderBy(t => t.Name).ThenBy(t => t.Value).ToArray();
            var expectedAddedCookies = new[] { cookie1, cookie2, cookie4, cookie3 };
            AssertUtil.SequenceEqual(expectedAddedCookies, resultAddedCookies, (x, y) => x.Name == y.Name && x.Value == y.Value);

            cookieContainer.RemoveCookie(cookie2.Name);

            var resultRemainingCookies = cookieContainer.GetCookies().OrderBy(t => t.Name).ThenBy(t => t.Value).ToArray();
            var expectedRemainingCookies = new[] { cookie1, cookie3 };
            AssertUtil.SequenceEqual(expectedRemainingCookies, resultRemainingCookies, (x, y) => x.Name == y.Name && x.Value == y.Value);
        }
    }
}
