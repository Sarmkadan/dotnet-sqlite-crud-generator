// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Deployment Guide

Guide for deploying SQLite CRUD Generator in production environments.

## Deployment Strategies

### Strategy 1: Standalone Console Application

**Best for**: Small projects, microservices, scheduled jobs

```bash
# Build for deployment
dotnet publish -c Release -o ./publish

# Copy to production server
scp -r ./publish/ user@server:/opt/crudgenerator/

# Run on server
ssh user@server
cd /opt/crudgenerator
./DotNet.SQLite.CrudGenerator
```

### Strategy 2: Docker Container

**Best for**: Cloud deployments, containerized infrastructure

```bash
# Build image
docker build -t dotnet-crud-generator:1.0 .

# Tag for registry
docker tag dotnet-crud-generator:1.0 myregistry/dotnet-crud-generator:1.0

# Push to registry
docker push myregistry/dotnet-crud-generator:1.0

# Run container
docker run -d \
  --name crudgen \
  -e DATABASE_PATH=/data/app.db \
  -v /data:/data \
  myregistry/dotnet-crud-generator:1.0
```

### Strategy 3: Docker Compose

**Best for**: Multi-service applications, development environments

```bash
docker-compose up -d
docker-compose logs -f
docker-compose down
```

### Strategy 4: Kubernetes

**Best for**: Large-scale deployments, high availability

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crud-generator
spec:
  replicas: 3
  selector:
    matchLabels:
      app: crud-generator
  template:
    metadata:
      labels:
        app: crud-generator
    spec:
      containers:
      - name: app
        image: myregistry/dotnet-crud-generator:1.0
        env:
        - name: DATABASE_PATH
          value: /data/app.db
        volumeMounts:
        - name: data
          mountPath: /data
      volumes:
      - name: data
        persistentVolumeClaim:
          claimName: crud-generator-pvc
```

## Configuration Management

### Environment Variables

```bash
# Database path
export DATABASE_PATH="/var/lib/crudgen/app.db"

# Connection timeout
export CONNECTION_TIMEOUT="30"

# Cache settings
export CACHE_ENABLED="true"
export CACHE_EXPIRATION="60"

# Logging
export LOG_LEVEL="Information"
```

### appsettings.Production.json

```json
{
  "DatabaseSettings": {
    "FilePath": "/var/lib/crudgen/app.db",
    "ConnectionTimeout": 30,
    "MaxPoolSize": 50
  },
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 120,
    "SlidingExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  }
}
```

## Database Backup

### Automated Backup

```bash
#!/bin/bash
# backup.sh

BACKUP_DIR="/var/backups/crudgen"
DB_PATH="/var/lib/crudgen/app.db"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR

# Backup database
cp $DB_PATH $BACKUP_DIR/app_$DATE.db

# Compress
gzip $BACKUP_DIR/app_$DATE.db

# Keep last 7 days
find $BACKUP_DIR -name "app_*.db.gz" -mtime +7 -delete

echo "Backup completed: app_$DATE.db.gz"
```

Schedule with cron:
```bash
0 2 * * * /usr/local/bin/backup.sh
```

### Backup Verification

```bash
# Check database integrity
sqlite3 /var/lib/crudgen/app.db "PRAGMA integrity_check;"

# Export schema
sqlite3 /var/lib/crudgen/app.db ".dump" > schema.sql
```

## Monitoring

### Health Check

```csharp
public async Task HealthCheck()
{
    var healthDb = new DatabaseConnection(connectionString);
    try
    {
        var connection = await healthDb.GetConnectionAsync();
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT 1";
            await cmd.ExecuteScalarAsync();
        }
        Console.WriteLine("✓ Health check passed");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Health check failed: {ex.Message}");
        return false;
    }
}
```

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    },
    "Console": {
      "IncludeScopes": true
    },
    "File": {
      "Path": "/var/log/crudgen/app.log",
      "RollingInterval": "Day",
      "RollingFileCountLimit": 7
    }
  }
}
```

### Metrics

Monitor key metrics:
- Response time
- Database connection count
- Cache hit ratio
- Error rate
- Audit log entries

```csharp
public class Metrics
{
    public TimeSpan AverageResponseTime { get; set; }
    public int ActiveConnections { get; set; }
    public decimal CacheHitRatio { get; set; }
    public int ErrorCount { get; set; }
}
```

## Performance Optimization

### Database Optimization

```sql
-- Create indexes for frequently queried columns
CREATE INDEX idx_user_email ON User(Email);
CREATE INDEX idx_product_category ON Product(CategoryId);
CREATE INDEX idx_order_user ON "Order"(UserId);

-- Analyze query performance
EXPLAIN QUERY PLAN SELECT * FROM User WHERE Email = 'test@example.com';
```

### Connection Pool Tuning

```json
{
  "DatabaseSettings": {
    "MaxPoolSize": 50,
    "MinPoolSize": 10,
    "ConnectionTimeout": 30,
    "IdleTimeout": 300
  }
}
```

### Cache Optimization

```json
{
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 60,
    "SlidingExpirationMinutes": 30,
    "MaxCacheSize": 100000
  }
}
```

## Security Hardening

### File Permissions

```bash
# Set restrictive permissions
chmod 600 /var/lib/crudgen/app.db
chmod 700 /var/lib/crudgen/

# Set ownership
chown crudgen:crudgen /var/lib/crudgen/
```

### Connection Security

```csharp
// Use parameterized queries (built-in)
var cmd = connection.CreateCommand();
cmd.CommandText = "SELECT * FROM User WHERE Email = @email";
cmd.Parameters.AddWithValue("@email", email);

// Avoid SQL injection
```

### Secrets Management

```bash
# Store sensitive data
export DB_PASSWORD=$(cat /run/secrets/db_password)
export API_KEY=$(cat /run/secrets/api_key)

# Use environment variables (never hardcode)
```

## Scaling Considerations

### Horizontal Scaling

For multiple instances:

```yaml
# Load balancer configuration
upstream crud_generator {
    server app1.local:5000;
    server app2.local:5000;
    server app3.local:5000;
}

server {
    listen 80;
    location / {
        proxy_pass http://crud_generator;
    }
}
```

### Vertical Scaling

Increase server resources:
- CPU: More parallel processing
- RAM: Larger cache, more connections
- Disk: Better I/O performance

### Data Sharding

For large datasets:

```csharp
// Partition by user ID range
public class UserDataShardingService
{
    private readonly Dictionary<int, DatabaseConnection> _shards;
    
    public DatabaseConnection GetShard(int userId)
    {
        int shardId = userId % _shards.Count;
        return _shards[shardId];
    }
}
```

## Disaster Recovery

### Backup Strategy

- **Frequency**: Daily backups
- **Retention**: 30 days
- **Location**: Off-site (cloud storage)
- **Verification**: Test restore monthly

### Recovery Procedure

```bash
#!/bin/bash
# restore.sh

BACKUP_FILE=$1
DB_PATH="/var/lib/crudgen/app.db"

# Stop application
systemctl stop crudgen

# Restore from backup
gunzip < $BACKUP_FILE > $DB_PATH

# Verify restoration
sqlite3 $DB_PATH "PRAGMA integrity_check;"

# Start application
systemctl start crudgen
```

### Failover Configuration

```yaml
# High availability setup
Primary Database
    ↓
    Replication to Standby
    ↓
Failover (if primary down)
    ↓
Standby becomes Primary
    ↓
Reconnect clients
```

## System Requirements

### Minimum

- CPU: 1 core
- RAM: 512 MB
- Disk: 10 GB
- Network: 10 Mbps

### Recommended

- CPU: 4 cores
- RAM: 8 GB
- Disk: 100 GB SSD
- Network: 1 Gbps

### Production

- CPU: 8+ cores
- RAM: 16+ GB
- Disk: 500+ GB SSD (RAID 1)
- Network: 10+ Gbps

## Maintenance

### Regular Tasks

```bash
# Daily
sqlite3 /var/lib/crudgen/app.db "PRAGMA optimize;"
sqlite3 /var/lib/crudgen/app.db "PRAGMA wal_checkpoint(TRUNCATE);"

# Weekly
/usr/local/bin/backup.sh

# Monthly
sqlite3 /var/lib/crudgen/app.db "VACUUM;"
```

### Updates

```bash
# Test update in staging
dotnet publish -c Release

# Create backup before update
cp /var/lib/crudgen/app.db /var/lib/crudgen/app.db.backup

# Deploy update
systemctl stop crudgen
cp new_binaries/* /opt/crudgen/
systemctl start crudgen

# Verify
curl http://localhost:5000/health
```

## Troubleshooting

### Application Won't Start

```bash
# Check logs
tail -f /var/log/crudgen/app.log

# Verify database
sqlite3 /var/lib/crudgen/app.db ".tables"

# Check permissions
ls -la /var/lib/crudgen/
```

### Performance Issues

```bash
# Monitor system resources
top -b -n 1 | head -20

# Check database file size
du -sh /var/lib/crudgen/app.db

# Analyze slow queries
PRAGMA query_only = OFF;
EXPLAIN QUERY PLAN SELECT * FROM User WHERE ...
```

### Database Corruption

```bash
# Check integrity
sqlite3 /var/lib/crudgen/app.db "PRAGMA integrity_check;"

# Recover if possible
sqlite3 /var/lib/crudgen/app.db "PRAGMA recovery_mode = ON; PRAGMA integrity_check;"

# Restore from backup
/usr/local/bin/restore.sh /var/backups/crudgen/app_20240101_020000.db.gz
```
