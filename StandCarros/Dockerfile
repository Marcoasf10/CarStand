FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install dependencies
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev

# Set working directory and copy project file
WORKDIR /src
COPY ["StandCarros/StandCarros.csproj", "StandCarros/"]

# Restore dependencies
RUN dotnet restore "StandCarros/StandCarros.csproj"

# Copy the remaining source code
COPY . .
WORKDIR "/src/StandCarros"

# Build the application
RUN dotnet build "StandCarros.csproj" -c Release -o /app/build

# Publish the application as a self-contained deployment
FROM build AS publish
RUN dotnet publish "StandCarros.csproj" -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o /app/publish

# Create the final image with the standalone app
FROM debian:12 AS final

# Install required runtime dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    libssl3 \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080

# Copy the self-contained application to the final image
COPY --from=publish /app/publish .

# Set the entry point to the single executable
ENTRYPOINT ["./StandCarros"]
