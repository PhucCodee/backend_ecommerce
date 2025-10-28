# ECommerce Backend

This is a backend application for an eCommerce platform built using .NET and PostgreSQL. The application is structured into multiple projects, each serving a specific purpose within the overall architecture.

## Project Structure

- **src**
  - **ECommerce.API**: Contains the API layer, including controllers and middleware for handling HTTP requests and responses.
  - **ECommerce.Application**: Contains the application logic, including services and data transfer objects (DTOs).
  - **ECommerce.Domain**: Contains the domain entities and common logic shared across the application.
  - **ECommerce.Infrastructure**: Contains the data access layer, including the Entity Framework Core context and repository implementations.

- **tests**
  - **ECommerce.UnitTests**: Contains unit tests for the application services.
  - **ECommerce.IntegrationTests**: Contains integration tests for the API controllers.

## Getting Started

### Prerequisites

- .NET SDK (version 6.0 or later)
- PostgreSQL database server
- An IDE or text editor of your choice

### Setup Instructions

1. **Clone the repository**:
   ```
   git clone <repository-url>
   cd ecommerce-backend
   ```

2. **Configure the database**:
   - Create a PostgreSQL database for the application.
   - Update the connection string in `src/ECommerce.API/appsettings.json` to point to your database.

3. **Run migrations**:
   - Navigate to the `ECommerce.Infrastructure` project and run the following command to apply migrations:
   ```
   dotnet ef database update
   ```

4. **Run the application**:
   - Navigate to the `ECommerce.API` project and run:
   ```
   dotnet run
   ```

5. **Access the API**:
   - The API will be available at `http://localhost:5000` (or the configured port).

## Usage

- The API provides endpoints for user management, product management, and order processing.
- Refer to the API documentation for details on available endpoints and their usage.

## Contributing

Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License

This project is licensed under the MIT License. See the LICENSE file for details.