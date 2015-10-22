using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.Web
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class CookieWebClient : WebClient
    {
        public CookieContainer Cookies { get; set; }
        public CookieWebClient()
        {
            this.Cookies = new CookieContainer();
        }
        
        protected override WebRequest GetWebRequest(Uri address)
        {
            var webRequest = base.GetWebRequest(address);

            if (webRequest is HttpWebRequest)
            {
                var httpWebRequest = (HttpWebRequest)webRequest;
                httpWebRequest.CookieContainer = Cookies;
            }

            return webRequest;
        }
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var webResponse = base.GetWebResponse(request);
            
            if (webResponse is HttpWebResponse)
            {
                var httpWebResponse = (HttpWebResponse)webResponse;
                if (httpWebResponse.Cookies != null)
                {
                    Cookies.Add(httpWebResponse.Cookies);
                }
            }
            return webResponse;
        }
    }
}
