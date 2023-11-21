using Flaminco.ManualMapper.Extensions;
using Flaminco.Pipeline.Extensions;
using FlamincoWebApi.Controllers;
using FlamincoWebApi.Entities;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FlamincoWebApi
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

            builder.Services.AddHttpLoggingInterceptor<HttpLoggerInterceptor>();

            builder.Services.AddExceptionHandler<DefaultExceptionHandler>();


            builder.Services.AddProblemDetails();

            //builder.Services.TryAddSingleton(ObjectPool.Create<HttpLoggingInterceptorContext>());

            //builder.Services.TryAddSingleton(TimeProvider.System);

            builder.Services.AddHttpLogging(logger =>
            {
                logger.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.Response;
            });

            builder.Services.AddMediatR(e => e.RegisterServicesFromAssembly(typeof(Program).Assembly));

            builder.Services.AddManualMapper<Program>();

            builder.Services.AddDbContext<UserDbContext>(options =>
            {
                options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=UsersDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
            });

            string serviceName = "TEST Service";

            builder.Services.AddValidation<Program>();
            builder.Services.AddPipelines<Program>();

            builder.Services.AddOpenTelemetry().WithTracing(config =>
            {
                config.AddConsoleExporter();
                config.AddSource(serviceName).SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: "1.0.0"));

                config.AddAspNetCoreInstrumentation(o =>
                {
                    o.Filter = (httpContext) =>
                    {
                        if (httpContext.Request.Path.Value?.Contains("swagger") ?? false)
                        {
                            return false;
                        }
                        else if (httpContext.Request.Path.Value?.Contains("_framework") ?? false)
                        {
                            return false;
                        }
                        // only collect telemetry about HTTP GET requests
                        return true;
                    };

                    o.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("requestProtocol", httpRequest.Protocol);
                    };

                    o.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("responseLength", httpResponse.ContentLength);
                    };

                    o.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("exceptionType", exception.GetType().ToString());
                    };
                });

                //config.AddSqlClientInstrumentation();

                config.AddEntityFrameworkCoreInstrumentation(o =>
                {
                    o.SetDbStatementForText = true;
                });

                config.AddHttpClientInstrumentation();

            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

                    if (dbContext is not null)
                    {
                        if (dbContext.Database.IsSqlServer())
                        {
                            dbContext.Database.EnsureCreated();
                        }
                    }
                }
            }



            app.UseExceptionHandler();

            app.UseHttpLogging();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.AddEndPoints();

            app.MapControllers();

            app.Run();








        }


    }
}