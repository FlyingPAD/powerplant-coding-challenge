# Step 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining files and build the application
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Step 2: Set up the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the build output from the previous step
COPY --from=build /app/publish .

# Expose the port used by the application
EXPOSE 8888

# Set the entry point of the application
ENTRYPOINT ["dotnet", "powerplant_coding_challenge.dll"]
