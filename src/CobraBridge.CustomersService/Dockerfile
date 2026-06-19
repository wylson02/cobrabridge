# CobraBridge CustomersService — a net-new modern capability (no legacy
# COBOL equivalent). Multi-stage build: full SDK to restore/publish, slim
# ASP.NET runtime to run. Build context is this folder — unlike Bridge and
# AccountsService, CustomersService has no shared-project dependency.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CobraBridge.CustomersService.csproj ./
RUN dotnet restore CobraBridge.CustomersService.csproj

COPY . ./
RUN dotnet publish CobraBridge.CustomersService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CobraBridge.CustomersService.dll"]
