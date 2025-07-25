using EmployeeManagement.Web.Components;
using EmployeeManagement.Application.Interfaces;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Infrastructure.Repositories;
using EmployeeManagement.Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DevExpress Blazor services
builder.Services.AddDevExpressBlazor();

// Configure Data Protection for container deployment
var dataProtectionPath = builder.Environment.IsDevelopment()
    ? "/tmp/dataprotection-keys"
    : "/app/data/dataprotection-keys";

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("EmployeeManagement");

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", true);

if (useInMemoryDatabase || string.IsNullOrEmpty(connectionString))
{
    // Development - Use In-Memory Database
    builder.Services.AddDbContext<EmployeeDbContext>(options =>
        options.UseInMemoryDatabase("EmployeeManagement"));
    builder.Services.AddScoped<IEmployeeRepository, InMemoryEmployeeRepository>();
}
else
{
    // Production - Use SQL Server
    builder.Services.AddDbContext<EmployeeDbContext>(options =>
        options.UseSqlServer(connectionString).ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
    builder.Services.AddScoped<IEmployeeRepository, EfEmployeeRepository>();
}

// Register application services following Dependency Injection principles
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EmployeeDbContext>();

var app = builder.Build();

// Ensure database is created and seeded in production
if (!app.Environment.IsDevelopment() && !useInMemoryDatabase)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();
