using EmployeeManagement.Domain.Entities;
using Xunit;

namespace EmployeeManagement.Tests.Domain;

public class EmployeeTests
{
    [Fact]
    public void Employee_Constructor_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var employee = new Employee();

        // Assert
        Assert.True(employee.IsActive);
        Assert.True(employee.CreatedAt <= DateTime.UtcNow);
        Assert.Equal(string.Empty, employee.FirstName);
        Assert.Equal(string.Empty, employee.LastName);
        Assert.Equal(string.Empty, employee.Email);
        Assert.Equal(string.Empty, employee.Position);
        Assert.Equal(string.Empty, employee.Department);
    }

    [Fact]
    public void Employee_FullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var fullName = employee.FullName;

        // Assert
        Assert.Equal("John Doe", fullName);
    }

    [Fact]
    public void Employee_Deactivate_ShouldSetIsActiveToFalseAndUpdateTimestamp()
    {
        // Arrange
        var employee = new Employee { IsActive = true };
        var beforeDeactivation = DateTime.UtcNow;

        // Act
        employee.Deactivate();

        // Assert
        Assert.False(employee.IsActive);
        Assert.NotNull(employee.UpdatedAt);
        Assert.True(employee.UpdatedAt >= beforeDeactivation);
    }

    [Fact]
    public void Employee_Activate_ShouldSetIsActiveToTrueAndUpdateTimestamp()
    {
        // Arrange
        var employee = new Employee { IsActive = false };
        var beforeActivation = DateTime.UtcNow;

        // Act
        employee.Activate();

        // Assert
        Assert.True(employee.IsActive);
        Assert.NotNull(employee.UpdatedAt);
        Assert.True(employee.UpdatedAt >= beforeActivation);
    }

    [Fact]
    public void Employee_UpdateSalary_WithValidSalary_ShouldUpdateSalaryAndTimestamp()
    {
        // Arrange
        var employee = new Employee { Salary = 50000 };
        var newSalary = 60000m;
        var beforeUpdate = DateTime.UtcNow;

        // Act
        employee.UpdateSalary(newSalary);

        // Assert
        Assert.Equal(newSalary, employee.Salary);
        Assert.NotNull(employee.UpdatedAt);
        Assert.True(employee.UpdatedAt >= beforeUpdate);
    }

    [Fact]
    public void Employee_UpdateSalary_WithNegativeSalary_ShouldThrowArgumentException()
    {
        // Arrange
        var employee = new Employee();
        var negativeSalary = -1000m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => employee.UpdateSalary(negativeSalary));
        Assert.Contains("Salary cannot be negative", exception.Message);
        Assert.Equal("newSalary", exception.ParamName);
    }
}
