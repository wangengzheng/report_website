
namespace AVGD.Common
{
    public class CacheHelper
    {
        private static ICacheStrategy cache;

        /// <summary>
        /// 缓存
        /// </summary>
        public static ICacheStrategy Cache
        {
            get
            {
                if (CacheHelper.cache == null)
                {
                    CacheHelper.cache = new CacheManager();
                }
                return CacheHelper.cache;
            }
        }
    }
}