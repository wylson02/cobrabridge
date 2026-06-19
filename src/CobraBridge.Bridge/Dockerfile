# CobraBridge Bridge — the anti-corruption layer.
# Multi-stage build: full SDK to restore/publish, slim ASP.NET runtime to run.
# Build context is src/ (not this folder) because the Bridge references the
# shared CobraBridge.Domain project — see docker-compose.yml.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CobraBridge.Bridge/CobraBridge.Bridge.csproj CobraBridge.Bridge/
COPY CobraBridge.Domain/CobraBridge.Domain.csproj CobraBridge.Domain/
RUN dotnet restore CobraBridge.Bridge/CobraBridge.Bridge.csproj

COPY CobraBridge.Bridge/ CobraBridge.Bridge/
COPY CobraBridge.Domain/ CobraBridge.Domain/
RUN dotnet publish CobraBridge.Bridge/CobraBridge.Bridge.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CobraBridge.Bridge.dll"]
