using api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Infrastructure.Database
{
    public class TransfersDbContext : DbContext
    {
        public TransfersDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
    }
}