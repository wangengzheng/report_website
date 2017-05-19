using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AVGD.Common
{
    /// <summary>
    /// Remove Response Headers Server
    /// </summary>
    public class RemoveSeverHeaderModule :IHttpModule
    {

        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }


        void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Headers.Remove("Server");
        
        }
    }
}