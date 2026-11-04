#!/usr/bin/env bash
set -e

echo "🚀 [Facturno Container] Iniciando PocketBase local en segundo plano..."
/app/pocketbase serve --http="127.0.0.1:8090" --dir="/app/pb_data" --migrationsDir="/app/pb_migrations" &
PB_PID=$!

echo "⏳ [Facturno Container] Esperando que PocketBase esté listo en 127.0.0.1:8090..."
for i in {1..30}; do
    if curl -s http://127.0.0.1:8090/api/health > /dev/null 2>&1; then
        echo "✅ [Facturno Container] PocketBase está activo y saludable."
        break
    fi
    sleep 0.5
done

# Función de captura para detener PocketBase si la API se apaga
cleanup() {
    echo "🛑 [Facturno Container] Deteniendo servicios..."
    kill -TERM "$PB_PID" 2>/dev/null || true
}
trap cleanup SIGTERM SIGINT EXIT

echo "🚀 [Facturno Container] Iniciando Facturno.API en ASP.NET Core 10..."
exec dotnet Facturno.API.dll

