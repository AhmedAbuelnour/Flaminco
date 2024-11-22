using Flaminco.ManualMapper.Extensions;
using Flaminco.MinimalMediatR.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication().AddBearerToken(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

builder.Services.AddModules<Program>();

builder.Services.AddMediatR(o =>
{
    o.RegisterServicesFromAssemblyContaining<Program>();
});

builder.Services.AddManualMapper<Program>();

builder.Services.AddEntityDtoAdaptors<Program>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddValidationExceptionHandler(options =>
{
    options.Title = "Test title";
});

builder.Services.AddProblemDetails();

var app = builder.Build();

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

