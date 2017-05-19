using System;
using System.IO;
using System.Threading.Tasks;

namespace AVGD.Common
{
    public class BugLog
    {
        private static object lockObject = new object();
       private static void Write2File(string msg,string file=null)
        {
            lock (lockObject)
            {
                try
                {
                    //创建系统错误日志文件夹
                    string rootPath = AppDomain.CurrentDomain.BaseDirectory + "SysLog";
                    DirectoryInfo di = new DirectoryInfo(rootPath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }
                    string fileName = rootPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                    if (file.IsNotEmpty())
                        fileName = file;

                    FileInfo fi = new FileInfo(fileName);
                    if (!fi.Exists)
                    {
                        fi.Create().Close();
                    }
                    StreamWriter sw = File.AppendText(fileName);
                    //写入日志
                    sw.WriteLine("运行时间：" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + "类型：" + "消息" + "\r\n" + "消息信息：" + msg + "\r\n");
                    sw.Close();
                }
                catch
                {

                }
            }
        }

        /// <summary>
       /// 异步方法写入 系统日志
        /// </summary>
        /// <param name="msg">信息</param>
       /// <param name="file">AppDomain.CurrentDomain.BaseDirectory + "SysLog"+"\\" + DateTime.Now.ToString("yyyyMMdd") + ".log"; 为null时有默认路径</param>
       ///当filePath不为null时  指定的是 文件（可以读写的文件）
       public static void Write(string msg, string file = null)
        {
            Task.Factory.StartNew(() =>
            {
                BugLog.Write2File(msg,file);
            });
        }


       public static void NoAsyncWrite(string msg, string file = null)
       {
           Write2File(msg, file);
       }
    }
}