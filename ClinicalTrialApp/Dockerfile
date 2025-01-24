# Running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget
ENV ASPNETCORE_ENVIRONMENT=Development

# Build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ClinicalTrialApp/ClinicalTrialApp.csproj", "ClinicalTrialApp/"]
RUN dotnet restore "ClinicalTrialApp/ClinicalTrialApp.csproj"
COPY . .
WORKDIR "/src/ClinicalTrialApp"
RUN dotnet build "ClinicalTrialApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ClinicalTrialApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Production or when running from VS in regular mode
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure for HTTPS
ENV ASPNETCORE_URLS=https://+:5001;http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "ClinicalTrialApp.dll"]