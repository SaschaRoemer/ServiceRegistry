
[ApiController]
[Route("[controller]")]
public class ServiceController : ControllerBase
{
    private readonly IServerRegistry _registry;

    public ServiceController(
        IServerRegistry registry,
        IConfiguration configuration,
        ILogger<ServiceController> logger)
    {
        _registry = registry;
    }

    [HttpGet("{key}", Name = "GetService")]

    public Service? GetService(string key)
    {
        return _registry.GetService((ServiceKey)key);
    }

    /// <summary>Registers a new service</summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /Service
    ///     {
    ///         "key": {
    ///             "environment": "staging",
    ///             "name": "Echo"
    ///         },
    ///         "location": {
    ///             "scheme": "http",
    ///             "host": "localhost",
    ///             "path": "/Echo"
    ///         }
    ///     }
    /// </remarks>
    [HttpPost(Name = "PostService")]
    public void Register(Service service)
    {
        _registry.Register(service);
    }

    [HttpPut(Name = "PutService")]
    public void Renew(Service service)
    {
        _registry.Renew(service);

    }

    [HttpDelete("{location}", Name = "DeleteService")]
    public void Cancel(Location location)
    {
        _registry.Cancel(location);
    }
}
