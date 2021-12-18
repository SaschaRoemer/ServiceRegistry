[ApiController]
[Route("[controller]")]
public class RegistryController : ControllerBase
{
    
    private readonly ConcurrentDictionary<ServiceKey, string> _Service = new();
    private readonly IReplicationContext _replicationContext;
    private readonly IServerRegistry _registry;

    public RegistryController(
        IReplicationContext replicationContext,
        IServerRegistry registry)
    {
        _replicationContext = replicationContext;
        _registry = registry;
    }

    [HttpPost(Name = "PostRegistry")]
    public void Replicate(string[] services)
    {
        System.Console.WriteLine("Replicate");

        var servicesInternal = services.Select(e => (Service)e);

        foreach(var e in services)
        {
            _registry.Renew((Service)e);
        }

        var client = new ReplicationClient(_replicationContext);
        _registry.ReplicateRemote(_replicationContext);
        // bool replicate;
        // lock(_replicationContext)
        // {
        //     replicate = _replicationContext.Replicate;
        //     if (_replicationContext.Replicate)
        //     {
        //         _replicationContext.Replicate = false;
        //     }
        // }
        // if (replicate)
        // {
        //     client.ReplicateRemote(_registry.Services);
        // }
    }
}
