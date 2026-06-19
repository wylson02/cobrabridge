# CobraBridge AccountsService — the modern half of the Strangler swap.
# Multi-stage build: full SDK to restore/publish, slim ASP.NET runtime to run.
# Build context is src/ (not this folder) because the service references
# the shared CobraBridge.Domain project — see docker-compose.yml.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CobraBridge.AccountsService/CobraBridge.AccountsService.csproj CobraBridge.AccountsService/
COPY CobraBridge.Domain/CobraBridge.Domain.csproj CobraBridge.Domain/
RUN dotnet restore CobraBridge.AccountsService/CobraBridge.AccountsService.csproj

COPY CobraBridge.AccountsService/ CobraBridge.AccountsService/
COPY CobraBridge.Domain/ CobraBridge.Domain/
RUN dotnet publish CobraBridge.AccountsService/CobraBridge.AccountsService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CobraBridge.AccountsService.dll"]
