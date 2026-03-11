FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/identity/Nova.IdentityService/src/Nova.IdentityService.Host/Nova.IdentityService.Host.csproj"
RUN dotnet publish "services/identity/Nova.IdentityService/src/Nova.IdentityService.Host/Nova.IdentityService.Host.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Nova.IdentityService.Host.dll"]
