# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["dotnet-sqlite-crud-generator.sln", "."]
COPY ["src/DotNet.SQLite.CrudGenerator/DotNet.SQLite.CrudGenerator.csproj", "src/DotNet.SQLite.CrudGenerator/"]

RUN dotnet restore "dotnet-sqlite-crud-generator.sln"

COPY . .
WORKDIR "/src/src/DotNet.SQLite.CrudGenerator"

RUN dotnet build "DotNet.SQLite.CrudGenerator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotNet.SQLite.CrudGenerator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

RUN mkdir -p /data && chmod 777 /data

COPY --from=publish /app/publish .

ENV DATABASE_PATH=/data/app.db
ENV LOG_LEVEL=Information

ENTRYPOINT ["dotnet", "DotNet.SQLite.CrudGenerator.dll"]
