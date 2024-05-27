#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FoodHealthChecker.csproj", "."]
RUN dotnet restore "./FoodHealthChecker.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./FoodHealthChecker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FoodHealthChecker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
USER root  
RUN useradd -m -u 1000 user
COPY --from=publish /app/publish .
RUN mkdir -p /app/wwwroot/temp  
RUN groupadd mygroup && usermod -a -G mygroup user && usermod -a -G mygroup app
RUN chown :mygroup -R /app/wwwroot/temp
RUN chmod 770 -R /app/wwwroot/temp
USER app  
ENTRYPOINT ["dotnet", "FoodHealthChecker.dll"]