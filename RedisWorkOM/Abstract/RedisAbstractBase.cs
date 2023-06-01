using Redis.OM.Searching;
using Redis.OM;
using LoggerSpace;

namespace RedisAbstractSpace
{
    public class RedisAbstractBase<T> where T : class, new()
    {
        protected RedisCollection<T>? list = null;
        protected Log log = new(true,true);
        private RedisConnectionProvider? provider = null;

        public RedisAbstractBase(RedisConnectionProvider provider) {
            list =(RedisCollection<T>?) provider.RedisCollection<T>();
            this.provider = provider;
        }

        public bool isExist(T item,Func<T,bool> query)
        {
            var result = false;
            if (list == null) return result;
            try
            {
                var r = list.Where(query);
                if(r!=null && r.ToArray().FirstOrDefault()!=null)
                {                 
                    result = true;
                }                
            }
            catch (Exception exp)
            {
                log.Error($"isExist/{exp.Message}");
            }
            return result;
        }

        public bool Add(T item)
        {
            var result = false;
            if (list == null) return result;
            try
            {
                list.Insert(item);
                result = true;
            }
            catch (Exception exp)
            {
                log.Error($"Add/{exp.Message}");
            }
            return result;
        }

        public bool Delete(T item)
        {
            var result = false;
            if (list == null) return result;
            try
            {
                list.Delete(item);

                result = true;
            }
            catch (Exception exp)
            {
                log.Error($"Delete/{exp.Message}");
            }
            return result;
        }

        public bool DeleteAll(RedisConnectionProvider provider, string indexName)
        {
            var result = false;
            if (list == null) return result;
            try
            {
                list = null;
                if (provider != null)
                {
                    provider.Connection.DropIndex(typeof(T));
                    CheckIndex(provider, indexName);
                }
                result = true;
            }
            catch (Exception exp)
            {
                log.Error($"DeleteAll/{exp.Message}");
            }
            return result;
        }

        protected void CheckIndex(RedisConnectionProvider provider, string indexName)
        {
            if (string.IsNullOrEmpty(indexName)) return;
            indexName= indexName.ToLower();
            var info = provider.Connection.Execute("FT._LIST").ToArray().Select(x => x.ToString());
            if (info.All(x => x != indexName))
            {
                provider.Connection.CreateIndex(typeof(T));
            }
        }

        public bool isExistIndex(string indexName)
        {
            var result = false;
            if (string.IsNullOrEmpty(indexName)) return result;
            indexName = indexName.ToLower();
            if (this.provider == null) return result;

            var info = this.provider.Connection.Execute("FT._LIST").ToArray().Select(x => x.ToString());
            if (info.All(x => x == indexName))
            {
                result = true;
            }
            return result;
        }
    }
}
