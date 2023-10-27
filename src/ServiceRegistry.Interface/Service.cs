namespace ServiceRegistry;

public class Service : IEquatable<Service>
{
    public Service(ServiceKey key, Location location)
    {
        Key = key;
        Location = location;
    }

    public ServiceKey Key { get; }
    public Location Location { get; }
    public DateTime? Time { get; set; }
    public long Calls { get; set; }

    public static explicit operator Service(string value)
    {
        //ENVIRONMENT/NAME@LOCATION|TIME
        var partsA = value.Split('@');
        var key = (ServiceKey)partsA[0];

        var partsB = partsA[1].Split('|');
        var location = (Location)partsB[0];
        var time = partsB.Length > 1 ? DateTime.Parse(partsB[1]) : (DateTime?)null;

        return new Service(key, location)
        {
            Time = time
        };
    }

    public override string ToString()
    {
        return $"{Key}@{Location}|{Time:O}";
    }

    public bool Equals(Service? other)
    {
        if (other == null) return false;

        return other.Key == Key && other.Location == Location;
    }

    public override bool Equals(object? obj)
    {
        return obj is Service service
        ? Equals(service)
        : false;

    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Key.GetHashCode();
            hash = hash * 23 + Location.GetHashCode();
            return hash;
        }
    }
}
