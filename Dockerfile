# Use the .NET 9.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/EmployeeManagement.Web/EmployeeManagement.Web.csproj", "src/EmployeeManagement.Web/"]
COPY ["src/EmployeeManagement.Application/EmployeeManagement.Application.csproj", "src/EmployeeManagement.Application/"]
COPY ["src/EmployeeManagement.Domain/EmployeeManagement.Domain.csproj", "src/EmployeeManagement.Domain/"]
COPY ["src/EmployeeManagement.Infrastructure/EmployeeManagement.Infrastructure.csproj", "src/EmployeeManagement.Infrastructure/"]
RUN dotnet restore "src/EmployeeManagement.Web/EmployeeManagement.Web.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/EmployeeManagement.Web"
RUN dotnet build "EmployeeManagement.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EmployeeManagement.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeManagement.Web.dll"]
