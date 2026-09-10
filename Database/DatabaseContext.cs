using BookTable.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookTable.Database
{
    public class DatabaseContext : DbContext
    {

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Table> Tables => Set<Table>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

    }
}
