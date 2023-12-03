
using WebApplication2.Controllers;

namespace WebApplication2
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


            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(typeof(MaskedAttributeFilter));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapGet("/Masked", () =>
            {
                return Results.Ok(new MyModel
                {
                    SensitiveData = "123456789"
                });
            }).AddMaskFilter();

            app.MapControllers();

            app.Run();
        }
    }
}
