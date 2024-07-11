using Flaminco.ImmutableLookups.Abstractions;
using Flaminco.ImmutableLookups.Implementations;
using Flaminco.Migration.Extensions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;

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


            builder.Services.AddScoped<IImmutableLookupQuery<WorkflowStatus, int>, ImmutableLookupQuery<LookupDbContext, WorkflowStatus, int>>();

            builder.Services.AddDbContext<LookupDbContext>(options =>
            {
                options.UseSqlServer("Server=localhost;Initial Catalog=LookupDbs;Persist Security Info=False;User ID=sa;Password=sa;MultipleActiveResultSets=False;TrustServerCertificate=true");
            });

            builder.Services.AddMigration<Program>(builder.Configuration);

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
