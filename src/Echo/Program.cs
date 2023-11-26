using ServiceRegistry;

var serviceRegistryEndpoint = Environment.GetEnvironmentVariable("SERVICE_REGISTRY_ENDPOINT");
var serviceEnvironment = Environment.GetEnvironmentVariable("SERVICE_ENVIRONMENT");
var serviceEndpoint = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
var echoForward = Environment.GetEnvironmentVariable("ECHO_FORWARD");
var echoText = Environment.GetEnvironmentVariable("ECHO_TEXT");
var serviceLabel = Environment.GetEnvironmentVariable("SERVICE_LABEL");

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddHttpClient...

var app = builder.Build();
var logger = app.Logger;

var api = app.MapGroup("/Echo");
api.MapGet("/", () => Results.Ok("Echo"));
api.MapGet("/{text}", async (string text) =>
    {
        text = $"{echoText}{text}{echoText}";

        if (echoForward != null)
        {
            var http = new HttpClient();
            var response = await http.GetAsync($"{serviceRegistryEndpoint}/{serviceEnvironment}/{echoForward}");
            var url = await response.Content.ReadAsStringAsync();
            logger.LogInformation("[ECHO_FORWARD] {echoForward}, StatusCode: {responseStatusCode}, URL: {url}", echoForward, response.StatusCode, url);

            if (url != null)
            {
                text = (await (await http.GetAsync($"{url}/{text}")).Content.ReadAsStringAsync()).Trim('"');
            }
        }

        logger.LogInformation("[Echo] {text}", text);
        return Results.Ok(text);
    });

Timer? serviceTimer = null;
var http = new HttpClient();
var service = (Service)$"{serviceEnvironment}/{serviceLabel}@{serviceEndpoint}/Echo";
if (serviceRegistryEndpoint != null)
{
    http = new HttpClient();
    try
    {
        logger.LogInformation("[RegisterService] {service}", service);
        await http.PostAsync(serviceRegistryEndpoint, JsonContent.Create(service));

        serviceTimer = new Timer(c =>
        {
            logger.LogInformation("[RenewService] {service}", service);
            http.PutAsync(serviceRegistryEndpoint, JsonContent.Create(service));
        }, null, 30000, 30000);
    }
    catch(Exception ex)
    {
        logger.LogError(ex.ToString());
    }
}

app.Run();

serviceTimer?.Dispose();
http?.DeleteAsync($"serviceRegistryEndpoint/{System.Web.HttpUtility.UrlEncode(serviceEndpoint)}");
