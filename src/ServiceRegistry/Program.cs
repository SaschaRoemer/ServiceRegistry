global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Configuration;
global using System.Collections.Concurrent;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Linq;
global using System.Text;
global using System.Text.Json;

global using ServiceRegistry;

using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    SwaggerGenOptionsExtensions
        .IncludeXmlComments(
            options,
            Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

#region Services
builder.Services.AddSingleton<IServerRegistry, ServerRegistry>();
#endregion

var app = builder.Build();
var logger = app.Logger;

foreach(var e in Environment.GetEnvironmentVariables()) { logger.LogInformation("{e}", e); }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Starting Swagger.");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();