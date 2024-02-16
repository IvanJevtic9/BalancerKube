#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# docker build -t event-generator-service -f .\BalanceKube.EventGenerator.API\Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
				
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["BalanceKube.EventGenerator.API/BalanceKube.EventGenerator.API.csproj", "BalanceKube.EventGenerator.API/"]
RUN dotnet restore "./BalanceKube.EventGenerator.API/./BalanceKube.EventGenerator.API.csproj"

COPY . .
WORKDIR "/src/BalanceKube.EventGenerator.API"
RUN dotnet build "./BalanceKube.EventGenerator.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BalanceKube.EventGenerator.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BalanceKube.EventGenerator.API.dll"]