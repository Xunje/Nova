FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/system/Nova.SystemService/Nova.SystemService.Host/Nova.SystemService.Host.csproj"
RUN dotnet publish "services/system/Nova.SystemService/Nova.SystemService.Host/Nova.SystemService.Host.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Nova.SystemService.Host.dll"]
