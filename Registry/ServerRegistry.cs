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
    private static TimeSpan ServiceTimeout = TimeSpan.FromMinutes(2);

    private ConcurrentDictionary<ServiceKey, HashSet<Service>> _registry = new();
    private ConcurrentDictionary<Location, Service> _locations = new();

    public ServerRegistry(
        ILogger<ServerRegistry> logger)
    {
    }

    public void Cancel(Location location)
    {
        if (_locations.Remove(location, out var service))
        {
            var set = _registry[service.Key].Remove(service);
        }
    }

    public Service? GetService(ServiceKey key)
    {
        if (_registry.TryGetValue(key, out var set))
        {
            var result =
                set
                .Where(e => (e.Time + ServiceTimeout) > DateTime.UtcNow)
                .OrderBy(e => e.Calls)
                .FirstOrDefault();

            // TODO increment the calls for offline services.
            
            if (result != null) result.Calls++;

            return result;
        }
        
        return null;
    }

    public void Register(Service service)
    {
        _locations[service.Location] = service;
        if (!_registry.TryGetValue(service.Key, out var set))
        {
            set = new HashSet<Service>();
            _registry[service.Key] = set;
        }
        set.Add(service);
    }

    public void Renew(Service service)
    {
        if (_registry.TryGetValue(service.Key, out var set))
        {
            if (set.TryGetValue(service, out var current))
            {
                if (current.Time < service.Time)
                {
                    current.Time = service.Time;
                }
            }
        }
        else
        {
            Register(service);
        }
    }

    public void ReplicateRemote(IReplicationContext replicationContext)
    {
        var services = _registry.SelectMany(e => e.Value);

        var remotes = _registry[(ServiceKey)"GLOBAL/ServiceRegistry"]
            .Where(e => e.Location != (Location)replicationContext.LocalUrl)
            .Select(e => e.Location);

        new ReplicationClient(replicationContext)
            .ReplicateRemote(
                remotes,
                services.ToArray());
    }
}