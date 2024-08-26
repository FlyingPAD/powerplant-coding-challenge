# Step 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["powerplant-coding-challenge/powerplant-coding-challenge.csproj", "powerplant-coding-challenge/"]
WORKDIR /src/powerplant-coding-challenge
RUN dotnet restore

# Copy the remaining source code files
COPY powerplant-coding-challenge/ .

# Build the project
RUN dotnet publish -c Release -o /app/publish --no-restore

# Step 2: Set up the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the build output from the previous step
COPY --from=build /app/publish .

# Expose the port used by the application
EXPOSE 8888

# Set the entry point of the application
ENTRYPOINT ["dotnet", "powerplant-coding-challenge.dll"]
