using Fischbowl_Project.Components;
using Fischbowl_Project.Data.Services;
using Fischbowl_Project.Components.Account;
using Fischbowl_Project.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fischbowl_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });
    }
}

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add authentication state and related services
        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        // Configure authentication schemes
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddIdentityCookies();

        // Configure Entity Framework and the database connection string
        var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") ?? throw new InvalidOperationException("SQL connection string not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        // Configure Identity services
        services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Configure email sender (No-op implementation)
        services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        // Register BlobStorageService with the connection string from environment variables
        services.AddScoped(sp =>
        {
            // Get the Azure Blob Storage connection string from environment variables
            var blobConnectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING");
            // Ensure the connection string is not null or empty
            if (string.IsNullOrEmpty(blobConnectionString))
            {
                throw new InvalidOperationException("Azure Blob Storage connection string not found in environment variables.");
            }
            // Return a new instance of BlobStorageService with the connection string
            return new BlobStorageService(blobConnectionString);
        });

        // Register the PhotoService with the SQL connection string and BlobStorageService
        services.AddScoped<PhotoService>(sp =>
        {
            var sqlConnectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            var blobStorageService = sp.GetRequiredService<BlobStorageService>();
            return new PhotoService(sqlConnectionString, blobStorageService);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
