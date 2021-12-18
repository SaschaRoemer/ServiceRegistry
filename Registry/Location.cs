namespace ServiceRegistry;

public record Location
{
    public Location(string scheme, string host, string path)
    {
        Scheme = scheme;
        Host = host;
        Path = path;
    }

    public string Scheme { get; set; }

    public string Host { get; set; }

    public string Path { get; set; }

    public static explicit operator Location(string value)
    {
        var uri = new Uri(value);
        
        return new Location(
            uri.Scheme,
            string.Concat(uri.Host, ":", uri.Port),
            uri.LocalPath);
    }

    public override string ToString()
    {
        return string.Concat(Scheme, "://", Host, Path);
    }
}