
public record Service
{
    public Service(ServiceKey key, Location location)
    {
        Key = key;
        Location = location;
    }

    public ServiceKey Key { get; }
    public Location Location { get; }
    internal DateTime? Time { get; set; }
    // TODO
    internal bool IsRegistry => Key.Name == "ServiceRegistry";

    public static explicit operator Service(string value)
    {
        //ENVIRONMENT/NAME@LOCATION|TIME
        var partsA = value.Split('@');
        var key = (ServiceKey)partsA[0];

        var partsB = partsA[1].Split('|');
        var location = (Location)partsB[0];
        var time = DateTime.Parse(partsB[1]);

        return new Service(key, location)
        {
            Time = time
        };
    }

    public override string ToString()
    {
        return $"{Key}@{Location}|{Time:O}";
    }
}
