#
# Stage 1: Build & Publish
#
# Use the .NET 8.0 SDK image, as specified in your tech stack [cite: 856]
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all .csproj files and restore dependencies first
# This leverages Docker's layer caching. 'dotnet restore' only re-runs
# if a .csproj file changes, not on every code change.
COPY src/ECommerce.API/ECommerce.API.csproj src/ECommerce.API/
COPY src/ECommerce.Application/ECommerce.Application.csproj src/ECommerce.Application/
COPY src/ECommerce.Domain/ECommerce.Domain.csproj src/ECommerce.Domain/
COPY src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj src/ECommerce.Infrastructure/
# If you have a .sln file in the root, you can copy and restore it instead:
# COPY ECommerce.sln .
# RUN dotnet restore "ECommerce.sln"

# Restore the API project (which restores its dependencies)
RUN dotnet restore "src/ECommerce.API/ECommerce.API.csproj"

# Copy the rest of the source code
COPY src/ ./src/

# Publish the application
RUN dotnet publish "src/ECommerce.API/ECommerce.API.csproj" -c Release -o /app/publish

#
# Stage 2: Final Runtime Image
#
# Use the lightweight ASP.NET 8.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set the container to listen on port 8080.
# ASP.NET Core images default to 8080 (HTTP) and 8081 (HTTPS).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Entry point for the container
ENTRYPOINT ["dotnet", "ECommerce.API.dll"]