
namespace Model
{
    public class LogInDb
    {
        public int id { get; set; } = 0;
        public string Message { get; set; } = "";
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
