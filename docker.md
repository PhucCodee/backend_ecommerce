docker-compose up -d
docker-compose up -d --build

docker compose exec db psql -U postgres -d database