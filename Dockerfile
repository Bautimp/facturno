# ==========================================================
# Etapa 1: Build y Publish de la API y de Blazor WASM
# ==========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar archivos de proyecto para cachear dependencias
COPY facturno/src/Core/Facturno.Shared/Facturno.Shared.csproj src/Core/Facturno.Shared/
COPY facturno/src/Infraestructura/Facturno.Infrastructure/Facturno.Infrastructure.csproj src/Infraestructura/Facturno.Infrastructure/
COPY facturno/src/API/Facturno.API/Facturno.API.csproj src/API/Facturno.API/
COPY facturno/src/UI/Facturno.Blazor/Facturno.Blazor.csproj src/UI/Facturno.Blazor/

# Restaurar dependencias
RUN dotnet restore src/API/Facturno.API/Facturno.API.csproj
RUN dotnet restore src/UI/Facturno.Blazor/Facturno.Blazor.csproj

# Copiar código fuente completo
COPY facturno/src/Core/ src/Core/
COPY facturno/src/Infraestructura/ src/Infraestructura/
COPY facturno/src/API/ src/API/
COPY facturno/src/UI/Facturno.Blazor/ src/UI/Facturno.Blazor/

# 1. Compilar Backend Web API
WORKDIR /app/src/API/Facturno.API
RUN dotnet publish -c Release -o /out_api

# 2. Compilar Frontend Blazor WebAssembly
WORKDIR /app/src/UI/Facturno.Blazor
RUN dotnet publish -c Release -o /out_blazor

# ==========================================================
# Etapa 2: Runtime unificado (ASP.NET Core 10 + Blazor + PocketBase)
# ==========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Instalar dependencias necesarias: curl y libfontconfig1 para QuestPDF
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    ca-certificates \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

# Copiar la API compilada
COPY --from=build /out_api .

# Copiar los archivos estáticos de Blazor WebAssembly a la carpeta wwwroot de la API
COPY --from=build /out_blazor/wwwroot /app/wwwroot

# Copiar binario de PocketBase y migraciones
COPY tools/pocketbase/pocketbase /app/pocketbase
COPY tools/pocketbase/pb_migrations /app/pb_migrations
RUN chmod +x /app/pocketbase

# Copiar script de arranque orquestado
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Configurar puerto dinámico (Render usa 8080 o $PORT)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
