using System.Collections;

namespace ServiceRegistry;

internal class ServiceList : IEnumerable<Service>
{
    private static TimeSpan ServiceTimeout = TimeSpan.FromMinutes(2);
    private HashSet<Service> _set = [];
    private Queue<Service> _queue = new();

    internal Service? GetNext()
    {
        while(_queue.TryDequeue(out var service))
        {
            if (service.Time + ServiceTimeout < DateTime.UtcNow)
            {
                _set.Remove(service);
                continue;
            }
            _queue.Enqueue(service);
            return service;
        }
        return null;
    }

    internal bool Add(Service service)
    {
        if (_set.Add(service))
        {
            _queue.Enqueue(service);
            return true;
        }

        return false;
    }

    internal bool Remove(Service service)
    {
        if (_set.Remove(service))
        {
            _queue = new Queue<Service>(_queue.Where(s => s != service));
            return true;
        }
        return false;
    }

    internal bool TryGetValue(Service service, out Service current)
    {
        return _set.TryGetValue(service, out current);
    }

    public IEnumerator<Service> GetEnumerator() => _set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();
}
