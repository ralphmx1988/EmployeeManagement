using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Interfaces;

namespace EmployeeManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        return employee != null ? MapToDto(employee) : null;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
    {
        var employee = new Employee
        {
            FirstName = createEmployeeDto.FirstName,
            LastName = createEmployeeDto.LastName,
            Email = createEmployeeDto.Email,
            PhoneNumber = createEmployeeDto.PhoneNumber,
            Position = createEmployeeDto.Position,
            Department = createEmployeeDto.Department,
            Salary = createEmployeeDto.Salary,
            HireDate = createEmployeeDto.HireDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdEmployee = await _employeeRepository.AddAsync(employee);
        return MapToDto(createdEmployee);
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto updateEmployeeDto)
    {
        var existingEmployee = await _employeeRepository.GetByIdAsync(updateEmployeeDto.Id);
        if (existingEmployee == null)
            throw new InvalidOperationException($"Employee with ID {updateEmployeeDto.Id} not found");

        existingEmployee.FirstName = updateEmployeeDto.FirstName;
        existingEmployee.LastName = updateEmployeeDto.LastName;
        existingEmployee.Email = updateEmployeeDto.Email;
        existingEmployee.PhoneNumber = updateEmployeeDto.PhoneNumber;
        existingEmployee.Position = updateEmployeeDto.Position;
        existingEmployee.Department = updateEmployeeDto.Department;
        existingEmployee.Salary = updateEmployeeDto.Salary;
        existingEmployee.HireDate = updateEmployeeDto.HireDate;
        existingEmployee.IsActive = updateEmployeeDto.IsActive;
        existingEmployee.UpdatedAt = DateTime.UtcNow;

        var updatedEmployee = await _employeeRepository.UpdateAsync(existingEmployee);
        return MapToDto(updatedEmployee);
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var exists = await _employeeRepository.ExistsAsync(id);
        if (!exists)
            throw new InvalidOperationException($"Employee with ID {id} not found");

        await _employeeRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string department)
    {
        var employees = await _employeeRepository.GetByDepartmentAsync(department);
        return employees.Select(MapToDto);
    }

    public async Task<IEnumerable<string>> GetDepartmentsAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return employees.Select(e => e.Department).Distinct().OrderBy(d => d);
    }

    public async Task DeactivateEmployeeAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
            throw new InvalidOperationException($"Employee with ID {id} not found");

        employee.Deactivate();
        await _employeeRepository.UpdateAsync(employee);
    }

    public async Task ActivateEmployeeAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
            throw new InvalidOperationException($"Employee with ID {id} not found");

        employee.Activate();
        await _employeeRepository.UpdateAsync(employee);
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Position = employee.Position,
            Department = employee.Department,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive,
            FullName = employee.FullName
        };
    }
}
