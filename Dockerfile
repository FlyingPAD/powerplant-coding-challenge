# Step 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["powerplant-coding-challenge/powerplant-coding-challenge.csproj", "./"]
RUN dotnet restore "powerplant-coding-challenge.csproj"

# Copy the remaining source code
COPY ./powerplant-coding-challenge ./powerplant-coding-challenge

# Set the working directory to the project directory
WORKDIR /src/powerplant-coding-challenge

# Build and publish the project
RUN dotnet publish -c Release -o /app/publish

# Step 2: Create the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8888
ENV ASPNETCORE_URLS=http://+:8888
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "powerplant-coding-challenge.dll"]
