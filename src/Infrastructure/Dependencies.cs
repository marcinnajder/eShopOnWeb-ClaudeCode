using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NimblePros.Metronome;

namespace Microsoft.eShopWeb.Infrastructure;

public static class Dependencies
{
    public const string USE_SQLITE_DATABASE_CONFIG_NAME = "UseSqliteDatabase";

    public static void ConfigureLocalDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
    {
        bool useOnlyInMemoryDatabase = false;
        if (configuration["UseOnlyInMemoryDatabase"] != null)
        {
            useOnlyInMemoryDatabase = bool.Parse(configuration["UseOnlyInMemoryDatabase"]!);
        }

        // SQLite takes precedence over in-memory, so it can be switched on without touching the in-memory setting
        if (configuration.GetValue<bool>(USE_SQLITE_DATABASE_CONFIG_NAME))
        {
            string catalogConnectionString = GetSqliteConnectionString(configuration, "SqliteCatalogConnection");
            services.AddDbContext<CatalogContext>((provider, options) =>
                options.UseSqlite(catalogConnectionString)
                    .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>()));

            string identityConnectionString = GetSqliteConnectionString(configuration, "SqliteIdentityConnection");
            services.AddDbContext<AppIdentityDbContext>((provider, options) =>
                options.UseSqlite(identityConnectionString)
                    .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>()));
        }
        else if (useOnlyInMemoryDatabase)
        {
            services.AddDbContext<CatalogContext>((provider, options) =>
               options.UseInMemoryDatabase("Catalog"));

            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseInMemoryDatabase("Identity"));
        }
        else
        {
            // use real database
            // Requires LocalDB which can be installed with SQL Server Express 2016
            // https://www.microsoft.com/en-us/download/details.aspx?id=54284
            string catalogConnectionString = configuration.GetConnectionString("CatalogConnection")!;
            services.AddDbContext<CatalogContext>((provider, options) =>
            {
                // why doesn't this extension method work?
                // see: https://github.com/NimblePros/Metronome/blob/main/src/NimblePros.Metronome/ServiceRegistrationExtensions.cs#L24
                //options.UseSqlServer(catalogConnectionString).AddMetronomeDbTracking(provider); ;

                // but this works
                options.UseSqlServer(catalogConnectionString)
                    .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });

            // Add Identity DbContext
            string identityConnectionString = configuration.GetConnectionString("IdentityConnection")!;
            services.AddDbContext<AppIdentityDbContext>((provider, options) =>
            {
                options.UseSqlServer(identityConnectionString);
                // not sure why AddMetronomeDbTracking extension method doesn't work here
                options.AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });
        }
    }

    /// <summary>
    /// Resolves a relative SQLite file path against the content root (the project folder), so Web and PublicApi
    /// share the same database file no matter which directory they are started from.
    /// </summary>
    private static string GetSqliteConnectionString(IConfiguration configuration, string name)
    {
        var builder = new SqliteConnectionStringBuilder(configuration.GetConnectionString(name));
        string contentRoot = configuration[HostDefaults.ContentRootKey] ?? Directory.GetCurrentDirectory();
        builder.DataSource = Path.GetFullPath(builder.DataSource, contentRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(builder.DataSource)!);
        return builder.ToString();
    }

    public static void AddJwtSettings(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.CONFIG_NAME))
            .PostConfigure(settings =>
            {
                // Lets the sample run locally with no setup. JwtSettingsValidator rejects this key outside local development.
                if (string.IsNullOrWhiteSpace(settings.SigningKey) && environment.IsLocalDevelopment())
                {
                    settings.SigningKey = JwtSettings.DEVELOPMENT_SIGNING_KEY;
                }
            })
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();
    }
}
