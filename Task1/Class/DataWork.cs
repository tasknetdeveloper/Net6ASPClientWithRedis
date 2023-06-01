using Model;
using Redis.OM;
using RedisAbstractSpace;
using LoggerSpace;
using Dal;

namespace Task1
{
    internal class DataWork
    {
        private RedisAbstract<Items>? redis = null;        
        private ReposiotoryWork? repo = null;       
        
        private ISystemSettings settings;
        internal DataWork(RedisConnectionProvider? provider, Log log, 
            ISystemSettings settings) {

            this.settings = settings;
            
            if(provider!=null)
                redis = new(log, provider, this.settings.defaultRedisIndex);

            CheckRedis(this.settings.defaultRedisIndex);
           
            if (!string.IsNullOrEmpty(settings.ConnectionDB))
            {
                repo = new(log, redis,settings);
                Items[]? tmplist = null;
                if (redis != null)
                {
                    tmplist = redis.GetSearchResult(x => x.code == 0);
                }   
            }            
        }

        internal bool CheckRedis(string indexName)
        {
            if (redis == null) return false;
            try
            {
                return redis.isExistIndex(indexName);
            }
            catch 
            {
                redis = null;
                return false;
            }
        }

        internal async Task<bool> SaveItems(Items[] list)
        {
            return await Task<bool>.Run(() => {
                return (repo!=null)? repo.AddItems(list) : false;
            });            
        }

        internal async Task<ItemsInfo?> GetItemsInfoExt(FilterRequest request, int iPage, int N)
        {
            if (request == null || repo == null) return null;

            var Npages = 0;
            if (N == 0) N = 1;

            var context = repo.OpenConnectionExt();

            ItemsInfo result = new();
            IEnumerable<Items> ? list = null;
            

            await Task.Run(() => {
                
                if (request.code > 0)
                {
                    if (redis != null && redis.isExistIndex(this.settings.defaultRedisIndex))
                        list = redis.GetIEnumerableSearchResult(x => x.code == request.code);
                    else
                        list = repo.GetItemsExt(x => x != null && x.code == request.code, context);
                }

                if (!string.IsNullOrEmpty(request.value))
                {
                    if (list != null && list.Count() > 0)
                        list = SearchIsideResultExt(request, list, x => x != null && x.value.ToLower() == request.value.ToLower());
                    else
                        list = repo.GetItemsExt(x => x != null && x.value == request.value, context);
                }

              

                if (list != null)
                    Npages = list.Count() / N;

            }).ConfigureAwait(false);

            if (list != null && list.Count() >= (iPage * N))
                result.items = list.ToList().Skip(iPage * N).Take(N);

            result.Npages = Npages;
            result.PageNumber = iPage;
            
            repo.CloseConnectionExt();

            return result;
        }
       

        private IEnumerable<Items>? SearchIsideResultExt(FilterRequest request, IEnumerable<Items>? list,
                                           Func<Items, bool>? expression = null)
        {
            IEnumerable<Items>? result = null;
            if (list != null && expression != null)
            {
                var r = list.Where(expression);
                if (r != null && r.FirstOrDefault() != null)
                {
                    result = r;
                }
            }
            return result;
        }

        private Items[]? SearchIsideResult(FilterRequest request, IEnumerable<Items>? list,
                                           Func<Items, bool>? expression = null)
        {
            Items[]? result = null;
            if (list != null && expression != null)
            {

                var r = list.Where(expression);
                if (r != null && r.FirstOrDefault() != null)
                {
                    result = r.ToArray();
                }
            }
            return result;
        }


    }
}
