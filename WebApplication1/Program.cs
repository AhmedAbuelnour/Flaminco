using Flaminco.RediPolly.Extensions;
using Microsoft.Extensions.Caching.Hybrid;
using WebApplication1.BackgroundServices;
using WebApplication1.Controllers;

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


            builder.Services.AddScoped<Counter>();

            builder.Services.AddRediPolly<Program>("172.17.7.4:6379");

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "172.17.7.4:6379";
            });


            // HybridCache Support two level of caching L1 and L2 at the same time.
            // Once AddStackExchangeRedisCache is configured it will use L2 caching along side L1.
            // By default it will use L1 caching
            builder.Services.AddHybridCache(opt =>
            {
                // We can set the default entry options for all items.
                // for most cases we should make the L1 expiration less than L2 expiration
                opt.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(30), // only 30 mins, the default is 5 mins
                    LocalCacheExpiration = TimeSpan.FromMinutes(2), // only 2 mins,, the default is 1 mins
                    Flags = HybridCacheEntryFlags.DisableLocalCache, // means don't use L1 caching, don't store anything there, unless the user set it explicitly.
                };

                //opt.MaximumPayloadBytes The default value is 1 MiB and the max is 2GB, 

                // opt.MaximumKeyLength The default value is 1024 characters
            });


            builder.Services.AddHostedService<ChannelListenerExample>();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
