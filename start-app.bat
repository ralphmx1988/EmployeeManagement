@echo off
echo Starting Employee Management Application...
echo.
cd /d "C:\EmployeeManagement\src\EmployeeManagement.Web"
echo Restoring packages...
dotnet restore
echo.
echo Building application...
dotnet build
echo.
echo Starting application...
echo The application will be available at:
echo   HTTP:  http://localhost:5091
echo   HTTPS: https://localhost:7095
echo.
echo Press Ctrl+C to stop the application
dotnet run
