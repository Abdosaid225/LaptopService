namespace LaptopService.Models;

public class Customer
{
    public string Name { get; }
    public string Phone { get; }

    public Customer(string name, string phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Customer phone is required.", nameof(phone));

        Name = name.Trim();
        Phone = phone.Trim();
    }
}
