FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/business/Nova.BusinessService/src/Nova.BusinessService.Host/Nova.BusinessService.Host.csproj"
RUN dotnet publish "services/business/Nova.BusinessService/src/Nova.BusinessService.Host/Nova.BusinessService.Host.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Nova.BusinessService.Host.dll"]
