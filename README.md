# EntranceTest - Full Stack Project

## Technologies Used

- **Frontend**: ReactJS (JavaScript, Antd)
- **Backend**: .NET Core (C#, ASP.NET Core)
- **Database**: PostgreSQL
- **Containerization**: Docker, Docker Compose
- **Version Control**: Git, GitHub

## Prerequisites

Before you begin, ensure you have the following installed:

- Docker and Docker Compose
- Git
- A code editor like Visual Studio Code

## Setup and Installation

Follow these steps to clone and set up the project locally:

**Clone the Repository**

```bash
git clone https://github.com/ducgiay123/EntranceTest.git
cd EntranceTest
```

## Verify Project Structure

Ensure the repository contains the following key files and directories:

- authentication-test/: Contains the ReactJS application
- AuthenticationTestDotNet/: Contains the .NET Core application
- docker-compose.yml: Defines services for frontend, backend, and PostgreSQL

## Running the Project

The project uses Docker Compose to manage the frontend, backend, and database services. Follow these steps to build and run the application:

1. **Build the Docker Images**

```bash
docker-compose up --build
```

This will:

- Build the .NET Core API
- Build the React frontend
- Start PostgreSQL

And wire everything together

2. **Access the App**

- Frontend: http://localhost:3000

- Backend API: http://localhost:5000

3. **Restart Without Rebuilding**

If everything is already built, just run

```bash
docker-compose up -d
```

4. **Stop the App**

To stop and remove all containers, networks, and volumes:

```bash
docker-compose down
```

To stop the app without removing containers (can restart later):

```bash
docker-compose stop
```

To restart the app after stopping:

```bash
docker-compose start

```

## 📚 Swagger API Documentation

The backend API is documented using **Swagger**, making it easy to explore and test endpoints.

### 🔧 How to Access Swagger

After starting the backend server (either via Docker Compose or manually), you can access Swagger UI at:
http://localhost:5000/swagger

> Make sure the backend is running before accessing the above URL.

### ✅ What You Can Do with Swagger

- View all available API endpoints (GET, POST, PUT, DELETE)
- Test API calls directly from the browser
- See request/response schema and status codes
- Try authenticated requests by adding a Bearer token

### 🛡 Adding Authorization

To test protected endpoints:

- Click the **Authorize** button on the top right of the Swagger page.
- Enter your JWT token like this: Bearer YOUR_JWT_TOKEN_HERE
- Click "Authorize" to authenticate your session.
  You can obtain a token by logging in via the frontend or using a login API endpoint.
