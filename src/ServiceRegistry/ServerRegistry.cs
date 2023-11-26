namespace ServiceRegistry;

public interface IServerRegistry
{
    Service? GetService(ServiceKey key);

    void Register(Service service);

    void Renew(Service service);
    
    void Cancel(Location location);
}

public class ServerRegistry : IServerRegistry, IDisposable
{
    private ILogger<ServerRegistry> _logger;
    private ConcurrentDictionary<ServiceKey, ServiceList>? _registry = new();
    private ConcurrentDictionary<Location, Service>? _locations = new();
    private bool disposedValue;
    private Timer? _logTimer;

    public ServerRegistry(ILogger<ServerRegistry> logger)
    {
        _logger = logger;

        _logTimer = new Timer(c => LogRegistry(), null, 5000, 10000);
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
            var result = set.GetNext();
            
            if (result != null)
            {
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
            set = new();
            _registry[service.Key] = set;
        }
        
        set.Add(service);
        _logger.LogService("Register", "Add service", service);
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

    private void LogRegistry()
    {
        if (_registry != null)
        {
            _logger.LogInformation($"[Registry] {string.Join("; ", _registry.Select(e => $"{e.Key} {string.Join(", ", e.Value.Select(v => $"{v.ToString()}"))}"))}");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _logTimer?.Dispose();
            }

            _registry = null;
            _locations = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}