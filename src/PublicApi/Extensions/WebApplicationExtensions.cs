using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.PublicApi.Extensions;

public static class WebApplicationExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        app.Logger.LogInformation("Seeding Database...");

        using var scope = app.Services.CreateScope();
        var scopedProvider = scope.ServiceProvider;
        try
        {
            var catalogContext = scopedProvider.GetRequiredService<CatalogContext>();
            var identityContext = scopedProvider.GetRequiredService<AppIdentityDbContext>();

            // The SQLite database is shared with Web, which creates and seeds it; PublicApi only connects
            if (catalogContext.Database.IsSqlite())
            {
                if (!HasTables(catalogContext) || !HasTables(identityContext))
                {
                    app.Logger.LogWarning("SQLite database not found or empty. Start the Web application first to create and seed it.");
                }
                return;
            }

            await CatalogContextSeed.SeedAsync(catalogContext, app.Logger);

            var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var seedDemoUsers = app.Environment.IsLocalDevelopment()
                || app.Configuration.GetValue<bool>(AppIdentityDbContextSeed.SEED_DEMO_USERS_CONFIG_NAME);
            await AppIdentityDbContextSeed.SeedAsync(identityContext, userManager, roleManager, seedDemoUsers);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred seeding the DB.");
        }
    }

    private static bool HasTables(DbContext context)
    {
        var databaseCreator = context.GetService<IRelationalDatabaseCreator>();
        return databaseCreator.Exists() && databaseCreator.HasTables();
    }
}
