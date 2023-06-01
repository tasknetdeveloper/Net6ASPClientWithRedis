using Microsoft.EntityFrameworkCore;
using Model;
namespace Dal
{
    
    public class ApplicationContext : DbContext
    {
        public DbSet<LogInDb>? LogInDb { get; set; } = null;
        public DbSet<Items>? Items { get; set; } = null;
      
        private string connection = "";

        public ApplicationContext(string connection)
        {
            this.connection = connection;            
            Database.EnsureCreated();           
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connection);
            //optionsBuilder.UseMySql(this.connection,

            //    ServerVersion.AutoDetect(this.connection),
            //    options => options.EnableRetryOnFailure(
            //        maxRetryCount: 5,
            //        maxRetryDelay: System.TimeSpan.FromSeconds(30),
            //        errorNumbersToAdd: null));
        }
    }
}
