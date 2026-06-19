# CobraBridge Gateway — the single public entry point (YARP reverse proxy).
# Multi-stage build: full SDK to restore/publish, slim ASP.NET runtime to run.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CobraBridge.Gateway.csproj ./
RUN dotnet restore CobraBridge.Gateway.csproj

COPY . ./
RUN dotnet publish CobraBridge.Gateway.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CobraBridge.Gateway.dll"]
