# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# Docker Configuration for .NET SQLite CRUD Generator
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Install native dependencies for SQLite and gRPC
RUN apk add --no-cache \
    icu-libs \
    krb5-libs \
    libgcc \
    libintl \
    libssl3 \
    libstdc++ \
    zlib \
    && apk --update add --no-cache icu-data-full

# Copy project files and restore dependencies
COPY ["src/DotNet.SQLite.CrudGenerator/DotNet.SQLite.CrudGenerator.csproj", "src/DotNet.SQLite.CrudGenerator/"]
COPY ["dotnet-sqlite-crud-generator.sln", "."]

RUN dotnet restore "dotnet-sqlite-crud-generator.sln" \
    --use-current-runtime \
    --disable-parallel


# Copy source code and build
COPY . .
WORKDIR "/src/src/DotNet.SQLite.CrudGenerator"


RUN dotnet build "DotNet.SQLite.CrudGenerator.csproj" \
    -c Release \
    -o /app/build \
    --no-restore


# Publish stage
FROM build AS publish
RUN dotnet publish "DotNet.SQLite.CrudGenerator.csproj" \
    -c Release \
    -o /app/publish \
    --no-build \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final


# Install native dependencies for SQLite and gRPC
RUN apk add --no-cache \
    icu-libs \
    krb5-libs \
    libgcc \
    libintl \
    libssl3 \
    libstdc++ \
    zlib \
    && apk --update add --no-cache icu-data-full


# Create non-root user and group
RUN addgroup -S appuser && adduser -S -G appuser appuser


WORKDIR /app

# Create data directory with proper permissions
RUN mkdir -p /data && \
    chown -R appuser:appuser /data && \
    chmod 777 /data

# Copy published application
COPY --from=publish /app/publish . .

# Set permissions for application files
RUN chown -R appuser:appuser /app && \
    chmod -R 755 /app

# Switch to non-root user
USER appuser

ENV ASPNETCORE_URLS=http://+:5000
ENV DATABASE_PATH=/data/app.db
ENV LOG_LEVEL=Information
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health/ready || exit 1

EXPOSE 5000

ENTRYPOINT ["dotnet", "DotNet.SQLite.CrudGenerator.dll"]
