using Redis.OM;
using LoggerSpace;
using Redis.OM.Searching;
using System.Linq.Expressions;
using StackExchange.Redis;

namespace RedisAbstractSpace
{
    public sealed class RedisAbstract<T> : RedisAbstractBase<T> where T : class, new()
    {
        public RedisConnectionProvider? provider { get; private set; }
        public RedisAbstract(Log log, RedisConnectionProvider provider, 
                                                      string indexName): base(provider)
        {
            this.provider = provider;
            this.log = log;
            Ini(provider, $"{indexName}-idx");
        }

        public RedisAbstract(Log log, RedisConnectionProvider provider):base(provider)
        {
            this.log = log;
            this.provider = provider;
        }

        public T[]? GetAll()
        {
            T[]? result = null;
            if (list == null) return null;

            try
            {                
                if (list != null)
                {
                    result = list.ToArray();
                }
                else
                    log.TraceInfo("GetAll list is null");
            }
            catch (RedisException exp0)
            {
                log.Error($"GetAll/{exp0.Message}");
            }
            catch (Exception exp)
            {
                log.Error($"GetAll/{exp.Message}");
            }
            return result;
        }

        public IEnumerable<T>? GetIEnumerableSearchResult(Expression<Func<T, bool>>? expression = null)
        {
            IEnumerable<T>? result = null;
            if (list == null || expression == null) return null;

            try
            {
                var r = list.Where(expression);
                if (r != null)
                {
                    result = r;
                }
                else
                    log.TraceInfo("GetSearchResult2: r is null");
            }
            catch (RedisException exp0)
            {
                log.Error($"GetSearchResult2/{exp0.Message}");
            }
            catch (Exception exp)
            {
                log.Error($"GetSearchResult2/{exp.Message}");
            }
            return result;
        }

        public T[]? GetSearchResult(Expression<Func<T, bool>>? expression = null)
        {
            T[]? result = null;
            if (list == null || expression==null) return null;

            try
            {
                var r = list.Where(expression);
                if (r != null)
                {
                    result = r.ToArray();
                }
                else
                    log.TraceInfo("GetSearchResult r is null");
            }
            catch (RedisException exp0)
            {
                log.Error($"GetSearchResult/{exp0.Message}");
            }
            catch (Exception exp)
            {
                log.Error($"GetSearchResult/{exp.Message}");
            }
            return result;
        }

        public void LoadData(T[] items)
        {
            if (items == null) return;
            if (provider == null) return;

            list = (RedisCollection<T>)provider.RedisCollection<T>();
            foreach (var item in items)
            {
                list.Insert(item);
            }
        }

        private void Ini(RedisConnectionProvider? provider_, string indexName)
        {
            try
            {
                if (provider_ == null) return;
                provider = provider_;

                CheckIndex(provider_, indexName);
            }
            catch (Exception exp)
            {
                log.Error($"Ini/{exp.Message}");
            }
        }
    }
}
