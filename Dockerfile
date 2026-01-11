# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY src/FileConversionApi.Domain/FileConversionApi.Domain.csproj src/FileConversionApi.Domain/
COPY src/FileConversionApi.Application/FileConversionApi.Application.csproj src/FileConversionApi.Application/
COPY src/FileConversionApi.Infrastructure/FileConversionApi.Infrastructure.csproj src/FileConversionApi.Infrastructure/
COPY src/FileConversionApi.Api/FileConversionApi.Api.csproj src/FileConversionApi.Api/
COPY Directory.Build.props .
COPY stylecop.json .

# Restore dependencies
RUN dotnet restore src/FileConversionApi.Api/FileConversionApi.Api.csproj

# Copy source code
COPY src/ src/

# Build and Publish
RUN dotnet publish src/FileConversionApi.Api/FileConversionApi.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install Chromium for PuppeteerSharp
RUN apt-get update && apt-get install -y \
    chromium \
    fonts-liberation \
    libasound2t64 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libcups2 \
    libdbus-1-3 \
    libgdk-pixbuf2.0-0 \
    libnspr4 \
    libnss3 \
    libx11-xcb1 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    xdg-utils \
    --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*

ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "FileConversionApi.Api.dll"]
