FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/GameOfDrones.Api/GameOfDrones.Api.csproj backend/GameOfDrones.Api/
RUN dotnet restore backend/GameOfDrones.Api/GameOfDrones.Api.csproj

COPY backend/GameOfDrones.Api/ backend/GameOfDrones.Api/
RUN dotnet publish backend/GameOfDrones.Api/GameOfDrones.Api.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet GameOfDrones.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]
