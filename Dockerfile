#
# Stage 1: Build & Publish
#
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all .csproj files and restore dependencies first
COPY src/ECommerce.API/ECommerce.API.csproj src/ECommerce.API/
COPY src/ECommerce.Application/ECommerce.Application.csproj src/ECommerce.Application/
COPY src/ECommerce.Domain/ECommerce.Domain.csproj src/ECommerce.Domain/
COPY src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj src/ECommerce.Infrastructure/

# Restore the API project (which restores its dependencies)
RUN dotnet restore "src/ECommerce.API/ECommerce.API.csproj"

# Copy the rest of the source code
COPY src/ ./src/

# Publish the application
RUN dotnet publish "src/ECommerce.API/ECommerce.API.csproj" -c Release -o /app/publish

#
# Stage 2: Final Runtime Image with SDK for EF tools
#
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Install postgresql-client for pg_isready command
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy published application
COPY --from=build /app/publish .

# Copy source code for EF migrations
COPY --from=build /src/src ./src

# Copy startup script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Set the container to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Use custom entrypoint
ENTRYPOINT ["/docker-entrypoint.sh"]