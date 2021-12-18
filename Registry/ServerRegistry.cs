namespace ServiceRegistry;

public interface IServerRegistry
{
    Service? GetService(ServiceKey key);

    void Register(Service service);

    void Renew(Service service);
    
    void Cancel(Location location);

    void ReplicateRemote(IReplicationContext replicationContext);
}

public class ServerRegistry : IServerRegistry
{
    private const string ServiceRegistryName = "ServiceRegistry";
    // TODO Services as HashSet<Service>
    private ConcurrentDictionary<ServiceKey, Service> _registry = new();
    private ConcurrentDictionary<Location, Service> _locations = new();

    public ServerRegistry(
        ILogger<ServerRegistry> logger)
    {
    }

    public void Cancel(Location location)
    {
        if (_locations.Remove(location, out var service))
        {
            _registry.Remove(service.Key, out service);
        }
    }

    public Service? GetService(ServiceKey key)
    {
        return
        _registry.TryGetValue(key, out var value)
        ? value
        : null;
    }

    public void Register(Service service)
    {
        service.Time = DateTime.UtcNow;
        _registry[service.Key] = service;
        _locations[service.Location] = service;
    }

    public void Renew(Service service)
    {
        // TODO Update time if service exists.
        Register(service);
    }

    public void ReplicateRemote(IReplicationContext replicationContext)
    {
        throw new NotImplementedException();
    }
}