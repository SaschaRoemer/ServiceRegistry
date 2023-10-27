using ServiceRegistry;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var api = app.MapGroup("/Echo");
api.MapGet("/", () => Results.Ok("Echo"));
api.MapGet("/{text}", (string text) => Results.Ok(text));

var registryEndpoint = Environment.GetEnvironmentVariable("SERVICE_REGISTRY_ENDPOINT");
Timer? serviceTimer = null;
if (registryEndpoint != null)
{
    serviceTimer = new Timer(c =>
    {
        var http = new HttpClient();
        var service = (Service)"test/Echo@http://localhost:7089/Echo";
        http.PostAsync(registryEndpoint, JsonContent.Create(service));
    }, null, 0, 30000);
}

app.Run();

serviceTimer?.Dispose();
