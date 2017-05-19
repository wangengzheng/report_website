using System;
using System.Web;
using System.Web.Caching;

namespace AVGD.Common
{
    /// <summary>
    /// 默认缓存管理类
    /// </summary>
    public class CacheManager : ICacheStrategy
    {
        private static readonly CacheManager instance;

        protected static volatile Cache webCache;

        /// <summary>
        /// 默认缓存存活期为3600秒(1小时)
        /// </summary>
        protected int _timeOut = 3600;

        private static object syncObj;

        /// <summary>
        /// 设置到期相对时间[单位: 秒] 
        /// </summary>
        public virtual int TimeOut
        {
            get
            {
                return (this._timeOut > 0) ? this._timeOut : 3600;
            }
            set
            {
                this._timeOut = ((value > 0) ? value : 3600);
            }
        }

        public static Cache GetWebCacheObj
        {
            get
            {
                return CacheManager.webCache;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        static CacheManager()
        {
            CacheManager.instance = new CacheManager();
            CacheManager.webCache = HttpRuntime.Cache;
            CacheManager.syncObj = new object();
        }

        /// <summary>
        /// 加入当前对象到缓存中
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        public virtual void AddObject(string objId, object o)
        {
            if (objId != null && objId.Length != 0 && o != null)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(this.onRemove);
                if (this.TimeOut == 7200)
                {
                    CacheManager.webCache.Insert(objId, o, null, DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.High, callBack);
                }
                else
                {
                    CacheManager.webCache.Insert(objId, o, null, DateTime.Now.AddSeconds((double)this.TimeOut), Cache.NoSlidingExpiration, CacheItemPriority.High, callBack);
                }
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        public virtual void AddObjectWith(string objId, object o)
        {
            if (objId != null && objId.Length != 0 && o != null)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(this.onRemove);
                CacheManager.webCache.Insert(objId, o, null, DateTime.Now.AddSeconds((double)this.TimeOut), Cache.NoSlidingExpiration, CacheItemPriority.High, callBack);
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中,并对相关文件建立依赖
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="files">监视的路径文件</param>
        public virtual void AddObjectWithFileChange(string objId, object o, params string[] files)
        {
            if (objId != null && objId.Length != 0 && o != null)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(this.onRemove);
                CacheDependency dep = new CacheDependency(files, DateTime.Now);
                CacheManager.webCache.Insert(objId, o, dep, DateTime.Now.AddSeconds((double)this.TimeOut), Cache.NoSlidingExpiration, CacheItemPriority.High, callBack);
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中,并对相关文件建立依赖
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="files">监视的路径文件</param>
        public virtual void AddObjectWithFileChange(string objId, object o, CacheItemRemovedCallback callback, params string[] files)
        {
            if (objId != null && objId.Length != 0 && o != null)
            {
                CacheDependency dep = new CacheDependency(files, DateTime.Now);
                CacheManager.webCache.Insert(objId, o, dep, DateTime.Now.AddSeconds((double)this.TimeOut), Cache.NoSlidingExpiration, CacheItemPriority.High, callback);
            }
        }

        /// <summary>
        /// 加入当前对象到缓存中,并使用依赖键
        /// </summary>
        /// <param name="objId">对象的键值</param>
        /// <param name="o">缓存的对象</param>
        /// <param name="dependKey">依赖关联的键值</param>
        public virtual void AddObjectWithDepend(string objId, object o, params string[] dependKey)
        {
            if (objId != null && objId.Length != 0 && o != null)
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(this.onRemove);
                CacheDependency dep = new CacheDependency(null, dependKey, DateTime.Now);
                CacheManager.webCache.Insert(objId, o, dep, DateTime.Now.AddSeconds((double)this.TimeOut), Cache.NoSlidingExpiration, CacheItemPriority.High, callBack);
            }
        }

        /// <summary>
        /// 建立回调委托的一个实例
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="reason"></param>
        public void onRemove(string key, object val, CacheItemRemovedReason reason)
        {
        }

        /// <summary>
        /// 删除缓存对象
        /// </summary>
        /// <param name="objId">对象的关键字</param>
        public virtual void RemoveObject(string objId)
        {
            if (objId != null && objId.Length != 0)
            {
                CacheManager.webCache.Remove(objId);
            }
        }

        /// <summary>
        /// 返回一个指定的对象
        /// </summary>
        /// <param name="objId">对象的关键字</param>
        /// <returns>对象</returns>
        public virtual object RetrieveObject(string objId)
        {
            object result;
            if (objId == null || objId.Length == 0)
            {
                result = null;
            }
            else
            {
                result = CacheManager.webCache.Get(objId);
            }
            return result;
        }

        /// <summary>
        /// 返回指定ID的对象
        /// </summary>
        /// <param name="objId"></param>
        /// <typeparam name="T">返回数据的类型</typeparam>
        /// <returns></returns>
        public virtual T RetrieveObject<T>(string objId)
        {
            object o = this.RetrieveObject(objId);
            return (o != null) ? ((T)((object)o)) : default(T);
        }

        public void AddObject(string objId, object o, int timeOut)
        {
            if (!string.IsNullOrEmpty(objId) && !string.IsNullOrEmpty(objId.Trim()))
            {
                CacheItemRemovedCallback callBack = new CacheItemRemovedCallback(this.onRemove);
                if (timeOut > 0)
                {
                    CacheManager.webCache.Insert(objId, o, null, DateTime.Now.AddMilliseconds((double)timeOut), Cache.NoSlidingExpiration, CacheItemPriority.High, callBack);
                }
                else
                {
                    CacheManager.webCache.Insert(objId, o, null, DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.High, callBack);
                }
            }
        }
    }
}