namespace ServiceRegistry;

public class ReplicationClient
{
    private readonly IReplicationContext _replicationContext;

    public ReplicationClient(
        IReplicationContext replicationContext)
    {
        _replicationContext = replicationContext;
        
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("User-Agent", "Service Registry Replication");
    }

    private readonly HttpClient _client = new HttpClient();

    public async void ReplicateLocal()
    {
        // TODO read from Service[] services parameter
        // TODO Environment.
        var services = new[]
        {
            new Service((ServiceKey)"GLOBAL/ServiceRegistry", (Location)_replicationContext.LocalUrl)
            {
                Time = DateTime.UtcNow
            }.ToString()
        }
        .Union(
            _replicationContext.PeersUrl.Select(e =>
                new Service((ServiceKey)"GLOBAL/ServiceRegistry", (Location)e)
                {
                    Time = _replicationContext.StartTime
                }.ToString())
        );

        _replicationContext.Replicate = true;
        StringContent content = await Replicate(
            _replicationContext.LocalUrl,
            services);
    }

    public async void ReplicateRemote(IEnumerable<Location> remotes, Service[] services)
    {
        if (remotes == null)
        {
            return;
        }

        var servicesAsString =
            services.Select(e => e.ToString());

        foreach(var remote in remotes)
        {
            await Replicate(
                remote.ToString(),
                servicesAsString);
        }
    }

    private async Task<StringContent> Replicate(string targetUrl, IEnumerable<string> services)
    {
        var content =
                    new StringContent(
                        JsonSerializer.Serialize(services),
                        Encoding.UTF8,
                        "application/json");

        var task = _client.PostAsync($"{targetUrl}/Registry", content);

        var message = await task;
        string resp = await message.Content.ReadAsStringAsync();
        Console.Write(message);
        return content;
    }
}