using Flaminco.AdvancedHybridCache.Extensions;
using Flaminco.DualMapper.Extensions;
using Flaminco.ManualMapper.Extensions;
using Flaminco.MinimalMediatR.Extensions;
using Flaminco.StateMachine;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Hybrid;
using WebApplication1.Validations;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStateMachine<Program>();

builder.Services.AddAuthentication().AddBearerToken(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

builder.Services.AddManualMapper<Program>();

builder.Services.AddDualMappers<Program>();

builder.Services.AddStackExchangeRedisCache(a =>
{
    a.Configuration = "172.17.7.5:6379";
});

builder.Services.AddAdvancedHybridCache(a =>
{
    a.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(1),
        Flags = HybridCacheEntryFlags.DisableLocalCache
    };
});


builder.Services.AddModules<Program>();

builder.Services.AddMediatR(o =>
{
    o.RegisterServicesFromAssemblyContaining<Program>();
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddValidationExceptionHandler(options =>
{
    options.Title = "Test title";
});

builder.Services.AddBusinessExceptionHandler(a =>
{
    a.Type = "https://dga.error.codes/index#";
});

builder.Services.AddProblemDetails();

builder.Services.AddInMemoryChannel();

builder.Services.AddScoped<ITest, Test>();

var app = builder.Build();

app.MapGet("/test", async (ITest test) =>
{
    await test.Test();
    return Results.Ok();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();

app.MapModules();

app.UseExceptionHandler();

app.Run();

