# AzureFunctionsDemo

## Overview
This repository contains an Azure Functions project designed to handle news-related email notifications. The project includes:

- A function (`NewsEmailingFunction.cs`) for sending emails
- Service dependencies for integration with Azure services
- Deployment configuration files

## Prerequisites
Before running or deploying this project, ensure you have the following:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- An active [Azure subscription](https://azure.microsoft.com/en-us/free/)

## Getting Started
### 1. Clone the Repository
```sh
git clone <repository-url>
cd AzureFunctionsDemo
```

### 2. Install Dependencies
```sh
dotnet restore
```

### 3. Run the Function Locally
```sh
dotnet build
func start
```

## Deployment
This project supports deployment to Azure using **Zip Deploy**.

### Publish to Azure
```sh
dotnet publish -c Release
func azure functionapp publish <YourFunctionAppName>
```

## Folder Structure
```
AzureFunctionsDemo/
├── NewsApi/                  # Function logic (if modularized)
├── .gitignore                # Ignored files for Git
├── AzureFunctionsDemo.csproj # Project configuration
├── Dockerfile                # Docker support
├── NewsEmailingFunction.cs   # Main function implementation
├── Program.cs                # Function startup entry point
├── host.json                 # Azure Functions host configuration
├── local.settings.json       # Local development settings (excluded from Git)
└── README.md                 # Project documentation
```

## Security Considerations
- **Do not commit sensitive files** such as `local.settings.json` and `*.pubxml`.
- Use **Azure Key Vault** to manage secrets instead of storing them in configuration files.

## Contributing
Feel free to fork this repository and submit pull requests with improvements.

## License
This project is licensed under the MIT License.

