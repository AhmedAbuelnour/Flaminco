using Flaminco.Cache.Extensions;
using Flaminco.RedisChannels.Extensions;
using Flaminco.StateMachine;
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

            builder.Services.AddStateMachine<Program>();


            builder.Services.AddScoped<Counter>();

            builder.Services.AddRedisChannels<Program>("172.17.7.4:6379");

            builder.Services.AddCache(builder.Configuration);

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
