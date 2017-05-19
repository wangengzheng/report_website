using System.Web.Caching;

namespace AVGD.Common
{
    /// <summary>
    /// 公共缓存策略接口
    /// </summary>
    public interface ICacheStrategy
    {
        /// <summary>
        /// 到期时间,单位：秒
        /// </summary>
        int TimeOut
        {
            get;
            set;
        }

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        void AddObject(string objId, object o);

        /// <summary>
        /// 添加指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="timeOut">过期时间</param>
        void AddObject(string objId, object o, int timeOut);

        /// <summary>
        /// 添加指定ID的对象(关联指定文件组)
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="files"></param>
        void AddObjectWithFileChange(string objId, object o, params string[] files);

        /// <summary>
        /// 添加指定ID的对象(关联指定文件组)
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="callback"></param>
        /// <param name="files"></param>
        void AddObjectWithFileChange(string objId, object o, CacheItemRemovedCallback callback, params string[] files);

        /// <summary>
        /// 添加指定ID的对象(关联指定键值组)
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="o"></param>
        /// <param name="dependKey"></param>
        void AddObjectWithDepend(string objId, object o, params string[] dependKey);

        /// <summary>
        /// 移除指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        void RemoveObject(string objId);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        object RetrieveObject(string objId);

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        T RetrieveObject<T>(string objId);
    }
}