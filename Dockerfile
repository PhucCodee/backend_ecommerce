# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy central package/build props first so restore can resolve package versions correctly
COPY Directory.Packages.props ./
COPY Directory.Build.props ./

# Copy project files for restore layer caching
COPY src/ECommerce.API/ECommerce.API.csproj src/ECommerce.API/
COPY src/ECommerce.Application/ECommerce.Application.csproj src/ECommerce.Application/
COPY src/ECommerce.Domain/ECommerce.Domain.csproj src/ECommerce.Domain/
COPY src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj src/ECommerce.Infrastructure/

RUN dotnet restore src/ECommerce.API/ECommerce.API.csproj

# Copy the rest of the source
COPY src/ ./src/

RUN dotnet publish src/ECommerce.API/ECommerce.API.csproj -c Release -o /app/publish --no-restore

# Stage 2: Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y postgresql-client \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN sed -i 's/\r$//' /docker-entrypoint.sh && chmod +x /docker-entrypoint.sh

COPY wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["/docker-entrypoint.sh"]