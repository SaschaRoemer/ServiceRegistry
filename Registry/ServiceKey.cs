
public record ServiceKey
{
    public ServiceKey(string environment, string name)
    {
        Environment = environment;
        Name = name;
    }

    public string Environment { get; set; }

    public string Name { get; set; }

    public static explicit operator ServiceKey(string value)
    {
        var parts = value.Split("/");
        return parts.Length > 1
        ? new ServiceKey(parts[0], parts[1])
        : new ServiceKey("GLOBAL", parts[0]);
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Environment)
        ? Name
        : string.Concat(Environment, "/", Name);
    }
}
