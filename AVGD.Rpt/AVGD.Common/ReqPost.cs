using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace AVGD.Common
{
    public class ReqPost
    {
        /// <summary>
        /// 请求方法WebResponse/WebRequest
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string CallRequestMethod(ResponseCallModel model)
        {
            try
            { 
                Uri uri = null;
                try
                {
                    uri = new Uri(model.URI);
                }
                catch (Exception ex)
                {                    
                    throw new UriFormatException("url 错误" + model.URI +ex.ToString());
                }
                WebRequest req = HttpWebRequest.Create(model.URI);
                string format = string.Empty;
                if (model.EncodingFormat == EncodingFormat.UTF8)
                {
                    format = "utf-8";
                }
                else
                {
                    format = model.EncodingFormat.ToString();
                }
                Encoding encoding = Encoding.GetEncoding(format);
                string param = model.RequestParam;
                param = param.Replace("+", "%2B");
                param = param.Replace("&", "%26");
                //param = param.Replace("/", "%2F");
                //param = param.Replace("?", "%3F");
                //param = param.Replace("%", "%25");
                //param = param.Replace("#", "%23");
                //param = param.Replace("<", "&lt;");
                byte[] bs = encoding.GetBytes(param);  //Encoding.ASCII.GetBytes
                req.Method = model.RequestMethod.ToString();
                //req.ContentType = "application/x-www-form-urlencoded"; text/plain; charset=utf-8
                req.ContentType = "text/plain; charset=utf-8"; 
                req.ContentLength = bs.Length;
                using (Stream sw = req.GetRequestStream())
                {
                    sw.Write(bs, 0, bs.Length);
                    //sw.Close();
                }
                string responseData = String.Empty;

                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseData = reader.ReadToEnd().ToString();
                    }
                }
                return responseData;
            }
            catch (Exception ex)
            {
                BugLog.Write(ex.ToString());
                throw new WebException("http请求出错！！" + ex.ToString());
            }
        }

        /// <summary>
        /// 请求方法WebClient
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string CallRequestMethodWebClient(ResponseCallModel model)
        {
            try
            {
                WebClient wc = new WebClient();
                string format = string.Empty;
                if (model.EncodingFormat == EncodingFormat.UTF8)
                {
                    format = "utf-8";
                }
                else
                {
                    format = model.EncodingFormat.ToString();
                }
                Encoding encoding = Encoding.GetEncoding(format);
                //下面是GB2312编码
                byte[] sendData = encoding.GetBytes(model.RequestParam);
                //wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                wc.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                wc.Headers.Add("ContentLength", sendData.Length.ToString());
                byte[] recData = wc.UploadData(model.URI, model.RequestMethod.ToString(), sendData);
                return encoding.GetString(recData);
            }
            catch (Exception ex)
            {
                BugLog.Write(ex.ToString());
                throw new WebException("WebClient请求出错！！" + ex.ToString());
            }
        }

        public static YPResultSet PostSendMicroMessage(string jsonString, string sendUrl)
        {
            try
            {
                System.Net.HttpWebRequest hwRequest;
                System.Net.HttpWebResponse hwResponse;
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bData = encoding.GetBytes(jsonString);

                hwRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(sendUrl);
                hwRequest.Timeout = 5000;
                hwRequest.Method = "POST";
                hwRequest.ContentType = "text/plain; charset=utf-8";
                hwRequest.ContentLength = bData.Length;

                System.IO.Stream smWrite = hwRequest.GetRequestStream();
                smWrite.Write(bData, 0, bData.Length);
                smWrite.Close();

                hwResponse = (HttpWebResponse)hwRequest.GetResponse();
                StreamReader srReader = new StreamReader(hwResponse.GetResponseStream(), Encoding.UTF8);
                string strResult = srReader.ReadToEnd();
                srReader.Close();
                hwResponse.Close();
                YPResultSet set = JsonConvert.DeserializeObject<YPResultSet>(strResult);
                //YPResultSet set = null;
                return set;
            }
            catch (Exception ex)
            {
                return new YPResultSet
                {
                    msg = ex.Message,
                    code = "no",
                    data = ""
                };
            }
        }

    }
}

public class ResponseCallModel
{
    public string URI { get; set; }
    public string RequestParam { get; set; }
    public RequestMethod RequestMethod { get; set; }
    public EncodingFormat EncodingFormat { get; set; }
}

public enum RequestMethod
{
    POST,
    GET,
}

public enum EncodingFormat
{
    UTF8,
    GB2312
}



/// <summary>
/// 订单 ViewModel
/// </summary>
public class MessageValidation
{
    /// <summary>
    /// appkey
    /// </summary>
    public string appKey { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public string timestamp { get; set; }


    /// <summary>
    /// appSecret
    /// </summary>
    public string appSecret { get; set; }

    /// <summary>
    /// 对象数据
    /// </summary>
    public string data { get; set; }

   
}


public class YPResultSet
{
    public string code { get; set; }
    public string data { get; set; }
    public string msg { get; set; }
}