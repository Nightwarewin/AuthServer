# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the AuthServer.csproj file into the container
COPY ./AuthServer/AuthServer.csproj ./AuthServer/  # Adjust this path

# Restore dependencies
RUN dotnet restore "AuthServer/AuthServer.csproj"

# Copy the rest of the application code
COPY . .

WORKDIR /src/AuthServer

# Build and publish the application
RUN dotnet publish "AuthServer.csproj" -c Release -o /app/publish

# Set up the runtime environment
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish . 
ENTRYPOINT ["dotnet", "AuthServer.dll"]