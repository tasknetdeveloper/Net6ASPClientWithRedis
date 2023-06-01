namespace Model
{
    public interface IReposiotoryWork
    {
        bool Add_LogInDb(string message);
    }

    public class IReposiotoryWorkEmpty : IReposiotoryWork
    {
        public bool Add_LogInDb(string message)
        {
            return false;
        }
    }
}
