# 🔧 Cruise Ship Container Deployment - Technical Troubleshooting Guide

## Overview
This guide provides comprehensive troubleshooting procedures for the cruise ship container deployment, covering common issues, diagnostic procedures, and resolution strategies.

## 1. Container Health Diagnostics

### **Container Status Monitoring**
```bash
#!/bin/bash
# Comprehensive container health check script

check_container_health() {
    echo "=== Container Health Check ===" | tee -a /app/logs/health-check.log
    
    # Check all containers status
    echo "1. Container Status:" | tee -a /app/logs/health-check.log
    docker ps -a --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | tee -a /app/logs/health-check.log
    
    # Check container resource usage
    echo -e "\n2. Resource Usage:" | tee -a /app/logs/health-check.log
    docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}\t{{.BlockIO}}" | tee -a /app/logs/health-check.log
    
    # Check container logs for errors
    echo -e "\n3. Recent Error Logs:" | tee -a /app/logs/health-check.log
    for container in $(docker ps --format "{{.Names}}"); do
        echo "--- $container errors ---" | tee -a /app/logs/health-check.log
        docker logs --since=1h "$container" 2>&1 | grep -i "error\|exception\|fail" | tail -5 | tee -a /app/logs/health-check.log
    done
    
    # Check network connectivity
    echo -e "\n4. Network Connectivity:" | tee -a /app/logs/health-check.log
    docker exec employeemanagement-web curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health | tee -a /app/logs/health-check.log
    docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$DB_PASSWORD" -Q "SELECT 1" | tee -a /app/logs/health-check.log
    
    # Check disk space
    echo -e "\n5. Disk Space:" | tee -a /app/logs/health-check.log
    df -h | grep -E "(/app|/var/lib/docker)" | tee -a /app/logs/health-check.log
}

# Run health check
check_container_health
```

### **Application-Level Diagnostics**
```csharp
// Health check endpoint implementation
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly EmployeeDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var healthResult = new HealthCheckResult
        {
            Timestamp = DateTime.UtcNow,
            ShipId = Environment.GetEnvironmentVariable("SHIP_ID"),
            Status = "Healthy"
        };
        
        try
        {
            // Database connectivity check
            var dbHealth = await CheckDatabaseHealth();
            healthResult.Components.Add("Database", dbHealth);
            
            // Application service check
            var appHealth = await CheckApplicationHealth();
            healthResult.Components.Add("Application", appHealth);
            
            // External dependencies check
            var extHealth = await CheckExternalDependencies();
            healthResult.Components.Add("External", extHealth);
            
            // Resource utilization check
            var resourceHealth = CheckResourceUtilization();
            healthResult.Components.Add("Resources", resourceHealth);
            
            // Determine overall status
            healthResult.Status = healthResult.Components.All(c => c.Value.Status == "Healthy") 
                ? "Healthy" : "Degraded";
                
            return Ok(healthResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            healthResult.Status = "Unhealthy";
            healthResult.Error = ex.Message;
            return StatusCode(503, healthResult);
        }
    }
    
    private async Task<ComponentHealth> CheckDatabaseHealth()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Test basic connectivity
            await _dbContext.Database.CanConnectAsync();
            
            // Test query performance
            var employeeCount = await _dbContext.Employees.CountAsync();
            
            stopwatch.Stop();
            
            return new ComponentHealth
            {
                Status = "Healthy",
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Details = new { EmployeeCount = employeeCount }
            };
        }
        catch (Exception ex)
        {
            return new ComponentHealth
            {
                Status = "Unhealthy",
                Error = ex.Message,
                ResponseTime = stopwatch.ElapsedMilliseconds
            };
        }
    }
    
    private ComponentHealth CheckResourceUtilization()
    {
        var process = Process.GetCurrentProcess();
        var memoryUsageMB = process.WorkingSet64 / 1024 / 1024;
        var cpuUsage = GetCpuUsage();
        
        var status = memoryUsageMB > 1024 || cpuUsage > 80 ? "Degraded" : "Healthy";
        
        return new ComponentHealth
        {
            Status = status,
            Details = new 
            { 
                MemoryUsageMB = memoryUsageMB,
                CpuUsagePercent = cpuUsage,
                DiskFreeGB = GetDiskFreeSpace()
            }
        };
    }
}
```

## 2. Network Connectivity Issues

### **Diagnostic Procedures**
```bash
#!/bin/bash
# Network connectivity diagnostic script

diagnose_network() {
    echo "=== Network Connectivity Diagnosis ===" | tee -a /app/logs/network-diag.log
    
    # 1. Check internal container network
    echo "1. Internal Container Network:" | tee -a /app/logs/network-diag.log
    docker network ls | tee -a /app/logs/network-diag.log
    docker network inspect cruise-network | jq '.[] | {Name: .Name, Driver: .Driver, Containers: .Containers}' | tee -a /app/logs/network-diag.log
    
    # 2. Test container-to-container connectivity
    echo -e "\n2. Container-to-Container Connectivity:" | tee -a /app/logs/network-diag.log
    docker exec employeemanagement-web ping -c 3 sqlserver | tee -a /app/logs/network-diag.log
    docker exec employeemanagement-web telnet sqlserver 1433 | tee -a /app/logs/network-diag.log
    
    # 3. Test external connectivity (when available)
    echo -e "\n3. External Connectivity:" | tee -a /app/logs/network-diag.log
    docker exec employeemanagement-web ping -c 3 8.8.8.8 | tee -a /app/logs/network-diag.log
    docker exec employeemanagement-web nslookup registry.cruiseline.com | tee -a /app/logs/network-diag.log
    
    # 4. Check port bindings
    echo -e "\n4. Port Bindings:" | tee -a /app/logs/network-diag.log
    netstat -tlnp | grep -E ":80|:443|:1433|:8080|:9090" | tee -a /app/logs/network-diag.log
    
    # 5. Check firewall rules (if applicable)
    echo -e "\n5. Firewall Status:" | tee -a /app/logs/network-diag.log
    if command -v ufw >/dev/null; then
        ufw status | tee -a /app/logs/network-diag.log
    elif command -v firewall-cmd >/dev/null; then
        firewall-cmd --list-all | tee -a /app/logs/network-diag.log
    fi
}

# Network performance test
test_network_performance() {
    echo "=== Network Performance Test ===" | tee -a /app/logs/network-perf.log
    
    # Bandwidth test to shore (when connected)
    if ping -c 1 registry.cruiseline.com >/dev/null 2>&1; then
        echo "Testing bandwidth to shore registry..." | tee -a /app/logs/network-perf.log
        docker exec update-agent python3 -c "
import time
import requests
start = time.time()
response = requests.get('https://registry.cruiseline.com/v2/', timeout=30)
end = time.time()
print(f'Latency: {(end-start)*1000:.2f}ms')
print(f'Status: {response.status_code}')
" | tee -a /app/logs/network-perf.log
    else
        echo "No external connectivity available" | tee -a /app/logs/network-perf.log
    fi
}

diagnose_network
test_network_performance
```

## 3. Database Issues

### **SQL Server Diagnostics**
```sql
-- Database health and performance diagnostics
-- Run these queries within the SQL Server container

-- 1. Database status and size
SELECT 
    name AS DatabaseName,
    state_desc AS Status,
    (size * 8.0 / 1024) AS SizeMB,
    (FILEPROPERTY(name, 'SpaceUsed') * 8.0 / 1024) AS UsedMB,
    ((size - FILEPROPERTY(name, 'SpaceUsed')) * 8.0 / 1024) AS FreeMB
FROM sys.master_files 
WHERE database_id = DB_ID('EmployeeManagement');

-- 2. Active connections
SELECT 
    session_id,
    login_name,
    host_name,
    program_name,
    status,
    cpu_time,
    memory_usage,
    reads,
    writes,
    last_request_start_time
FROM sys.dm_exec_sessions 
WHERE database_id = DB_ID('EmployeeManagement');

-- 3. Long-running queries
SELECT 
    r.session_id,
    r.status,
    r.command,
    r.cpu_time,
    r.total_elapsed_time,
    t.text AS query_text
FROM sys.dm_exec_requests r
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
WHERE r.total_elapsed_time > 5000  -- queries running > 5 seconds
ORDER BY r.total_elapsed_time DESC;

-- 4. Database locks
SELECT 
    tl.resource_type,
    tl.resource_database_id,
    tl.resource_associated_entity_id,
    tl.request_mode,
    tl.request_status,
    wt.blocking_session_id,
    wt.wait_type,
    wt.wait_time_ms
FROM sys.dm_tran_locks tl
LEFT JOIN sys.dm_os_waiting_tasks wt ON tl.lock_owner_address = wt.resource_address
WHERE tl.resource_database_id = DB_ID('EmployeeManagement');

-- 5. Error log analysis
EXEC xp_readerrorlog 0, 1, N'Error', N'EmployeeManagement';
```

### **Database Recovery Procedures**
```bash
#!/bin/bash
# Database recovery and maintenance procedures

recover_database() {
    echo "=== Database Recovery Procedures ===" | tee -a /app/logs/db-recovery.log
    
    # 1. Check database container status
    echo "1. Checking database container..." | tee -a /app/logs/db-recovery.log
    if ! docker ps | grep -q employeemanagement-db; then
        echo "Database container is not running. Starting..." | tee -a /app/logs/db-recovery.log
        docker start employeemanagement-db
        sleep 30
    fi
    
    # 2. Test database connectivity
    echo "2. Testing database connectivity..." | tee -a /app/logs/db-recovery.log
    max_attempts=10
    attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd \
           -S localhost -U sa -P "$DB_PASSWORD" -Q "SELECT 1" >/dev/null 2>&1; then
            echo "Database is accessible" | tee -a /app/logs/db-recovery.log
            break
        else
            echo "Attempt $attempt/$max_attempts failed. Waiting..." | tee -a /app/logs/db-recovery.log
            sleep 10
            ((attempt++))
        fi
    done
    
    # 3. Check database integrity
    echo "3. Running database integrity check..." | tee -a /app/logs/db-recovery.log
    docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "$DB_PASSWORD" \
        -Q "DBCC CHECKDB('EmployeeManagement') WITH NO_INFOMSGS;" | tee -a /app/logs/db-recovery.log
    
    # 4. Restore from backup if corruption detected
    if grep -q "corruption" /app/logs/db-recovery.log; then
        echo "4. Corruption detected. Attempting restore from backup..." | tee -a /app/logs/db-recovery.log
        restore_from_backup
    fi
}

restore_from_backup() {
    echo "=== Restoring Database from Backup ===" | tee -a /app/logs/db-restore.log
    
    # Find latest backup
    latest_backup=$(ls -t /var/opt/mssql/backups/full_*.bak 2>/dev/null | head -n1)
    
    if [ -n "$latest_backup" ]; then
        echo "Restoring from: $latest_backup" | tee -a /app/logs/db-restore.log
        
        # Stop application to prevent connections
        docker stop employeemanagement-web
        
        # Restore database
        docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd \
            -S localhost -U sa -P "$DB_PASSWORD" \
            -Q "RESTORE DATABASE EmployeeManagement FROM DISK = '$latest_backup' WITH REPLACE;" | tee -a /app/logs/db-restore.log
        
        # Restart application
        docker start employeemanagement-web
        
        echo "Database restore completed" | tee -a /app/logs/db-restore.log
    else
        echo "No backup files found!" | tee -a /app/logs/db-restore.log
    fi
}

# Performance optimization
optimize_database() {
    echo "=== Database Performance Optimization ===" | tee -a /app/logs/db-optimize.log
    
    # Update statistics
    docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "$DB_PASSWORD" \
        -Q "EXEC sp_updatestats;" | tee -a /app/logs/db-optimize.log
    
    # Rebuild indexes
    docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "$DB_PASSWORD" \
        -Q "ALTER INDEX ALL ON Employees REBUILD;" | tee -a /app/logs/db-optimize.log
    
    # Shrink log file if necessary
    docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "$DB_PASSWORD" \
        -Q "DBCC SHRINKFILE('EmployeeManagement_Log', 100);" | tee -a /app/logs/db-optimize.log
}

recover_database
```

## 4. Update Agent Issues

### **Update Agent Diagnostics**
```python
# update-agent-diagnostics.py
import json
import logging
import docker
import requests
from datetime import datetime, timedelta

class UpdateAgentDiagnostics:
    def __init__(self):
        self.docker_client = docker.from_env()
        self.logger = logging.getLogger(__name__)
        
    def run_full_diagnostics(self):
        """Run comprehensive diagnostics on update agent"""
        results = {
            'timestamp': datetime.utcnow().isoformat(),
            'diagnostics': {}
        }
        
        # 1. Check update agent container status
        results['diagnostics']['container_status'] = self.check_container_status()
        
        # 2. Check registry connectivity
        results['diagnostics']['registry_connectivity'] = self.check_registry_connectivity()
        
        # 3. Check local images
        results['diagnostics']['local_images'] = self.check_local_images()
        
        # 4. Check update configuration
        results['diagnostics']['update_config'] = self.check_update_configuration()
        
        # 5. Check recent update attempts
        results['diagnostics']['recent_updates'] = self.check_recent_updates()
        
        # 6. Check disk space for image storage
        results['diagnostics']['disk_space'] = self.check_disk_space()
        
        return results
    
    def check_container_status(self):
        """Check update agent container health"""
        try:
            container = self.docker_client.containers.get('update-agent')
            return {
                'status': container.status,
                'created': container.attrs['Created'],
                'started': container.attrs['State']['StartedAt'],
                'restarts': container.attrs['RestartCount'],
                'health': 'healthy' if container.status == 'running' else 'unhealthy'
            }
        except docker.errors.NotFound:
            return {'status': 'not_found', 'health': 'unhealthy'}
        except Exception as e:
            return {'status': 'error', 'error': str(e), 'health': 'unhealthy'}
    
    def check_registry_connectivity(self):
        """Test connectivity to container registry"""
        registry_url = os.getenv('REGISTRY_URL', 'registry.cruiseline.com')
        
        try:
            # Test basic connectivity
            response = requests.get(f"https://{registry_url}/v2/", timeout=30)
            
            # Test authentication
            auth_response = requests.get(
                f"https://{registry_url}/v2/employeemanagement/manifests/latest",
                headers={'Authorization': f'Bearer {self.get_auth_token()}'},
                timeout=30
            )
            
            return {
                'registry_reachable': response.status_code == 200,
                'authentication_valid': auth_response.status_code in [200, 404],
                'response_time_ms': response.elapsed.total_seconds() * 1000,
                'last_check': datetime.utcnow().isoformat()
            }
        except requests.exceptions.RequestException as e:
            return {
                'registry_reachable': False,
                'error': str(e),
                'last_check': datetime.utcnow().isoformat()
            }
    
    def check_recent_updates(self):
        """Check recent update attempts and their status"""
        try:
            # Read update log file
            with open('/app/logs/update-agent.log', 'r') as f:
                logs = f.readlines()
            
            # Parse recent update attempts (last 24 hours)
            recent_updates = []
            for line in logs[-100:]:  # Check last 100 lines
                if 'update' in line.lower() and any(keyword in line.lower() 
                                                   for keyword in ['success', 'failed', 'started']):
                    recent_updates.append({
                        'timestamp': self.extract_timestamp(line),
                        'message': line.strip(),
                        'status': self.extract_status(line)
                    })
            
            return {
                'total_attempts': len(recent_updates),
                'successful_updates': len([u for u in recent_updates if u['status'] == 'success']),
                'failed_updates': len([u for u in recent_updates if u['status'] == 'failed']),
                'recent_attempts': recent_updates[-5:]  # Last 5 attempts
            }
        except Exception as e:
            return {'error': str(e)}
    
    def fix_common_issues(self):
        """Attempt to fix common update agent issues"""
        fixes_applied = []
        
        # 1. Restart update agent if unhealthy
        container_status = self.check_container_status()
        if container_status['health'] == 'unhealthy':
            try:
                container = self.docker_client.containers.get('update-agent')
                container.restart()
                fixes_applied.append('restarted_update_agent')
            except Exception as e:
                fixes_applied.append(f'failed_to_restart_agent: {e}')
        
        # 2. Clean up old images to free space
        disk_status = self.check_disk_space()
        if disk_status['usage_percent'] > 85:
            try:
                self.docker_client.images.prune()
                fixes_applied.append('pruned_old_images')
            except Exception as e:
                fixes_applied.append(f'failed_to_prune_images: {e}')
        
        # 3. Reset update configuration if corrupted
        try:
            config_status = self.check_update_configuration()
            if 'error' in config_status:
                self.reset_update_configuration()
                fixes_applied.append('reset_update_configuration')
        except Exception as e:
            fixes_applied.append(f'failed_to_reset_config: {e}')
        
        return {'fixes_applied': fixes_applied}

# Run diagnostics
if __name__ == "__main__":
    diagnostics = UpdateAgentDiagnostics()
    results = diagnostics.run_full_diagnostics()
    
    # Save results
    with open('/app/logs/update-diagnostics.json', 'w') as f:
        json.dump(results, f, indent=2)
    
    # Apply fixes if needed
    if any(component['health'] == 'unhealthy' 
           for component in results['diagnostics'].values() 
           if isinstance(component, dict) and 'health' in component):
        fixes = diagnostics.fix_common_issues()
        print(f"Applied fixes: {fixes['fixes_applied']}")
```

## 5. Performance Issues

### **Resource Monitoring and Optimization**
```bash
#!/bin/bash
# Performance monitoring and optimization script

monitor_performance() {
    echo "=== Performance Monitoring ===" | tee -a /app/logs/performance.log
    
    # 1. System resource usage
    echo "1. System Resources:" | tee -a /app/logs/performance.log
    echo "CPU Usage:" | tee -a /app/logs/performance.log
    top -bn1 | head -5 | tee -a /app/logs/performance.log
    
    echo "Memory Usage:" | tee -a /app/logs/performance.log
    free -h | tee -a /app/logs/performance.log
    
    echo "Disk Usage:" | tee -a /app/logs/performance.log
    df -h | tee -a /app/logs/performance.log
    
    echo "Network I/O:" | tee -a /app/logs/performance.log
    sar -n DEV 1 1 | tee -a /app/logs/performance.log
    
    # 2. Container resource usage
    echo -e "\n2. Container Resources:" | tee -a /app/logs/performance.log
    docker stats --no-stream | tee -a /app/logs/performance.log
    
    # 3. Database performance
    echo -e "\n3. Database Performance:" | tee -a /app/logs/performance.log
    docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "$DB_PASSWORD" \
        -Q "SELECT 
            (SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1) AS ActiveSessions,
            (SELECT COUNT(*) FROM sys.dm_exec_requests) AS ActiveRequests,
            (SELECT SUM(user_seeks + user_scans + user_lookups) FROM sys.dm_db_index_usage_stats) AS IndexUsage;" | tee -a /app/logs/performance.log
    
    # 4. Application response times
    echo -e "\n4. Application Response Times:" | tee -a /app/logs/performance.log
    curl -w "Health endpoint: %{time_total}s\n" -s -o /dev/null http://localhost/health | tee -a /app/logs/performance.log
    curl -w "Home page: %{time_total}s\n" -s -o /dev/null http://localhost/ | tee -a /app/logs/performance.log
}

optimize_performance() {
    echo "=== Performance Optimization ===" | tee -a /app/logs/performance-opt.log
    
    # 1. Optimize Docker daemon
    echo "1. Optimizing Docker daemon..." | tee -a /app/logs/performance-opt.log
    
    # Clean up unused resources
    docker system prune -f | tee -a /app/logs/performance-opt.log
    
    # 2. Optimize database
    echo "2. Optimizing database..." | tee -a /app/logs/performance-opt.log
    
    # Update statistics
    docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "$DB_PASSWORD" \
        -Q "EXEC sp_updatestats;" | tee -a /app/logs/performance-opt.log
    
    # 3. Adjust container resource limits if needed
    echo "3. Checking resource limits..." | tee -a /app/logs/performance-opt.log
    
    # Check if containers are hitting resource limits
    docker stats --no-stream | awk 'NR>1 {
        if ($3 > 90) print $1 " is using high CPU: " $3
        if ($4 > 90) print $1 " is using high memory: " $4
    }' | tee -a /app/logs/performance-opt.log
}

# Generate performance report
generate_performance_report() {
    echo "=== Performance Report $(date) ===" > /app/logs/performance-report.log
    
    # Collect metrics
    monitor_performance >> /app/logs/performance-report.log
    
    # Add recommendations
    echo -e "\n=== Recommendations ===" >> /app/logs/performance-report.log
    
    # Check disk space
    disk_usage=$(df / | awk 'NR==2 {print $5}' | sed 's/%//')
    if [ "$disk_usage" -gt 80 ]; then
        echo "WARNING: Disk usage is at ${disk_usage}%. Consider cleanup." >> /app/logs/performance-report.log
    fi
    
    # Check memory usage
    mem_usage=$(free | awk 'NR==2{printf "%.0f", $3*100/$2}')
    if [ "$mem_usage" -gt 80 ]; then
        echo "WARNING: Memory usage is at ${mem_usage}%. Consider optimization." >> /app/logs/performance-report.log
    fi
    
    echo "Performance report generated: /app/logs/performance-report.log"
}

# Run performance monitoring
monitor_performance
generate_performance_report
```

This comprehensive troubleshooting guide provides the tools and procedures needed to diagnose and resolve issues in the cruise ship container deployment, ensuring maximum uptime and performance in the challenging maritime environment.
