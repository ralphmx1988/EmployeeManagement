using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Position { get; set; } = string.Empty;
    
    [Required]
    [StringLength(150)]
    public string Department { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal Salary { get; set; }
    
    public DateTime HireDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Computed property
    public string FullName => $"{FirstName} {LastName}";
    
    // Business method
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateSalary(decimal newSalary)
    {
        if (newSalary < 0)
            throw new ArgumentException("Salary cannot be negative", nameof(newSalary));
            
        Salary = newSalary;
        UpdatedAt = DateTime.UtcNow;
    }
}
