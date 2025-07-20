using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto);
    Task<EmployeeDto> UpdateEmployeeAsync(UpdateEmployeeDto updateEmployeeDto);
    Task DeleteEmployeeAsync(int id);
    Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentAsync(string department);
    Task<IEnumerable<string>> GetDepartmentsAsync();
    Task DeactivateEmployeeAsync(int id);
    Task ActivateEmployeeAsync(int id);
}
