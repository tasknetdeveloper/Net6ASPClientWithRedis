
namespace Model
{
    public interface ISystemSettings
    {
         string ConnectionDB { get; } 
         string RedisConnection { get;}
         bool isLogEnable { get;}
         int NRrowsInPage { get; } 
         int MaxSavingItems { get;}
         string defaultRedisIndex { get; }
    }
}
