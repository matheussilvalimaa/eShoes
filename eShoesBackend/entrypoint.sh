#!/bin/bash
set -e

echo "Applying migrations"
dotnet ef database update --project eShoes.csproj --context eShoesDbContext

echo "Iniciating the aplication"
exec "$@"
