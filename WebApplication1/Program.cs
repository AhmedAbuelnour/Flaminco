using Flaminco.AzureBus.AMQP.Extensions;
using Flaminco.RazorInk.Extensions;
using Microsoft.EntityFrameworkCore;

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

            builder.Services.AddRazorInk();

            //builder.Services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = "127.0.0.1:6379";
            //});

            //AddHybridCache configures a two-level (L1 and L2)
            //caching mechanism where both in-memory (L1) and distributed (L2) caches are used. By default, if only AddStackExchangeRedisCache is configured, the application will utilize L2 caching alongside L1 caching.

            // builder.Services.AddHostedService<GarnetServerService>();

            //builder.Services.AddHybridCache(opt =>
            //{
            //    // We can set the default entry options for all items.
            //    // for most cases we should make the L1 expiration less than L2 expiration
            //    opt.DefaultEntryOptions = new HybridCacheEntryOptions
            //    {
            //        Expiration = TimeSpan.FromMinutes(30), // only 30 mins, the default is 5 mins (L2) - 100ms
            //        LocalCacheExpiration = TimeSpan.FromMinutes(2),// only 2 mins, the default is 1 mins - 20ms
            //        Flags = HybridCacheEntryFlags.None, // means don't use L1 caching, don't store anything there, unless the user set it explicitly.
            //    };

            //    // opt.MaximumPayloadBytes The default value is 1 MiB and the max is 2GB, 
            //    // opt.MaximumKeyLength The default value is 1024 characters
            //});

            //builder.Services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = "127.0.0.1:3278";
            //});



            //builder.Services.AddMigration<Program>(builder.Configuration);
            // AMQP 0.9 

            // AMQP 1.0


            //builder.Services.AddAMQPClient<Program>(options =>
            //{
            //    options.Host = "Endpoint=sb://sb-dev-app.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=onouja6KzYf5AwJWQ/GASEPhEKBIo3lqw+ASbKKsNuc=";
            //});   

            builder.Services.AddAMQPClient<Program>(options =>
            {
                options.Host = "Endpoint=sb://sb-dev-app.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=onouja6KzYf5AwJWQ/GASEPhEKBIo3lqw+ASbKKsNuc=";
            });

            // Register the sender



            // builder.Services.AddAMQPService<HelloConsumer, string>();
            //builder.Services.AddAMQPService<HelloConsumer2, string>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer("Server=localhost;Initial Catalog=LookupDbs;Persist Security Info=False;User ID=sa;Password=sa;MultipleActiveResultSets=False;TrustServerCertificate=true");
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                using var scope = app.Services.CreateScope();

                ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                //  bool xx = context.Database.EnsureCreated();
            }

            // app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
