using Microsoft.EntityFrameworkCore;


namespace CreateWebApiProj
{
    public partial class ApiProjectDbContext : DbContext
    {
        string _connString;
        public ApiProjectDbContext(string connString)
        {
            _connString = connString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connString);
            }
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
