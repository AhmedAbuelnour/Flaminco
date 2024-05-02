using Flaminco.Cache.Extensions;
using Flaminco.RedisChannels.Options;
using Flaminco.RedisChannels.Subscribers;
using Flaminco.StateMachine;
using StackExchange.Redis;
using WebApplication1.BackgroundServices;
using WebApplication1.Controllers;
using WebApplication1.Publishers;

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

            builder.Services.AddStateMachine<Program>();


            builder.Services.AddScoped<Counter>();

            builder.Services.AddScoped<ChannelPublisher, TESTRedisPublisher>();

            builder.Services.AddCache(builder.Configuration);

            builder.Services.Configure<RedisChannelConfiguration>(opt =>
            {
                opt.ConnectionMultiplexer = ConnectionMultiplexer.Connect("172.17.7.4:6379");
            });

            builder.Services.AddHostedService<ChannelLisnterBackgroundService>();

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
