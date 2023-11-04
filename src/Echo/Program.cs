using ServiceRegistry;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var api = app.MapGroup("/Echo");
api.MapGet("/", () => Results.Ok("Echo"));
api.MapGet("/{text}", (string text) => Results.Ok(text));

var registryEndpoint = Environment.GetEnvironmentVariable("SERVICE_REGISTRY_ENDPOINT");
var environment = Environment.GetEnvironmentVariable("SERVICE_ENVIRONMENT");
var endpoint = Environment.GetEnvironmentVariable("SERVICE_ENDPOINT");
Timer? serviceTimer = null;
if (registryEndpoint != null)
{
    var http = new HttpClient();
    var service = (Service)$"{environment}/Echo@{endpoint}/Echo";
    await http.PostAsync(registryEndpoint, JsonContent.Create(service));
    serviceTimer = new Timer(c =>
    {
        http.PutAsync(registryEndpoint, JsonContent.Create(service));
    }, null, 30000, 30000);
}

app.Run();

serviceTimer?.Dispose();
