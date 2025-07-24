#!/usr/bin/env python3
"""
Cruise Ship Container Update Agent

This service checks for container updates when internet connectivity is available
and applies them during maintenance windows.
"""

import os
import sys
import time
import json
import logging
import subprocess
import schedule
import docker
import requests
from datetime import datetime, timedelta
from typing import Dict, List, Optional

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('/app/logs/update-agent.log'),
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger('UpdateAgent')

class CruiseShipUpdateAgent:
    def __init__(self):
        self.ship_id = os.getenv('SHIP_ID', 'Ship001')
        self.registry_url = os.getenv('REGISTRY_URL', 'registry.cruiseline.com')
        self.update_interval = int(os.getenv('UPDATE_CHECK_INTERVAL', '3600'))
        self.docker_client = docker.from_env()
        self.config_path = '/app/config'
        self.last_update_check = None
        
        # Load configuration
        self.load_config()
        
    def load_config(self) -> Dict:
        """Load ship-specific configuration"""
        config_file = f"{self.config_path}/ship-config.json"
        try:
            with open(config_file, 'r') as f:
                self.config = json.load(f)
                logger.info(f"Loaded configuration for {self.ship_id}")
        except FileNotFoundError:
            logger.warning(f"Configuration file not found: {config_file}")
            self.config = self.get_default_config()
            self.save_config()
        except Exception as e:
            logger.error(f"Error loading configuration: {e}")
            self.config = self.get_default_config()
    
    def get_default_config(self) -> Dict:
        """Get default configuration"""
        return {
            "ship_id": self.ship_id,
            "services": [
                {
                    "name": "employeemanagement-web",
                    "image": "registry.cruiseline.com/employeemanagement:latest",
                    "update_policy": "auto",
                    "rollback_enabled": True
                }
            ],
            "update_window": {
                "start": "02:00",
                "end": "04:00",
                "timezone": "UTC"
            },
            "auto_update_enabled": True,
            "max_retries": 3,
            "health_check_timeout": 300
        }
    
    def save_config(self):
        """Save current configuration"""
        config_file = f"{self.config_path}/ship-config.json"
        os.makedirs(self.config_path, exist_ok=True)
        try:
            with open(config_file, 'w') as f:
                json.dump(self.config, f, indent=2)
            logger.info(f"Configuration saved to {config_file}")
        except Exception as e:
            logger.error(f"Error saving configuration: {e}")
    
    def check_internet_connectivity(self) -> bool:
        """Check if internet connection is available"""
        try:
            response = requests.get(
                f"https://{self.registry_url}/v2/",
                timeout=10,
                headers={'User-Agent': f'CruiseShip-UpdateAgent/{self.ship_id}'}
            )
            return response.status_code == 200
        except Exception as e:
            logger.debug(f"Internet connectivity check failed: {e}")
            return False
    
    def is_in_update_window(self) -> bool:
        """Check if current time is within the update window"""
        now = datetime.now()
        start_time = datetime.strptime(self.config["update_window"]["start"], "%H:%M").time()
        end_time = datetime.strptime(self.config["update_window"]["end"], "%H:%M").time()
        
        current_time = now.time()
        
        if start_time <= end_time:
            return start_time <= current_time <= end_time
        else:  # Window crosses midnight
            return current_time >= start_time or current_time <= end_time
    
    def check_for_updates(self):
        """Check for available container updates"""
        if not self.check_internet_connectivity():
            logger.info("No internet connectivity - skipping update check")
            return
        
        logger.info("Checking for container updates...")
        self.last_update_check = datetime.now()
        
        for service in self.config["services"]:
            try:
                self.check_service_update(service)
            except Exception as e:
                logger.error(f"Error checking updates for {service['name']}: {e}")
    
    def check_service_update(self, service: Dict):
        """Check for updates for a specific service"""
        service_name = service["name"]
        current_image = service["image"]
        
        try:
            # Get current container
            container = self.docker_client.containers.get(service_name)
            current_image_id = container.image.id
            
            # Pull latest image
            logger.info(f"Pulling latest image for {service_name}")
            latest_image = self.docker_client.images.pull(current_image)
            
            # Compare image IDs
            if current_image_id != latest_image.id:
                logger.info(f"Update available for {service_name}")
                
                if service.get("update_policy") == "auto" and self.config.get("auto_update_enabled"):
                    if self.is_in_update_window():
                        self.apply_service_update(service, latest_image)
                    else:
                        logger.info(f"Update scheduled for {service_name} during maintenance window")
                        self.schedule_update(service, latest_image)
                else:
                    logger.info(f"Manual update required for {service_name}")
            else:
                logger.info(f"No updates available for {service_name}")
                
        except docker.errors.NotFound:
            logger.error(f"Container {service_name} not found")
        except Exception as e:
            logger.error(f"Error checking service update for {service_name}: {e}")
    
    def apply_service_update(self, service: Dict, new_image):
        """Apply update to a service"""
        service_name = service["name"]
        logger.info(f"Applying update to {service_name}")
        
        try:
            # Get current container
            container = self.docker_client.containers.get(service_name)
            old_image_id = container.image.id
            
            # Create backup of current container
            backup_name = f"{service_name}_backup_{int(time.time())}"
            
            # Stop current container
            logger.info(f"Stopping {service_name}")
            container.stop(timeout=30)
            
            # Rename current container as backup
            container.rename(backup_name)
            
            # Start new container with updated image
            logger.info(f"Starting new {service_name} with updated image")
            
            # Get container configuration
            config = container.attrs['Config']
            host_config = container.attrs['HostConfig']
            networking_config = container.attrs['NetworkSettings']
            
            # Create new container
            new_container = self.docker_client.containers.run(
                new_image.id,
                name=service_name,
                detach=True,
                **self.extract_container_config(config, host_config, networking_config)
            )
            
            # Health check
            if self.verify_service_health(service_name):
                logger.info(f"Successfully updated {service_name}")
                # Remove backup container
                backup_container = self.docker_client.containers.get(backup_name)
                backup_container.remove()
                # Remove old image
                try:
                    self.docker_client.images.remove(old_image_id)
                except:
                    pass
            else:
                logger.error(f"Health check failed for {service_name} - rolling back")
                self.rollback_service(service_name, backup_name)
                
        except Exception as e:
            logger.error(f"Error applying update to {service_name}: {e}")
            self.rollback_service(service_name, backup_name)
    
    def extract_container_config(self, config: Dict, host_config: Dict, networking_config: Dict) -> Dict:
        """Extract container configuration for recreation"""
        return {
            'environment': config.get('Env', []),
            'ports': host_config.get('PortBindings', {}),
            'volumes': host_config.get('Binds', []),
            'network_mode': host_config.get('NetworkMode'),
            'restart_policy': host_config.get('RestartPolicy', {}),
            'command': config.get('Cmd'),
            'entrypoint': config.get('Entrypoint'),
            'working_dir': config.get('WorkingDir'),
            'user': config.get('User')
        }
    
    def verify_service_health(self, service_name: str, timeout: int = 300) -> bool:
        """Verify service health after update"""
        logger.info(f"Performing health check for {service_name}")
        
        start_time = time.time()
        while time.time() - start_time < timeout:
            try:
                container = self.docker_client.containers.get(service_name)
                if container.status == 'running':
                    # Additional health checks can be added here
                    # For now, just check if container is running
                    time.sleep(10)  # Wait a bit for service to stabilize
                    return True
                time.sleep(5)
            except Exception as e:
                logger.debug(f"Health check error: {e}")
                time.sleep(5)
        
        return False
    
    def rollback_service(self, service_name: str, backup_name: str):
        """Rollback service to previous version"""
        logger.info(f"Rolling back {service_name}")
        
        try:
            # Stop failed container
            try:
                failed_container = self.docker_client.containers.get(service_name)
                failed_container.stop(timeout=10)
                failed_container.remove()
            except:
                pass
            
            # Restore backup
            backup_container = self.docker_client.containers.get(backup_name)
            backup_container.rename(service_name)
            backup_container.start()
            
            logger.info(f"Successfully rolled back {service_name}")
            
        except Exception as e:
            logger.error(f"Error during rollback of {service_name}: {e}")
    
    def schedule_update(self, service: Dict, new_image):
        """Schedule update for maintenance window"""
        # This is a simplified implementation
        # In production, you might want to use a more sophisticated scheduler
        update_time = self.config["update_window"]["start"]
        schedule.every().day.at(update_time).do(
            self.apply_service_update, service, new_image
        ).tag(f"update_{service['name']}")
    
    def run(self):
        """Main update agent loop"""
        logger.info(f"Starting Cruise Ship Update Agent for {self.ship_id}")
        
        # Schedule regular update checks
        schedule.every(self.update_interval).seconds.do(self.check_for_updates)
        
        # Initial update check
        self.check_for_updates()
        
        # Main loop
        while True:
            try:
                schedule.run_pending()
                time.sleep(60)  # Check every minute
            except KeyboardInterrupt:
                logger.info("Update agent stopped by user")
                break
            except Exception as e:
                logger.error(f"Error in main loop: {e}")
                time.sleep(60)

if __name__ == "__main__":
    agent = CruiseShipUpdateAgent()
    agent.run()
