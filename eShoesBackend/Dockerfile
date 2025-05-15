FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["eShoesBackend.csproj", "./"]
RUN dotnet restore "eShoesBackend.csproj"

COPY . .
RUN dotnet publish "eShoesBackend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "eShoesBackend.dll"]