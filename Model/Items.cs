using Redis.OM.Modeling;

namespace Model
{
    [Document(IndexName = "Idx-Items", StorageType = StorageType.Json)]
    public class Items
    {
        [RedisIdField]
        [Indexed(Sortable = true)]
        public int? id { get; set; } = null;
        [Indexed(Sortable = true)]
        public int code { get; set; } = 0;
        [Indexed(Sortable = true)]
        public string value { get; set; } = "";

    }
}
