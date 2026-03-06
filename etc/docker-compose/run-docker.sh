#!/bin/bash

if [[ ! -d certs ]]
then
    mkdir certs
    cd certs/
    if [[ ! -f localhost.pfx ]]
    then
        dotnet dev-certs https -v -ep localhost.pfx -p 37c024b4-4e55-478e-9c50-63df6b03942c -t
    fi
    cd ../
fi

docker-compose up -d
