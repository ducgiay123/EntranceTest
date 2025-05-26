# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY src/AuthenticationTest.Core/*.csproj AuthenticationTest.Core/
COPY src/AuthenticationTest.Service/*.csproj AuthenticationTest.Service/
COPY src/AuthenticationTest.Controller/*.csproj AuthenticationTest.Controller/
COPY src/AuthenticationTest.Infrastructure/*.csproj AuthenticationTest.Infrastructure/

# Restore dependencies
RUN dotnet restore AuthenticationTest.Infrastructure/AuthenticationTest.Infrastructure.csproj
RUN dotnet restore AuthenticationTest.Service/AuthenticationTest.Service.csproj
RUN dotnet restore AuthenticationTest.Controller/AuthenticationTest.Controller.csproj

# Copy the full source code
COPY src/ .

# Build the application
WORKDIR /src/AuthenticationTest.Infrastructure
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish



# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks (optional)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose port
EXPOSE 5000

# Set entry point
ENTRYPOINT ["dotnet", "AuthenticationTest.Infrastructure.dll"]