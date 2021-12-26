global using Microsoft.AspNetCore.Mvc;
global using System.Collections.Concurrent;
global using Microsoft.Extensions.Configuration;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Linq;
global using System.Text;
global using System.Text.Json;

global using ServiceRegistry;

using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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

var replicationContext = new ReplicationContext(builder.Configuration);
builder.Services.AddSingleton<IReplicationContext>(replicationContext);
#endregion

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

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

var replication = builder.Configuration.GetValue<string>("Replication").Split(';');

var timer = new System.Threading.Timer(
    (e) =>
    {
        new ReplicationClient(replicationContext).ReplicateLocal();
    },
    null,
    TimeSpan.Zero,
    TimeSpan.FromSeconds(1));

app.Run(replication[0]);

timer.Dispose();