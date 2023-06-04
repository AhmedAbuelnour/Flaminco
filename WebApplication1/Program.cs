using Flaminco.ManualMapper.Extensions;
using Flaminco.Pipeline.Extensions;
using Flaminco.StateMachine.Extensions;
using WebApplication1.Pipelines;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStateMachine<Program>();

builder.Services.AddManualMapper<Program>();

builder.Services.AddPipelines<IPipelinesScanner>();


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
