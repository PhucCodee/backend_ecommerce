#!/bin/bash
# filepath: /Users/tranhoangphuc/Downloads/backend/docker-entrypoint.sh

set -e

echo "Waiting for PostgreSQL to be ready..."
until pg_isready -h db -p 5432 -U postgres; do
  echo "Waiting for PostgreSQL..."
  sleep 2
done

echo "PostgreSQL is ready."
echo "Note: You should run migrations separately using 'docker-compose exec api dotnet ef database update'"

echo "Starting application..."
exec dotnet ECommerce.API.dll