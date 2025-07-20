# 🚀 Quick Start Guide - Employee Management System

## Prerequisites Checklist ✅

Before starting, make sure you have installed:

- [ ] **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- [ ] **Visual Studio 2022** or **VS Code** with C# extension
- [ ] **Git** for version control
- [ ] **Docker Desktop** (for containerization)
- [ ] **Azure CLI** (for Azure deployment)

## 🏃‍♂️ Running the Application Locally

### Option 1: Using Visual Studio Code

1. **Open the project**:
   ```bash
   code .
   ```

2. **Run the application** using the pre-configured task:
   - Press `Ctrl+Shift+P` (Windows) or `Cmd+Shift+P` (Mac)
   - Type "Tasks: Run Task"
   - Select "Run Employee Management App"
   - The application will start at `https://localhost:7071`

### Option 2: Using Command Line

1. **Navigate to the project directory**:
   ```bash
   cd C:\EmployeeManagement
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Run the application**:
   ```bash
   dotnet run --project src/EmployeeManagement.Web
   ```

4. **Access the application**:
   - Open your browser and navigate to `https://localhost:7071`
   - The application will display the Employee Management Dashboard

## 🎯 What You'll See

### Dashboard Features
- **Statistics Cards**: Total employees, active employees, departments, average salary
- **Department Overview**: List of employees by department
- **Recent Hires**: Latest 5 employees hired (within last 30 days)
- **Quick Actions**: Navigation buttons to employee management

### Employee Management Features
- **Employee Grid**: DevExpress data grid with 50 mock employees
- **Search & Filter**: Filter by department, search by name/email/position
- **CRUD Operations**: Add, Edit, Delete employees
- **Rich UI**: DevExpress components with professional styling

## 🗂️ Sample Data

The application comes with **50 mock employees** across **6 departments**:
- **IT**: Software Engineers, DevOps Engineers, QA Engineers, Tech Leads, Architects
- **HR**: HR Managers, Recruiters, HR Business Partners, Training Coordinators
- **Finance**: Financial Analysts, Accountants, Finance Managers, Controllers
- **Marketing**: Marketing Managers, Digital Marketers, Content Creators, Brand Managers
- **Operations**: Operations Managers, Process Analysts, Project Managers, Coordinators
- **Sales**: Sales Representatives, Account Managers, Sales Managers, Business Developers

## 🧪 Running Tests

```bash
# Run all unit tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🐳 Docker Quick Start

### Build and Run with Docker

```bash
# Build the Docker image
docker build -t employee-management:latest .

# Run the container
docker run -p 8080:8080 employee-management:latest

# Access at http://localhost:8080
```

## 🔧 Development Tips

### Visual Studio Code Extensions (Recommended)
- **C# Dev Kit** - Enhanced C# support
- **Azure Tools** - Azure integration
- **Docker** - Docker support
- **Kubernetes** - K8s management

### Key Features to Explore
1. **Clean Architecture**: Navigate through the layered project structure
2. **SOLID Principles**: Notice dependency injection and interface usage
3. **DevExpress Components**: Rich UI controls in the Employees page
4. **Blazor Server**: Real-time updates with SignalR
5. **Mock Data**: Realistic employee dataset for testing

## 🚨 Troubleshooting

### Common Issues & Solutions

**Issue**: Port already in use
```bash
# Solution: Change port in launchSettings.json or use different port
dotnet run --project src/EmployeeManagement.Web --urls "https://localhost:7072"
```

**Issue**: Package restore fails
```bash
# Solution: Clear NuGet cache and restore
dotnet nuget locals all --clear
dotnet restore --force
```

**Issue**: Build errors
```bash
# Solution: Clean and rebuild
dotnet clean
dotnet build
```

**Issue**: HTTPS certificate issues
```bash
# Solution: Trust the development certificate
dotnet dev-certs https --trust
```

## 📱 Application Navigation

1. **Dashboard** (`/`) - Overview and statistics
2. **Employees** (`/employees`) - Main employee management interface
3. **Counter** (`/counter`) - Sample Blazor component
4. **Weather** (`/weather`) - Sample data display

## 🎉 Success Criteria

You've successfully set up the application if you can:
- [ ] Navigate to the dashboard and see employee statistics
- [ ] View the employee list with 50 sample employees
- [ ] Add a new employee using the form
- [ ] Edit an existing employee
- [ ] Filter employees by department
- [ ] Search for employees by name

## 🔗 Next Steps

Once you have the application running locally:

1. **Explore the Code**: Study the Clean Architecture implementation
2. **Add Features**: Implement additional functionality
3. **Setup Azure**: Follow the main README for Azure deployment
4. **Configure CI/CD**: Set up Azure DevOps pipeline
5. **Deploy to AKS**: Deploy to Azure Kubernetes Service

## 📞 Support

If you encounter any issues:
1. Check the troubleshooting section above
2. Verify all prerequisites are installed
3. Review the main README.md for detailed documentation
4. Check the Azure DevOps pipeline configuration

---

**Happy Coding! 🚀**

The Employee Management System demonstrates modern .NET development practices with Clean Architecture, SOLID principles, and cloud-ready deployment strategies.
