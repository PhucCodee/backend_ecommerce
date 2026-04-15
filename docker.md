### Remove All Containers, Networks, Volumes

docker compose down -v

### Build and Start All Services

docker compose up -d --build

### Dive into the database
docker compose exec db psql -U postgres -d database

