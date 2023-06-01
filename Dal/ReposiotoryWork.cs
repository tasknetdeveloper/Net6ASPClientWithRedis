using Model;
using LoggerSpace;
using RedisAbstractSpace;
using Redis.OM;
using Microsoft.Data.SqlClient;

namespace Dal
{

    public class ReposiotoryWork : IReposiotoryWork
    {        
        private Log log = new(true, true);        
        private ISystemSettings settings;
        private RedisAbstract<Items>? redis = null;
        
        public ReposiotoryWork(Log log,  RedisAbstract<Items>? redis, ISystemSettings settings)
        {
            this.log = log;
            this.settings = settings;
            this.redis = redis;
        }

        private ApplicationContext? extContext = null;
        public ApplicationContext OpenConnectionExt()
        {
            extContext = new ApplicationContext(this.settings.ConnectionDB);            
            return extContext;
        }
        public void CloseConnectionExt()
        {
            if(extContext!=null)
                extContext.Dispose();
        }

        #region Get
        public IEnumerable<Items>? GetItemsExt(Func<Items, bool> query, ApplicationContext r)
        {
            IEnumerable<Items>? result = null;
            try
            {
                if (r != null && r.Items != null)
                {
                    result = r.Items.Where(query);
                }                
            }
            catch (SqlException exp)
            {
                SaveException(exp, "GetItems");
            }
            return result;
        }
        #endregion

        #region Add Update
        public bool Add_LogInDb(string message)
        {            

            if (string.IsNullOrEmpty(message) ||
                !this.log.isLogEnable)
                                            return false;

            if (message.Length > 16000)
                message = message.Substring(0, 16000);

            return Add_LogInDb(new LogInDb()
            {
                Message = message,
                DateCreated = DateTime.Now
            });            
        }

        private bool Add_LogInDb(LogInDb model)
        {
            var result = false;
            if (model == null) return result;
            try
            {
                using (var r = new ApplicationContext(this.settings.ConnectionDB))
                {
                    if (r != null && r.LogInDb != null)
                    {
                        r.LogInDb.Add(model);
                        r.SaveChanges();
                        result = true;
                    }
                }
            }
            catch (SqlException exp)
            {
                SaveException(exp, "Add_LogInDb");
            }
            return result;
        }

        public bool AddItems(Items[] list)
        {
            var result = false;
            if (list == null) return result;
            try
            {
                using (var r = new ApplicationContext(this.settings.ConnectionDB))
                {
                    using (var transaction = r.Database.BeginTransaction())
                    {
                        Delete_AllItems(r);
                        try
                        {
                            AddRangeItems(list.Where(x=>x!=null).OrderBy(x=>x.code).ToArray());
                            transaction.Commit();
                        }
                        catch (Exception exp)
                        {
                            transaction.Rollback();
                            SaveException(exp, "AddItems");
                        }
                    }                    
                    result = true;
                }
            }
            catch (SqlException exp)
            {
                SaveException(exp, "AddItems");
            }

            return result;
        }

        private bool AddRangeItems(Items[] list)
        {
            var result = false;
            
            try
            {
                using (var r = new ApplicationContext(this.settings.ConnectionDB))
                {
                    if (r.Items == null) return false;

                    //save to db                    
                    r.Items.AddRange(list);
                    r.SaveChanges();

                    //save to redis
                    if (redis != null)
                    {
                        foreach (var model in list)
                        {
                            if (!redis.isExist(model, x => x.id == model.id))
                            {
                                redis.Add(model);
                            }
                        }
                    }                    
                                        
                    result = true;
                }
            }
            catch (SqlException exp)
            {
                SaveException(exp, "AddUpdate_Items");
            }
            return result;
        }

        public bool AddUpdate_Items(Items model)
        {
            var result = false;            
            if (model == null) return result;
            try
            {
                using (var r = new ApplicationContext(this.settings.ConnectionDB))
                {
                    if (r.Items == null) return false;

                    if (model.id != -1)
                    {
                        var v = r.Items.Where(x => x.id == model.id).FirstOrDefault();
                        v = model;
                        r.SaveChanges();
                    }
                    else
                    {                                           
                        r.Items.Add(model);
                        r.SaveChanges();

                        //save to redis
                        if (redis != null && !redis.isExist(model, x => x.id == model.id))
                        {
                            redis.Add(model);
                        }                                                        
                    }
                    result = true;                    
                }
            }
            catch (SqlException exp)
            {
                SaveException(exp, "AddUpdate_Items");
            }

            return result;
        }        
        #endregion

        #region Delete
       
        public bool Delete_Items(Items model)
        {
            var result = false;
            if (model == null) return result;
            try
            {
                using (var r = new ApplicationContext(this.settings.ConnectionDB))
                {
                    if (r != null && r.Items != null)
                    {
                        var d = r.Items.Where(x => x.id == model.id).FirstOrDefault();
                        if (d != null)
                        {
                            r.Items.Remove(d);
                            r.SaveChanges();
                        }

                        if (redis != null && !redis.isExist(model, x => x.id == model.id))
                        {
                            redis.Delete(model);
                        }

                        result = true;
                    }
                }
            }
            catch (SqlException exp)
            {
                SaveException(exp, "Delete_Items");
            }
            return result;
        }

        public bool Delete_AllItems(ApplicationContext context)
        {
            var result = false;
            if (context.Items==null) return result;
            try
            {                
                context.Items.RemoveRange(context.Items);
                context.SaveChanges();

                if (redis != null && redis.provider!=null)
                {
                    redis.DeleteAll(redis.provider, this.settings.defaultRedisIndex);
                }
                result = true;
            }
            catch (SqlException exp)
            {
                SaveException(exp, "Delete_AllItems");
            }
            return result;
        }
        #endregion

        private void SaveException(SqlException exp,string methodName)
        {
            if(this.settings.isLogEnable)
            {
                log.Error($"{methodName}/Message={exp.Message} InnerException={exp.InnerException}");
                Add_LogInDb($"{methodName}/Message={exp.Message} InnerException={exp.InnerException}");
            }
        }

        private void SaveException(Exception exp,string methodName)
        {
            if (this.settings.isLogEnable)
            {
                log.Error($"{methodName}/Message={exp.Message} InnerException={exp.InnerException}");
                Add_LogInDb($"{methodName}/Message={exp.Message} InnerException={exp.InnerException}");
            }            
        }
    }
}
