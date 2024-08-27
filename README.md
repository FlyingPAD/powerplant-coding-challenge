# powerplant-coding-challenge

This project is an ASP.NET Core web application that calculates and allocates power production across different power plants based on input data.

## Prerequisites

- .NET 8.0 SDK
- Docker (if you choose to run the app via Docker)

## Running the Application

### Running Normally

1. **Restore dependencies:**

   dotnet restore

2. **Build the application:**

   dotnet build

3. **Run the application:**

   dotnet run

4. **Access the application** at `http://localhost:8888/swagger/index.html`.

### Running via Docker

1. **Build the Docker image:**

   docker build -t powerplant-coding-challenge .

2. **Run the Docker container:**

   docker run -d -p 8888:8888 powerplant-coding-challenge

3. **Access the application** at `http://localhost:8888/swagger/index.html`.

## Notes

- Ensure you have the necessary prerequisites installed.
- The default port for Docker is `8888`, which can be adjusted as needed.
