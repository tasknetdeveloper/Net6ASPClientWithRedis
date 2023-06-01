namespace Model
{
    public class ItemsInfo
    {
        public int Npages { get; set; } = 0;
        public int PageNumber { get; set; } = 0;
        public IEnumerable<Items>? items { get; set; }

    }
}
