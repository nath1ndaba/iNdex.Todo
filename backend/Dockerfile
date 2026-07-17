# ============================
# BUILD STAGE
# ============================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files
COPY src/iNdex.Todo.API/iNdex.Todo.API.csproj src/iNdex.Todo.API/
COPY src/iNdex.Todo.Application/iNdex.Todo.Application.csproj src/iNdex.Todo.Application/
COPY src/iNdex.Todo.Domain/iNdex.Todo.Domain.csproj src/iNdex.Todo.Domain/
COPY src/iNdex.Todo.Infrastructure/iNdex.Todo.Infrastructure.csproj src/iNdex.Todo.Infrastructure/
COPY src/iNdex.Todo.Contracts/iNdex.Todo.Contracts.csproj src/iNdex.Todo.Contracts/

# Restore packages
RUN dotnet restore src/iNdex.Todo.API/iNdex.Todo.API.csproj

# Copy remaining source
COPY . .

# Publish application
RUN dotnet publish src/iNdex.Todo.API/iNdex.Todo.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================
# RUNTIME STAGE
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Render configuration
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "iNdex.Todo.API.dll"]