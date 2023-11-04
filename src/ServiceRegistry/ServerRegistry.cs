namespace ServiceRegistry;

public interface IServerRegistry
{
    Service? GetService(ServiceKey key);

    void Register(Service service);

    void Renew(Service service);
    
    void Cancel(Location location);
}

public class ServerRegistry : IServerRegistry
{
    private ILogger<ServerRegistry> _logger;
    private static TimeSpan ServiceTimeout = TimeSpan.FromMinutes(2);
    private ConcurrentDictionary<ServiceKey, HashSet<Service>> _registry = new();
    private ConcurrentDictionary<Location, Service> _locations = new();

    public ServerRegistry(
        ILogger<ServerRegistry> logger)
    {
        _logger = logger;
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
            
            if (result != null)
            {
                result.Calls++;
                _logger.LogService("GetService", "Service found", result);
                return result;
            }
        }

        _logger.LogService("GetService", "null", null);
        return null;
    }

    public void Register(Service service)
    {
        _locations[service.Location] = service;
        if (!_registry.TryGetValue(service.Key, out var set))
        {
            _logger.LogService("Register", "New hashset", service);
            service.Time = DateTime.UtcNow;
            set = new HashSet<Service>();
            _registry[service.Key] = set;
        }
        
        set.Add(service);
        _logger.LogService("Register", "Add service", service, set.Count);
    }

    public void Renew(Service service)
    {
        if (_registry.TryGetValue(service.Key, out var set))
        {
            if (set.TryGetValue(service, out var current))
            {
                var time = DateTime.UtcNow;
                if (current.Time < time)
                {
                    current.Time = time;
                }

                _logger.LogService("Renew", "-", current);
                return;
            }
            _logger.LogService("Renew", "Service not found", service);
        }
        else
        {
            Register(service);
        }
    }
}