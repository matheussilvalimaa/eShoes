# 👟 eShoes 

**eShoes** is a full-stack e-commerce sample project built with .NET 8, Entity Framework Core, PostgreSQL, and Stripe. It features JWT-based authentication, CRUD operations for products, carts, orders, and integrates Stripe as a payment gateway. The front-end is a simple HTML/CSS/JavaScript Spa using the Live Server extension for local HTTPS.

## 📂 Repository Structure

```bash
├── backend/ # ASP.NET Core Web API
│ ├── Controllers/
│ ├── Models/
│   ├──── DTO/
│ ├── Services/
│ ├── Migrations/
│ ├── appsettings.json
│ ├── Program.cs
│ ├── Dockerfile
│ └── docker-compose.yml
├── frontend/ # Static HTML/CSS/JS pages
│ ├── Pages/
│ │ ├── Home/
│ │ ├── Products/
│ │ ├── Cart/
│ │ ├── images/
│ │ ├── Account/
│ │ └── About/
│ └── settings.json # VS Code Live Server config
├── .env.example
```

## ✨ Features

- **User Management:**
  - User registration and login with JWT-based authentication.
  - Profile management and logout functionality.
    
- **Shopping Cart:**
  - Add, remove, and clear products in the cart.
  - Retrieve all items in the current user's cart.

- **Order Processing:**
  - Create and retrieve orders based on cart items.
  - List all orders for administrative purposes.

- **Payment Integration:**
  - Create Stripe Payment Intents.

- **Database Migrations:**
  - Managed using Entity Framework Core Migrations for schema evolution.

## 🚀 Technology Stack

- **Backend:** ASP.NET Core 8.0, Kestrel HTTPS
- **Frontend:** HTML, CSS, Vanilla JavaScript, Live Server
- **Database:** PostgreSQL 
- **ORM:** Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)
- **Payment Processing:** Stripe API
- **Containerization:** Docker
- **Orchestration:** Docker Compose

## 🔧 Prerequisites

The prerequisites to run the project are:

- [Docker](https://www.docker.com/get-started) (includes Docker Compose)
- [Git](https://git-scm.com/downloads)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) *(for local development without Docker)*
- [VS Code](https://code.visualstudio.com/) (for front-end Live Server)

## 🛠️ Backend Setup

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/matheussilvalima/eShoes.git
   cd eShoes

2. **Set Up Environment Variables:**

Duplicate the .env.example file and rename it to .env:

```bash
  cp .env.example .env

  //.env
  POSTGRES_USER=postgres
  POSTGRES_PASSWORD=postgres
  POSTGRES_DB=eshoesdb

  CONNECTION_STRING=Host=postgres;Port=5432;Database=eshoesdb;Username=postgres;Password=postgres
  JWT_SECRET_KEY=your_jwt_secret_key
  STRIPE_SECRET_KEY=your_stripe_secret_key
```

3. **Generate HTTPS Dev Certificate:**

```bash
  dotnet dev-certs https --trust
```
Required for Kestrel to serve HTTPS locally.

4. **Start Docker-Compose and run the API**

```bash
  docker-compose up -d
  cd backend
  dotnet run
```

## 🌐 Front-end Setup (VS Code + Live Server)

Open eShoesFrontend/Pages in VSCode and install the [Live-Server](https://marketplace.visualstudio.com/items?itemName=ritwickdey.LiveServer).

1. **Generate HTTPS Local Certs in PowerShell**

```powershell
  New-SelfSignedCertificate -DnsName localhost -CertStoreLocation "cert:\LocalMachine\My"
  Export-PfxCertificate -Cert "cert:\LocalMachine\My\<THUMBPRINT>" -FilePath cert.pfx -Password (ConvertTo-SecureString -String "" -Force -AsPlainText)
  openssl pkcs12 -in cert.pfx -nocerts -out key.pem -nodes
  openssl pkcs12 -in cert.pfx -clcerts -nokeys -out cert.pem
```
Place cert.pem and key.pem under eShoesFrontend/Pages/certificates/

2. **Add to .vscode/settings.json**

```json
  {
    "liveServer.settings.https": {
      "enable": true,
      "cert": "${workspaceFolder}/eShoesFrontend/Pages/certificates/cert.pem",
      "key": "${workspaceFolder}/eShoesFrontend/Pages/certificates/key.pem",
      "passphrase": ""
    },
    "liveServer.settings.host": "localhost"
  }
```

3. **Start the Live Server**

Right-click index.html → Open with Live Server
Browse: https://localhost:5500/Pages/Home/index.html

## 🤝 Contributing

Contributions are welcome! Feel free to fork the project and contribute.

## 📝 License

This project is for demonstration & portfolio purposes.
Feel free to explore and adapt as needed!



