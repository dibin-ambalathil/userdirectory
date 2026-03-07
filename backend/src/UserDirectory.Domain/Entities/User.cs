namespace UserDirectory.Domain.Entities;

public class User
{
    private User()
    {
        // Required by EF Core.
    }

    public User(
        Guid id,
        string name,
        int age,
        string city,
        string state,
        string pincode,
        DateTime? createdAt = null)
    {
        Id = id;
        CreatedAt = createdAt ?? DateTime.UtcNow;
        UpdateDetails(name, age, city, state, pincode);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int Age { get; private set; }

    public string City { get; private set; } = string.Empty;

    public string State { get; private set; } = string.Empty;

    public string Pincode { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public void UpdateDetails(string name, int age, string city, string state, string pincode)
    {
        Name = name.Trim();
        Age = age;
        City = city.Trim();
        State = state.Trim();
        Pincode = pincode.Trim();
    }
}
