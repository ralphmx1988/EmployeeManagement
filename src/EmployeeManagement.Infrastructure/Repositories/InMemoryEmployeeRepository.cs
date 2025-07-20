using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Interfaces;

namespace EmployeeManagement.Infrastructure.Repositories;

public class InMemoryEmployeeRepository : IEmployeeRepository
{
    private readonly List<Employee> _employees;
    private int _nextId = 1;

    public InMemoryEmployeeRepository()
    {
        _employees = GenerateMockData();
    }

    public Task<IEnumerable<Employee>> GetAllAsync()
    {
        return Task.FromResult(_employees.AsEnumerable());
    }

    public Task<Employee?> GetByIdAsync(int id)
    {
        var employee = _employees.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(employee);
    }

    public Task<Employee> AddAsync(Employee employee)
    {
        employee.Id = _nextId++;
        employee.CreatedAt = DateTime.UtcNow;
        _employees.Add(employee);
        return Task.FromResult(employee);
    }

    public Task<Employee> UpdateAsync(Employee employee)
    {
        var existingEmployee = _employees.FirstOrDefault(e => e.Id == employee.Id);
        if (existingEmployee != null)
        {
            var index = _employees.IndexOf(existingEmployee);
            employee.UpdatedAt = DateTime.UtcNow;
            _employees[index] = employee;
        }
        return Task.FromResult(employee);
    }

    public Task DeleteAsync(int id)
    {
        var employee = _employees.FirstOrDefault(e => e.Id == id);
        if (employee != null)
        {
            _employees.Remove(employee);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
    {
        var employees = _employees.Where(e => e.Department.Equals(department, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(employees);
    }

    public Task<bool> ExistsAsync(int id)
    {
        var exists = _employees.Any(e => e.Id == id);
        return Task.FromResult(exists);
    }

    private List<Employee> GenerateMockData()
    {
        var random = new Random();
        var departments = new[] { "IT", "HR", "Finance", "Marketing", "Operations", "Sales" };
        var positions = new Dictionary<string, string[]>
        {
            { "IT", new[] { "Software Engineer", "DevOps Engineer", "QA Engineer", "Tech Lead", "Architect" } },
            { "HR", new[] { "HR Manager", "Recruiter", "HR Business Partner", "Training Coordinator" } },
            { "Finance", new[] { "Financial Analyst", "Accountant", "Finance Manager", "Controller" } },
            { "Marketing", new[] { "Marketing Manager", "Digital Marketer", "Content Creator", "Brand Manager" } },
            { "Operations", new[] { "Operations Manager", "Process Analyst", "Project Manager", "Coordinator" } },
            { "Sales", new[] { "Sales Representative", "Account Manager", "Sales Manager", "Business Developer" } }
        };

        var firstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Lisa", "James", "Maria", "William", "Jennifer", "Richard", "Patricia", "Thomas", "Linda" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas" };

        var employees = new List<Employee>();

        for (int i = 1; i <= 50; i++)
        {
            var department = departments[random.Next(departments.Length)];
            var positionList = positions[department];
            var position = positionList[random.Next(positionList.Length)];
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];

            var employee = new Employee
            {
                Id = i,
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@company.com",
                PhoneNumber = GeneratePhoneNumber(random),
                Position = position,
                Department = department,
                Salary = GenerateSalary(random, position),
                HireDate = GenerateHireDate(random),
                IsActive = random.Next(100) > 5, // 95% active
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365))
            };

            employees.Add(employee);
        }

        _nextId = employees.Count + 1;
        return employees;
    }

    private static string GeneratePhoneNumber(Random random)
    {
        return $"({random.Next(200, 999)}) {random.Next(200, 999)}-{random.Next(1000, 9999)}";
    }

    private static decimal GenerateSalary(Random random, string position)
    {
        var baseSalary = position.ToLower() switch
        {
            var p when p.Contains("engineer") => random.Next(70000, 120000),
            var p when p.Contains("manager") => random.Next(80000, 140000),
            var p when p.Contains("lead") || p.Contains("architect") => random.Next(100000, 160000),
            var p when p.Contains("analyst") => random.Next(55000, 85000),
            var p when p.Contains("coordinator") => random.Next(45000, 65000),
            var p when p.Contains("representative") => random.Next(40000, 70000),
            _ => random.Next(50000, 90000)
        };

        return baseSalary + (random.Next(-5000, 10000));
    }

    private static DateTime GenerateHireDate(Random random)
    {
        var startDate = DateTime.Today.AddYears(-10);
        var range = (DateTime.Today - startDate).Days;
        return startDate.AddDays(random.Next(range));
    }
}
