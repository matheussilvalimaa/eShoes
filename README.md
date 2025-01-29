# eShoes 

**eShoes** is a backend project for an hypothetical eCommerce called eShoes, built with ASP.NET Core 8.0 and PostgreSQL. It provides comprehensive functionalities for managing products, user authentication, shopping carts, orders, and payments integrated with Stripe. Containerized using Docker and orchestrated with Docker Compose, the project ensures easy deployment and consistent environments across different platforms.

## ✨ Features

- **User Management:**
  - User registration and login with JWT-based authentication.
  - Role-based authorization.
  - Profile management and logout functionality.

- **Product Management:**
  - CRUD operations for products.
  - Inventory management with stock updates.

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

- **Backend:** ASP.NET Core 8.0
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

##  📦 Docker Configuration

The project uses Docker and Docker Compose to manage services. Here's a brief overview of the configuration files:

**docker-compose.yml**

Defines two main services:

1. **PostgreSQL Database**
- Image: postgres:15.
- Environment Variables:
  - POSTGRES_USER
  - POSTGRES_PASSWORD
  - POSTGRES_DB
- Ports: 5432:5432.
- Volumes: Persist data using Docker volumes.
- Healthcheck: Ensures the database is completely ready before starting dependent services.

2. **eShoes application**
- Build: Uses the provided Dockerfile.
- Environment Variables: Loaded from the .env file.
- Ports: 8080:80.
- Depends On: Waits for the PostgreSQL service to be healthy.

**Dockerfile**

A multi-stage Dockerfile that:

1. **Build Stage**
- Uses the .NET SDK to restore dependencies and publish the application.
- Optimizes caching by copying .csproj files first.

2. **Runtime Stage**
- Uses the lighweight ASP.NET Core runtime image.
- Copies published artifacts form the build stage.
- Includes an entrypoint.sh script to apply database migrations before starting the application.

## 🛠️ Installation

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/matheussilvalima/eShoes.git
   cd eShoes

2. **Set Up Environment Variables:**

Duplicate the .env.example file and rename it to .env, and then configure the necessary variables:

```bash
  cp .env.example .env

  //.env
  POSTGRES_USER=eshoesUser
  POSTGRES_PASSWORD=eshoesPass
  POSTGRES_DB=eshoesdb

  CONNECTION_STRING=Host=postgres;Port=5432;Database=eshoesdb;Username=eshoesUser;Password=eshoesPass
  JWT_SECRET_KEY=your_jwt_secret_key
  STRIPE_SECRET_KEY=your_stripe_secret_key
  STRIPE_PUBLISHABLE_KEY=your_stripe_publishable_key
```

## 🏃 Running the Application

**Running with Docker**

With Docker and Docker Compose installed, follow these steps to run the application:

1. **Build and Start Services**
```bash
  docker-compose up -d --build
```

2. **Verify Running Containers**
```bash
  docker-compose ps
```

3. **Monitor Logs**
```bash
  docker-compose logs -f
```

4. **Access the API**
- Swagger UI: http://localhost:5032/swagger

5. **Stopping the Services**
```bash
  docker-compose down
```

**Running Locally without Docker**

You can run locally without Docker:

1. **Install [PostgreSQL](https://www.postgresql.org/download/) and execute it.**

2. **Copy the .env.example to .env:**
```bash
  cp .env.example .env
```

3. **Edit the .env file with your configurations**

4. **Apply your migrations**
```bash
  dotnet tool install --global dotnet-ef
  dotnet ef migrations add InitialCreate
  dotnet ef database update
```

5. **Start the application**
```bash
  dotnet run
```


## 📝 Usage

1. **Accessing Swagger UI:**

- *Open your Browser:*
    - Navigate to http://localhost:5032/swagger to access the API test environment.

- *Explore the Endpoints*
    - Browse through the available controllers and endpoints. You can test each endpoint directly from the Swagger UI by providing the necessary parameters and request bodies.

2. **Authentication**

- *Register a New User:*
    - Use the endpoint POST /api/user/register.
    - Provide Name, Email and Password in the request body.

- *Login*
    - Use the endpoint POST /api/user/login
    - Provide the Email and Password that you had created to receive a JWT token.

- *Authorize in Swagger*
    - Click on the "Authorize" button in the Swagger UI.
    - Enter the JWT token in the format: Bearer {your_jwt_token}.
    - Click "Authorize" and then "Close".

- *Access Protected Endpoints*
    - With the token authorized, you can now access endpoints that require authentication or specific roles.

3. **Testing the API**

- *Select an Endpoint*
    - Expand the desired controller.
    - Click on the endpoint you wish to test.

- *Provide Required Parameters*
    - For POST requests, fill in the required JSON body with appropriate data.
    - For endpoints with path parameters, provide the necessary ID.

- *Execute the Request*
    - Click the "Try it out" button.
    - After filling in the parameters, click the "Execute" button.
    - Observe the response returned by the API in the "Responses" section below.

## 🤝 Contributing

Contributions are welcome! Feel free to fork the project and contribute.



