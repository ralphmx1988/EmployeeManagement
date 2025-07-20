using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.DTOs;

public record EmployeeDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string Position { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public decimal Salary { get; init; }
    public DateTime HireDate { get; init; }
    public bool IsActive { get; init; }
    public string FullName { get; init; } = string.Empty;
}

public record CreateEmployeeDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string Position { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public decimal Salary { get; init; }
    public DateTime HireDate { get; init; } = DateTime.Today;
}

public record UpdateEmployeeDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string Position { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public decimal Salary { get; init; }
    public DateTime HireDate { get; init; }
    public bool IsActive { get; init; }
}
