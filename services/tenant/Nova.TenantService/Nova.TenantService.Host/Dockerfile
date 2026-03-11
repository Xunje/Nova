FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/tenant/Nova.TenantService/src/Nova.TenantService.Host/Nova.TenantService.Host.csproj"
RUN dotnet publish "services/tenant/Nova.TenantService/src/Nova.TenantService.Host/Nova.TenantService.Host.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Nova.TenantService.Host.dll"]
