## 📖 About 
A development sandbox to play around with Keycloak as an IDP/IAM with a ASP.NET Web API and a WPF client application.

## 💻 Prerequisites
*  Podman/Docker to run the Keycloak/Postgres containers

## 🚀 Getting Started
*  Generate Keycloak certificates by running the `./Keycloak/generate-certificate.ps1` script.  This script will generate a new self signed certificate and automatically trust the certificate.
*  Start the Keycloak container and navigate to [https://localhost:8443](https://localhost:8443), login with admin/admin, Clients -> "security-admin-console".  Add a valid redirect URI of `http://localhost`.
*  Run the KeycloakConfigurationUtility program to setup and configure the application realm.

## 🗝️ Keycloak Startup/Shutdown
1.  Start the Keycloak and Postgres containers by executing the compose script (`./Keycloak/docker-compose.yml`) with `podman compose up -d`.  You can now access the Keycloak instance with a webbrowser by navigating to [https://localhost:8443](https://localhost:8443) and using the default credentials of admin/admin.
2.  To stop the containers execute `podman compose down`