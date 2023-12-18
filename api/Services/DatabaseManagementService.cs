using api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public static class DatabaseManagementService
    {
        public static void MigrationInitialisation(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            serviceScope.ServiceProvider.GetService<TransfersDbContext>()?.Database.Migrate();
        }
    }
}