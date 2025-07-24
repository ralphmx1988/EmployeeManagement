#!/bin/bash

# Cruise Ship Container Deployment Script
# This script deploys the Employee Management System to a cruise ship

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
SHIP_ID="${1:-Ship001}"
SHIP_NAME="${2:-Cruise Ship Alpha}"

echo "🚢 Deploying Employee Management System to $SHIP_NAME ($SHIP_ID)"

# Check prerequisites
check_prerequisites() {
    echo "📋 Checking prerequisites..."
    
    if ! command -v docker &> /dev/null; then
        echo "❌ Docker is not installed"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        echo "❌ Docker Compose is not installed"
        exit 1
    fi
    
    echo "✅ Prerequisites check passed"
}

# Create ship-specific configuration
create_ship_config() {
    echo "⚙️ Creating ship-specific configuration..."
    
    # Create .env file from template
    cp "$PROJECT_ROOT/.env.template" "$PROJECT_ROOT/.env"
    
    # Update ship-specific values
    sed -i "s/SHIP_ID=Ship001/SHIP_ID=$SHIP_ID/" "$PROJECT_ROOT/.env"
    sed -i "s/SHIP_NAME=\"Cruise Ship Alpha\"/SHIP_NAME=\"$SHIP_NAME\"/" "$PROJECT_ROOT/.env"
    
    # Create config directory
    mkdir -p "$PROJECT_ROOT/config"
    mkdir -p "$PROJECT_ROOT/logs"
    mkdir -p "$PROJECT_ROOT/database/backups"
    mkdir -p "$PROJECT_ROOT/database/scripts"
    
    # Create ship config JSON
    cat > "$PROJECT_ROOT/config/ship-config.json" << EOF
{
  "ship_id": "$SHIP_ID",
  "ship_name": "$SHIP_NAME",
  "services": [
    {
      "name": "employeemanagement-web",
      "image": "registry.cruiseline.com/employeemanagement:latest",
      "update_policy": "auto",
      "rollback_enabled": true
    }
  ],
  "update_window": {
    "start": "02:00",
    "end": "04:00",
    "timezone": "UTC"
  },
  "auto_update_enabled": true,
  "max_retries": 3,
  "health_check_timeout": 300
}
EOF
    
    echo "✅ Ship configuration created"
}

# Build container images
build_images() {
    echo "🔨 Building container images..."
    
    cd "$PROJECT_ROOT"
    
    # Build main application
    docker build -t "employeemanagement:local" .
    
    # Build update agent
    cd cruise-ship-tools
    docker build -f Dockerfile.update-agent -t "update-agent:local" .
    cd ..
    
    echo "✅ Container images built"
}

# Create NGINX configuration
create_nginx_config() {
    echo "🌐 Creating NGINX configuration..."
    
    mkdir -p "$PROJECT_ROOT/nginx"
    
    cat > "$PROJECT_ROOT/nginx/nginx.conf" << 'EOF'
events {
    worker_connections 1024;
}

http {
    upstream employeemanagement {
        server employeemanagement-web:8080;
    }
    
    server {
        listen 80;
        server_name _;
        
        # Health check endpoint
        location /health {
            proxy_pass http://employeemanagement/health;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
        }
        
        # Main application
        location / {
            proxy_pass http://employeemanagement;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            
            # WebSocket support for Blazor
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
        }
        
        # Static files caching
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
            proxy_pass http://employeemanagement;
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
}
EOF
    
    echo "✅ NGINX configuration created"
}

# Deploy containers
deploy_containers() {
    echo "🚀 Deploying containers..."
    
    cd "$PROJECT_ROOT"
    
    # Stop existing containers
    docker-compose -f docker-compose.cruise.yml down || true
    
    # Start new deployment
    docker-compose -f docker-compose.cruise.yml up -d
    
    echo "✅ Containers deployed"
}

# Verify deployment
verify_deployment() {
    echo "🔍 Verifying deployment..."
    
    # Wait for services to start
    sleep 30
    
    # Check container status
    if docker-compose -f docker-compose.cruise.yml ps | grep -q "Up"; then
        echo "✅ Containers are running"
    else
        echo "❌ Some containers failed to start"
        docker-compose -f docker-compose.cruise.yml logs
        exit 1
    fi
    
    # Check application health
    for i in {1..10}; do
        if curl -f http://localhost/health &> /dev/null; then
            echo "✅ Application is healthy"
            break
        fi
        echo "⏳ Waiting for application to start... ($i/10)"
        sleep 10
    done
}

# Create maintenance scripts
create_maintenance_scripts() {
    echo "🔧 Creating maintenance scripts..."
    
    mkdir -p "$PROJECT_ROOT/scripts"
    
    # Backup script
    cat > "$PROJECT_ROOT/scripts/backup.sh" << 'EOF'
#!/bin/bash
# Database backup script for cruise ship

BACKUP_DIR="/var/opt/mssql/backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="EmployeeManagement_${DATE}.bak"

echo "Creating database backup: $BACKUP_FILE"

docker exec employeemanagement-db /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "${DB_PASSWORD}" \
    -Q "BACKUP DATABASE EmployeeManagement TO DISK = '/var/opt/mssql/backups/$BACKUP_FILE'"

echo "Backup completed: $BACKUP_FILE"
EOF
    
    # Log rotation script
    cat > "$PROJECT_ROOT/scripts/rotate-logs.sh" << 'EOF'
#!/bin/bash
# Log rotation script for cruise ship

LOG_DIR="./logs"
MAX_SIZE="100M"

find "$LOG_DIR" -name "*.log" -size +$MAX_SIZE -exec gzip {} \;
find "$LOG_DIR" -name "*.log.gz" -mtime +7 -delete

echo "Log rotation completed"
EOF
    
    chmod +x "$PROJECT_ROOT/scripts"/*.sh
    
    echo "✅ Maintenance scripts created"
}

# Main deployment flow
main() {
    echo "🚢 Starting Cruise Ship Deployment for $SHIP_NAME ($SHIP_ID)"
    echo "=================================================="
    
    check_prerequisites
    create_ship_config
    create_nginx_config
    build_images
    deploy_containers
    verify_deployment
    create_maintenance_scripts
    
    echo ""
    echo "🎉 Deployment completed successfully!"
    echo ""
    echo "📊 Services:"
    echo "   • Web Application: http://localhost"
    echo "   • Health Monitor: http://localhost:9090"
    echo "   • Database: localhost:1433"
    echo ""
    echo "📁 Important directories:"
    echo "   • Configuration: ./config/"
    echo "   • Logs: ./logs/"
    echo "   • Database backups: ./database/backups/"
    echo "   • Maintenance scripts: ./scripts/"
    echo ""
    echo "🔧 Management commands:"
    echo "   • View logs: docker-compose -f docker-compose.cruise.yml logs"
    echo "   • Stop services: docker-compose -f docker-compose.cruise.yml down"
    echo "   • Restart services: docker-compose -f docker-compose.cruise.yml restart"
    echo "   • Backup database: ./scripts/backup.sh"
    echo ""
}

# Run main function
main "$@"
