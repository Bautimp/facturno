# ==========================================================
# Etapa 1: Build y Publish de la API con SDK .NET 10
# ==========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar archivos .csproj para aprovechar el cacheo de dependencias
COPY facturno/src/Core/Facturno.Shared/Facturno.Shared.csproj src/Core/Facturno.Shared/
COPY facturno/src/Infraestructura/Facturno.Infrastructure/Facturno.Infrastructure.csproj src/Infraestructura/Facturno.Infrastructure/
COPY facturno/src/API/Facturno.API/Facturno.API.csproj src/API/Facturno.API/

# Restaurar paquetes NuGet
RUN dotnet restore src/API/Facturno.API/Facturno.API.csproj

# Copiar código fuente
COPY facturno/src/Core/ src/Core/
COPY facturno/src/Infraestructura/ src/Infraestructura/
COPY facturno/src/API/ src/API/

# Publicar en modo Release
WORKDIR /app/src/API/Facturno.API
RUN dotnet publish -c Release -o /out

# ==========================================================
# Etapa 2: Runtime unificado (ASP.NET Core 10 + PocketBase)
# ==========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Instalar curl (para healthcheck) y libfontconfig1 (para fuentes en QuestPDF)
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    ca-certificates \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

# Copiar artefactos de la API
COPY --from=build /out .

# Copiar binario de PocketBase y migraciones de inicialización
COPY tools/pocketbase/pocketbase /app/pocketbase
COPY tools/pocketbase/pb_migrations /app/pb_migrations
RUN chmod +x /app/pocketbase

# Copiar script de orquestación de arranque
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Configurar puertos: Render/Koyeb inyectan PORT o usan 8080
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
