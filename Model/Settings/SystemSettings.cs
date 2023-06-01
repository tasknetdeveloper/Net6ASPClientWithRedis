
namespace Model
{
    public class SystemSettings: ISystemSettings
    {
        public string ConnectionDB { get; set; } = "";
        public string RedisConnection { get; set; } = "";
        public bool isLogEnable { get; set; } = false;
        public int NRrowsInPage { get; set; } = 1000;
        public int MaxSavingItems { get; set; } = 100;
        public string defaultRedisIndex { get; set; } = "Idx-Items";
    }
}
