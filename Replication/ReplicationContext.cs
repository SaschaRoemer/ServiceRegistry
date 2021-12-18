namespace ServiceRegistry;

public interface IReplicationContext
{
    DateTime StartTime { get; }

    bool Replicate { get; set; }

    string LocalUrl { get; }

    string[] PeersUrl { get; }
}

public class ReplicationContext : IReplicationContext
{    
    public ReplicationContext(IConfiguration configuration)
    {
        var replication = configuration.GetValue<string>("Replication").Split(';');
        LocalUrl = replication[0];
        PeersUrl = replication.Skip(1).ToArray();
    }

    public DateTime StartTime { get; } = DateTime.UtcNow;

    public bool Replicate { get; set; }

    public string LocalUrl { get; }

    public string[] PeersUrl { get; }
}