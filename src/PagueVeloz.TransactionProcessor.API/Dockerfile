# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/PagueVeloz.TransactionProcessor.API/PagueVeloz.TransactionProcessor.API.csproj", "src/PagueVeloz.TransactionProcessor.API/"]
COPY ["src/PagueVeloz.TransactionProcessor.Application/PagueVeloz.TransactionProcessor.Application.csproj", "src/PagueVeloz.TransactionProcessor.Application/"]
COPY ["src/PagueVeloz.TransactionProcessor.Infrastructure/PagueVeloz.TransactionProcessor.Infrastructure.csproj", "src/PagueVeloz.TransactionProcessor.Infrastructure/"]
COPY ["src/PagueVeloz.TransactionProcessor.Domain/PagueVeloz.TransactionProcessor.Domain.csproj", "src/PagueVeloz.TransactionProcessor.Domain/"]

# Restore dependencies
RUN dotnet restore "src/PagueVeloz.TransactionProcessor.API/PagueVeloz.TransactionProcessor.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/PagueVeloz.TransactionProcessor.API"
RUN dotnet build "PagueVeloz.TransactionProcessor.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PagueVeloz.TransactionProcessor.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PagueVeloz.TransactionProcessor.API.dll"]

