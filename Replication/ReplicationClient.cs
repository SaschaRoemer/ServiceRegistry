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
            new Service((ServiceKey)"ServiceRegistry", (Location)_replicationContext.LocalUrl)
            {
                Time = DateTime.UtcNow
            }.ToString()
        }
        .Union(
            _replicationContext.PeersUrl.Select(e =>
                new Service((ServiceKey)"ServiceRegistry", (Location) e)
                {
                    Time = _replicationContext.StartTime
                }.ToString())
        );

        using var content =
            new StringContent(
                JsonSerializer.Serialize(services),
                Encoding.UTF8,
                "application/json");

        _replicationContext.Replicate = true;

        var task = _client.PostAsync($"{_replicationContext.LocalUrl}/Registry", content);

        var message = await task;
        string resp = await message.Content.ReadAsStringAsync();
        Console.Write(message);
    }

    public async void ReplicateRemote(Service[] services)
    {
        
    }
}