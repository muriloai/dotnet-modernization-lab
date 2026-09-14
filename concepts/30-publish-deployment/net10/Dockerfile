# Multi-stage Dockerfile para .NET 10
# Estagio 1: Build e Publicacao
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PublishDeployDemo.csproj", "./"]
RUN dotnet restore "PublishDeployDemo.csproj"
COPY . .
RUN dotnet publish "PublishDeployDemo.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estagio 2: Runtime Minimo Chiseled (sem shell, < 100 MB)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "PublishDeployDemo.dll"]
