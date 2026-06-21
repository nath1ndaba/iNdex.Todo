# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore first (layer-cache friendly)
COPY ["src/iNdex.Todo.API/iNdex.Todo.API.csproj",            "src/iNdex.Todo.API/"]
COPY ["src/iNdex.Todo.Application/iNdex.Todo.Application.csproj", "src/iNdex.Todo.Application/"]
COPY ["src/iNdex.Todo.Domain/iNdex.Todo.Domain.csproj",       "src/iNdex.Todo.Domain/"]
COPY ["src/iNdex.Todo.Infrastructure/iNdex.Todo.Infrastructure.csproj", "src/iNdex.Todo.Infrastructure/"]
COPY ["src/iNdex.Todo.Contracts/iNdex.Todo.Contracts.csproj", "src/iNdex.Todo.Contracts/"]

RUN dotnet restore "src/iNdex.Todo.API/iNdex.Todo.API.csproj"

# Copy source and publish
COPY . .
RUN dotnet publish "src/iNdex.Todo.API/iNdex.Todo.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "iNdex.Todo.API.dll"]
