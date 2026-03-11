FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "gateway/Nova.Gateway/Nova.Gateway.csproj"
RUN dotnet publish "gateway/Nova.Gateway/Nova.Gateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Nova.Gateway.dll"]
