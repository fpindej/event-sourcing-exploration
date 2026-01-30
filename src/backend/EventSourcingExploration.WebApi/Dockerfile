# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution-wide config files
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["nuget.config", "."]

# Copy csproj files
COPY ["EventSourcingExploration.Domain/EventSourcingExploration.Domain.csproj", "EventSourcingExploration.Domain/"]
COPY ["EventSourcingExploration.Application/EventSourcingExploration.Application.csproj", "EventSourcingExploration.Application/"]
COPY ["EventSourcingExploration.Infrastructure/EventSourcingExploration.Infrastructure.csproj", "EventSourcingExploration.Infrastructure/"]
COPY ["EventSourcingExploration.WebApi/EventSourcingExploration.WebApi.csproj", "EventSourcingExploration.WebApi/"]

# Restore dependencies
RUN dotnet restore "EventSourcingExploration.WebApi/EventSourcingExploration.WebApi.csproj"

# Copy the entire source code
COPY . .

# Build the application
WORKDIR "/src/EventSourcingExploration.WebApi"
RUN dotnet build "EventSourcingExploration.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EventSourcingExploration.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:DebugType=None /p:DebugSymbols=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "EventSourcingExploration.WebApi.dll"]
