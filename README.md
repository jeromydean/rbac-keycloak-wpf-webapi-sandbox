## Prerequisites
*  Podman/Docker to run the Keycloak/Postgres containers

## Initial Setup
*  Generate Keycloak certificates by running the `./Keycloak/generate-certificate.ps1` script.  This script will generate a new self signed certificate and automatically trust the certificate.

## Keycloak Startup/Shutdown
1.  Start the Keycloak and Postgres containers by executing the compose script (`./Keycloak/docker-compose.yml`) with `podman compose up -d`.  You can now access the Keycloak instance with a webbrowser by navigating to [https://localhost:8443](https://localhost:8443)
2.  To stop the containers execute `podman compose down`