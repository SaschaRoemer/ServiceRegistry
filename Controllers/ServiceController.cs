
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

    /// <summary>Request a service by key.</summary>
    /// <param name="environment">The environment part of the service key ENVIRONMENT/NAME.</param>
    /// <param name="name">The name part of the service key ENVIRONMENT/NAME.</param>
    /// <returns>
    /// If a service is online under the given key it is returned. If there are multiple services 
    /// online one is selected using round-robin algorithm.
    /// </returns>
    /// <response code="200" cref="Service">OK</response>
    /// <response code="400" cref="Service">
    /// <p>Name must not be null or empty.</p>
    /// <p>Name must not contain / separator.</p>
    /// <p>Environment must not contain / separator.</p>
    /// </response>
    /// <response code="404">Service not found</response>
    [HttpGet("{environment}/{name}", Name = "GetService")]
    public ActionResult<Service?> GetService(string environment, string name)
    {
        if (name == null) return BadRequest("Name must not be null or empty.");
        if (name.Contains("/")) return BadRequest("Name must not contain / separator.");
        if (environment?.Contains("/") == true) return BadRequest("Environment must not contain / separator.");
        
        var service = _registry.GetService((ServiceKey)$"{environment}/{name}");

        if (service == null) return NotFound();

        return Ok(service);
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

    /// <summary>
    /// Renews the service registration to indicate that the service is online.
    /// Registered services must renew every minute. If a service fails to
    /// renew for two minutes it is considred offline.
    /// Services that are offline will not be returned by <see cref="GetService"/>.</summary>
    [HttpPut(Name = "PutService")]
    public void Renew(Service service)
    {
        _registry.Renew(service);

    }

    /// <summary>Set a service offline.</summary>
    [HttpDelete("{location}", Name = "DeleteService")]
    public void Cancel(Location location)
    {
        _registry.Cancel(location);
    }
}
