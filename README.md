# UTCert Service

## Overview

The UTCert project enables schools and training centers to issue secure, verifiable certificates as NFTs on the Cardano blockchain. Eligible students receive their certificates in digital wallets, which they can share with employers via QR codes for easy verification.

## Table of Contents

1. [Features](#features)
2. [Installation](#installation)
3. [Configuration](#configuration)
4. [Usage](#usage)
5. [Contributing](#contributing)
6. [License](#license)

## Features

- **NFT-Based Certificate Issuance**: Schools can issue tamper-proof certificates as NFTs, secured on the Cardano blockchain.
- **Digital Wallet Integration**: Students store and manage certificates in digital wallets, ensuring easy access and secure storage.
- **QR Code Verification**: Students can generate QR codes for their certificates, allowing employers to quickly validate credentials.

## Installation

### Prerequisites

- .NET 6 SDK
- Azure CLI

### Steps

1. **Clone the repository**:
    ```bash
    git clone https://github.com/YourUsername/UTCert-Service.git
    cd UTCert-Service
    ```

2. **Install dependencies**:
    ```bash
    dotnet restore
    ```

3. **Set up the environment**:
    - Update the `appsettings.Development.json` file with your configuration settings.

## Configuration

Update the following settings in `appsettings.Development.json`:

- **JWTSettings**: Update JWT token settings for authentication.
- **ConnectionString**: Set your database connection string.
- **DatabaseConfig**: Set database configuration options.
- **PinataConfig**: Update API Key, ApiSecret for Pinata.
- **Cloudinary**: Update API Key, ApiSecret for Cloudinary.

```json
{
  "AppSettings": {
    "Secret": "",
    "RefreshTokenTTL": 2
  },
  "ConnectionString": "",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DatabaseConfig": {
    "TimeoutTime": 30,
    "DetailedError": true,
    "SensitiveDataLogging": true
  },
  "PinataConfig": {
    "ApiKey": "",
    "ApiSecret": "",
    "ApiUrl": "",
    "Bearer": ""
  },
  "Cloudinary": {
    "CloudName": "",
    "ApiKey": "",
    "ApiSecret": ""
  }
}
```

## Contributing

We welcome contributions to Careblock. To contribute:

1. **Fork the repository**:
    - Click the "Fork" button on the top right corner of the repository page on GitHub.

2. **Create a feature branch**:
    ```bash
    git checkout -b feature/your-feature-name
    ```

3. **Commit your changes**:
    - Make your changes and commit them with a descriptive message.
    ```bash
    git add .
    git commit -m "Add a detailed description of your changes"
    ```

4. **Push the branch**:
    ```bash
    git push origin feature/your-feature-name
    ```

5. **Open a pull request**:
    - Go to your forked repository on GitHub.
    - Click on the "Compare & pull request" button.
    - Provide a detailed description of your changes and submit the pull request.

## License

This project is licensed under the MIT License.

---

For any questions or support, please contact [utcert.contact@gmail.com](mailto:utcert.contact@gmail.com).
