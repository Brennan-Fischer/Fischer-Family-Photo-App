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

namespace Fischbowl_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Add authentication state and related services
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            // Configure authentication schemes
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
                .AddIdentityCookies();

            // Configure Entity Framework and the database connection string
            var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") ?? throw new InvalidOperationException("SQL connection string not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Configure Identity services
            builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            // Configure email sender (No-op implementation)
            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            // Register BlobStorageService with the connection string from environment variables
            builder.Services.AddScoped(sp =>
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
            builder.Services.AddScoped<PhotoService>(sp =>
            {
                var sqlConnectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
                var blobStorageService = sp.GetRequiredService<BlobStorageService>();
                return new PhotoService(sqlConnectionString, blobStorageService);
            });

            // Build the application
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint(); // Use migrations endpoint in development environment
            }
            else
            {
                app.UseExceptionHandler("/Error"); // Use exception handler in production environment
                app.UseHsts(); // Use HTTP Strict Transport Security (HSTS) in production
            }

            app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS

            app.UseStaticFiles(); // Serve static files

            app.UseAntiforgery(); // Use antiforgery token

            // Map Razor components to the app
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            // Run the application
            app.Run();
        }
    }
}
