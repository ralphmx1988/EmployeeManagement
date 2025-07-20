namespace EmployeeManagement.Domain.ValueObjects;

public record Address(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
)
{
    public static Address Empty => new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    
    public override string ToString()
    {
        return $"{Street}, {City}, {State} {PostalCode}, {Country}";
    }
}
