docker network create mvcalloptions --label=mvcalloptions
docker-compose -f containers/rabbitmq.yml up -d
docker-compose -f containers/redis.yml up -d
exit $LASTEXITCODE