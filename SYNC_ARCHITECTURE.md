# 🔄 Data Synchronization & Conflict Resolution - Deep Technical Analysis

## Overview
This document provides an in-depth technical explanation of how data synchronization works between cruise ships and shore systems, including conflict resolution strategies for disconnected scenarios.

## 1. Synchronization Architecture

### **Hub-and-Spoke Model**
```
                    ┌─────────────────────────────────────┐
                    │           Shore Data Center         │
                    │                                     │
                    │  ┌─────────────────┐               │
                    │  │ Central Database │               │
                    │  │  (Master Node)   │               │
                    │  └─────────────────┘               │
                    │          │                         │
                    │  ┌─────────────────┐               │
                    │  │  Sync Gateway   │               │
                    │  │  (REST API)     │               │
                    │  └─────────────────┘               │
                    └─────────────┬───────────────────────┘
                                  │
                     ┌────────────┼────────────┐
                     │            │            │
            ┌────────▼────┐  ┌────▼────┐  ┌───▼─────┐
            │   Ship A    │  │ Ship B  │  │ Ship C  │
            │ (Edge Node) │  │(Edge...) │  │(Edge...)│
            └─────────────┘  └─────────┘  └─────────┘
```

### **Synchronization States**
```csharp
public enum SyncState
{
    Offline,           // No internet connectivity
    Connecting,        // Attempting to establish connection
    Authenticating,    // Validating ship credentials
    Synchronizing,     // Active data sync in progress
    Conflicted,        // Data conflicts detected
    Failed,            // Sync operation failed
    Completed          // Sync successful
}

public class SyncStatus
{
    public SyncState CurrentState { get; set; }
    public DateTime LastSyncAttempt { get; set; }
    public DateTime LastSuccessfulSync { get; set; }
    public int PendingChanges { get; set; }
    public List<ConflictRecord> Conflicts { get; set; }
    public string ErrorMessage { get; set; }
}
```

## 2. Conflict Resolution Strategies

### **Vector Clock Implementation**
```csharp
public class VectorClock
{
    private Dictionary<string, long> _clocks;
    
    public VectorClock(string nodeId)
    {
        _clocks = new Dictionary<string, long> { { nodeId, 0 } };
    }
    
    public void Increment(string nodeId)
    {
        if (_clocks.ContainsKey(nodeId))
            _clocks[nodeId]++;
        else
            _clocks[nodeId] = 1;
    }
    
    public bool HappensBefore(VectorClock other)
    {
        bool allLessOrEqual = true;
        bool atLeastOneLess = false;
        
        foreach (var kvp in _clocks)
        {
            var otherValue = other._clocks.GetValueOrDefault(kvp.Key, 0);
            if (kvp.Value > otherValue)
            {
                allLessOrEqual = false;
                break;
            }
            if (kvp.Value < otherValue)
                atLeastOneLess = true;
        }
        
        return allLessOrEqual && atLeastOneLess;
    }
}

// Employee entity with conflict resolution metadata
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // ... other properties
    
    // Conflict resolution metadata
    public VectorClock VectorClock { get; set; }
    public string OriginNodeId { get; set; }
    public DateTime LastModified { get; set; }
    public byte[] RowVersion { get; set; }
    public bool IsTombstone { get; set; }  // For soft deletes
}
```

### **Three-Way Merge Algorithm**
```csharp
public class ThreeWayMergeResolver
{
    public MergeResult<Employee> ResolveConflict(
        Employee baseVersion,    // Last known common ancestor
        Employee localVersion,   // Ship's current version
        Employee remoteVersion)  // Shore's current version
    {
        var result = new MergeResult<Employee>();
        
        // 1. Check if one side is unchanged
        if (AreEqual(baseVersion, localVersion))
        {
            result.MergedEntity = remoteVersion;
            result.ConflictType = ConflictType.NoConflict;
            return result;
        }
        
        if (AreEqual(baseVersion, remoteVersion))
        {
            result.MergedEntity = localVersion;
            result.ConflictType = ConflictType.NoConflict;
            return result;
        }
        
        // 2. Attempt automatic merge
        var mergedEmployee = new Employee
        {
            Id = localVersion.Id,
            FirstName = ResolveField(baseVersion.FirstName, localVersion.FirstName, remoteVersion.FirstName),
            LastName = ResolveField(baseVersion.LastName, localVersion.LastName, remoteVersion.LastName),
            Email = ResolveField(baseVersion.Email, localVersion.Email, remoteVersion.Email),
            // ... resolve other fields
        };
        
        // 3. Check for unresolvable conflicts
        var conflicts = DetectConflicts(baseVersion, localVersion, remoteVersion);
        if (conflicts.Any())
        {
            result.ConflictType = ConflictType.RequiresManualResolution;
            result.Conflicts = conflicts;
            result.MergedEntity = mergedEmployee;
        }
        else
        {
            result.ConflictType = ConflictType.AutoResolved;
            result.MergedEntity = mergedEmployee;
        }
        
        return result;
    }
    
    private string ResolveField(string baseValue, string localValue, string remoteValue)
    {
        // If both sides changed to the same value, use it
        if (localValue == remoteValue)
            return localValue;
            
        // If only one side changed, use the changed value
        if (baseValue == localValue)
            return remoteValue;
        if (baseValue == remoteValue)
            return localValue;
            
        // Both sides changed to different values - conflict
        throw new ConflictException($"Field conflict: local='{localValue}', remote='{remoteValue}'");
    }
}
```

## 3. Offline Change Tracking

### **Change Log Implementation**
```sql
-- Change tracking table for offline modifications
CREATE TABLE EmployeeChangeLog (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    ChangeType NVARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    FieldName NVARCHAR(100),
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ChangedBy NVARCHAR(100) NOT NULL,
    ShipId NVARCHAR(50) NOT NULL,
    VectorClockJson NVARCHAR(MAX),
    IsSynced BIT NOT NULL DEFAULT 0,
    SyncedAt DATETIME2,
    ConflictResolved BIT NOT NULL DEFAULT 0
);

-- Trigger to capture changes
CREATE TRIGGER TR_Employee_ChangeTracking
ON Employees
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Handle INSERT
    IF EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO EmployeeChangeLog (EmployeeId, ChangeType, ChangedBy, ShipId, VectorClockJson)
        SELECT Id, 'INSERT', SYSTEM_USER, @ShipId, '{"' + @ShipId + '":1}'
        FROM inserted;
    END
    
    -- Handle UPDATE
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        -- Track field-level changes
        INSERT INTO EmployeeChangeLog (EmployeeId, ChangeType, FieldName, OldValue, NewValue, ChangedBy, ShipId)
        SELECT 
            i.Id, 
            'UPDATE',
            'FirstName',
            d.FirstName,
            i.FirstName,
            SYSTEM_USER,
            @ShipId
        FROM inserted i
        INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.FirstName <> d.FirstName;
        
        -- Similar for other fields...
    END
    
    -- Handle DELETE
    IF EXISTS (SELECT 1 FROM deleted) AND NOT EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO EmployeeChangeLog (EmployeeId, ChangeType, ChangedBy, ShipId, VectorClockJson)
        SELECT Id, 'DELETE', SYSTEM_USER, @ShipId, '{"' + @ShipId + '":1}'
        FROM deleted;
    END
END;
```

### **Delta Sync Implementation**
```csharp
public class DeltaSyncService
{
    public async Task<SyncResult> PerformDeltaSync(string shipId, DateTime lastSyncTimestamp)
    {
        var result = new SyncResult();
        
        try
        {
            // 1. Get local changes since last sync
            var localChanges = await GetLocalChangesSince(lastSyncTimestamp);
            
            // 2. Send local changes to shore
            var pushResult = await PushChangesToShore(shipId, localChanges);
            
            // 3. Get remote changes since last sync
            var remoteChanges = await GetRemoteChangesSince(shipId, lastSyncTimestamp);
            
            // 4. Apply remote changes locally with conflict resolution
            var applyResult = await ApplyRemoteChanges(remoteChanges);
            
            // 5. Update sync timestamp
            await UpdateLastSyncTimestamp(DateTime.UtcNow);
            
            result.IsSuccess = true;
            result.LocalChangesPushed = pushResult.SuccessCount;
            result.RemoteChangesApplied = applyResult.SuccessCount;
            result.ConflictsDetected = applyResult.ConflictCount;
            
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Delta sync failed for ship {ShipId}", shipId);
        }
        
        return result;
    }
    
    private async Task<List<ChangeRecord>> GetLocalChangesSince(DateTime timestamp)
    {
        return await _context.EmployeeChangeLog
            .Where(c => c.ChangedAt > timestamp && !c.IsSynced)
            .OrderBy(c => c.ChangedAt)
            .Select(c => new ChangeRecord
            {
                EntityId = c.EmployeeId,
                ChangeType = c.ChangeType,
                FieldChanges = c.FieldName != null ? new Dictionary<string, object>
                {
                    { c.FieldName, c.NewValue }
                } : null,
                Timestamp = c.ChangedAt,
                VectorClock = JsonSerializer.Deserialize<VectorClock>(c.VectorClockJson)
            })
            .ToListAsync();
    }
}
```

## 4. Network Optimization for Maritime Environment

### **Bandwidth-Aware Synchronization**
```csharp
public class BandwidthAwareSyncStrategy
{
    private readonly INetworkMonitor _networkMonitor;
    
    public async Task<SyncStrategy> DetermineSyncStrategy()
    {
        var networkMetrics = await _networkMonitor.GetCurrentMetrics();
        
        return networkMetrics.AvailableBandwidth switch
        {
            < 1_000_000 => new SyncStrategy  // < 1 Mbps
            {
                Mode = SyncMode.CriticalOnly,
                BatchSize = 10,
                CompressionLevel = CompressionLevel.Maximum,
                RetryPolicy = new ExponentialBackoff(TimeSpan.FromMinutes(30))
            },
            < 5_000_000 => new SyncStrategy  // < 5 Mbps
            {
                Mode = SyncMode.Incremental,
                BatchSize = 50,
                CompressionLevel = CompressionLevel.High,
                RetryPolicy = new ExponentialBackoff(TimeSpan.FromMinutes(10))
            },
            _ => new SyncStrategy            // >= 5 Mbps
            {
                Mode = SyncMode.Full,
                BatchSize = 200,
                CompressionLevel = CompressionLevel.Medium,
                RetryPolicy = new ExponentialBackoff(TimeSpan.FromMinutes(5))
            }
        };
    }
}

// Compression for low bandwidth scenarios
public class DataCompressionService
{
    public byte[] CompressChangeSet(List<ChangeRecord> changes, CompressionLevel level)
    {
        var json = JsonSerializer.Serialize(changes);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        using var output = new MemoryStream();
        using var gzip = new GZipStream(output, level);
        gzip.Write(bytes, 0, bytes.Length);
        
        return output.ToArray();
    }
    
    public List<ChangeRecord> DecompressChangeSet(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        
        gzip.CopyTo(output);
        var json = Encoding.UTF8.GetString(output.ToArray());
        
        return JsonSerializer.Deserialize<List<ChangeRecord>>(json);
    }
}
```

## 5. Monitoring & Observability for Sync Operations

### **Sync Metrics Collection**
```csharp
public class SyncMetricsCollector
{
    private readonly IMetricsLogger _metricsLogger;
    
    public void RecordSyncOperation(SyncResult result)
    {
        // Record sync duration
        _metricsLogger.RecordValue("sync.duration_ms", result.DurationMs);
        
        // Record data volume
        _metricsLogger.RecordValue("sync.changes_pushed", result.LocalChangesPushed);
        _metricsLogger.RecordValue("sync.changes_pulled", result.RemoteChangesApplied);
        
        // Record conflicts
        _metricsLogger.RecordValue("sync.conflicts_detected", result.ConflictsDetected);
        _metricsLogger.RecordValue("sync.conflicts_auto_resolved", result.ConflictsAutoResolved);
        
        // Record network metrics
        _metricsLogger.RecordValue("sync.bandwidth_used_bytes", result.BandwidthUsed);
        _metricsLogger.RecordValue("sync.network_latency_ms", result.NetworkLatency);
        
        // Record success/failure
        _metricsLogger.RecordCounter($"sync.result.{(result.IsSuccess ? "success" : "failure")}");
        
        // Ship-specific metrics
        _metricsLogger.RecordValue("sync.ship_specific", result.ShipId, new Dictionary<string, object>
        {
            { "last_sync", result.Timestamp },
            { "data_freshness_hours", (DateTime.UtcNow - result.Timestamp).TotalHours }
        });
    }
}
```

This comprehensive synchronization system ensures data consistency across the fleet while handling the unique challenges of maritime connectivity, providing robust conflict resolution and optimal bandwidth utilization.
