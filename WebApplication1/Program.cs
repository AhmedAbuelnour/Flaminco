using Flaminco.Migration.Extensions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Identity.Web;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "127.0.0.1:6379";
            });

            //AddHybridCache configures a two-level (L1 and L2)
            //caching mechanism where both in-memory (L1) and distributed (L2) caches are used. By default, if only AddStackExchangeRedisCache is configured, the application will utilize L2 caching alongside L1 caching.

            builder.Services.AddHybridCache(opt =>
            {
                // We can set the default entry options for all items.
                // for most cases we should make the L1 expiration less than L2 expiration
                opt.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(30), // only 30 mins, the default is 5 mins (L2) - 100ms
                    LocalCacheExpiration = TimeSpan.FromMinutes(2),// only 2 mins, the default is 1 mins - 20ms
                    Flags = HybridCacheEntryFlags.None, // means don't use L1 caching, don't store anything there, unless the user set it explicitly.
                };

                // opt.MaximumPayloadBytes The default value is 1 MiB and the max is 2GB, 
                // opt.MaximumKeyLength The default value is 1024 characters
            });


            builder.Services.AddMigration<Program>(builder.Configuration);


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer("Server=localhost;Initial Catalog=LookupDbs;Persist Security Info=False;User ID=sa;Password=sa;MultipleActiveResultSets=False;TrustServerCertificate=true");
            });

            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
      .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                using var scope = app.Services.CreateScope();

                ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                bool xx = context.Database.EnsureCreated();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            // Seed the OpenIddict client data
            OpenIddictSeedData.SeedAsync(app.Services).Wait();

            app.Run();
        }
    }
}
