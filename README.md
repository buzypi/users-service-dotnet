Run the users-service with the following commands:

```bash
docker build -t users-service:v1 .
docker network create mynet1
docker run -d --name users-db --net mynet1 mongo
docker run -d --name users-service --net mynet1 -p 80:8080 -e DB_HOST="mongodb://users-db" users-service:v1
```
