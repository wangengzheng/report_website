using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AVGD.Common
{
    public class AntiDosAttack
    {
        readonly static List<IpObject> items = new List<IpObject>();

        public static void Monitor(int Capacity, int Seconds2Keep, int AllowedCount)
        {
            string ip = HttpContext.Current.Request.UserHostAddress;

            if (ip == "")
                return;

            // This part to exclude some useful requesters
            if (HttpContext.Current.Request.UserAgent != null && HttpContext.Current.Request.UserAgent == "Some good bots")
                return;

            // to remove old requests from collection
            int index = -1;
            for (int i = 0; i < items.Count; i++)
            {

                if ((DateTime.Now - items[i].Date).TotalSeconds > Seconds2Keep)
                {
                    index = i;
                    break;
                }
            }

            if (index > -1)
            {
               items.RemoveRange(index, items.Count - index);
            }

            // Add new IP
            items.Insert(0, new IpObject(ip));

            // Trim collection capacity to original size, I could not find a better reliable way
            if (items.Count > Capacity)
            {
                items.RemoveAt(items.Count - 1);
            }

            // Count of currect IP in collection
            int count = items.Count(t => t.IP == ip);

            // Decide on block or bypass
            if (count > AllowedCount)
            {
                // alert webmaster by email (optional)
                //ErrorReport.Report.ToWebmaster(new Exception("Blocked probable ongoing ddos attack"), "EvrinHost 24 / 7 Support - DDOS Block", "");

                // create a response code 429 or whatever needed and end response
                HttpContext.Current.Response.StatusCode = 429;
                HttpContext.Current.Response.StatusDescription = "Too Many Requests, Slow down Cowboy!";
                HttpContext.Current.Response.Write("Too Many Requests");
                HttpContext.Current.Response.Flush(); // Sends all currently buffered output to the client.
                HttpContext.Current.Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
                HttpContext.Current.ApplicationInstance.CompleteRequest(); // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event.
            }
        }

        internal class IpObject
        {
            public IpObject(string ip)
            {
                IP = ip;
                Date = DateTime.Now;
            }

            public string IP { get; set; }
            public DateTime Date { get; set; }
        }
    }
}