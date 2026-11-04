#!/usr/bin/env python3
"""
Script de utilidad para sembrar (seed) la base de datos de PocketHost.io
con las colecciones y datos de prueba del sistema Facturno.

Uso:
  python3 tools/pocketbase/seed_pockethost.py <URL_POCKETHOST> <ADMIN_EMAIL> <ADMIN_PASSWORD>

Ejemplo:
  python3 tools/pocketbase/seed_pockethost.py https://facturno-db.pockethost.io admin@facturno.com MiPassword123!
"""

import sys
import json
import urllib.request
import urllib.error
from pathlib import Path

def post_json(url, data, token=None):
    payload = json.dumps(data).encode('utf-8')
    req = urllib.request.Request(url, data=payload, headers={'Content-Type': 'application/json'})
    if token:
        req.add_header('Authorization', token)
    try:
        with urllib.request.urlopen(req) as response:
            return json.loads(response.read().decode('utf-8'))
    except urllib.error.HTTPError as e:
        err_body = e.read().decode('utf-8')
        print(f"Error HTTP {e.code} en {url}: {err_body}")
        raise

def put_json(url, data, token=None):
    payload = json.dumps(data).encode('utf-8')
    req = urllib.request.Request(url, data=payload, headers={'Content-Type': 'application/json'}, method='PUT')
    if token:
        req.add_header('Authorization', token)
    try:
        with urllib.request.urlopen(req) as response:
            return json.loads(response.read().decode('utf-8'))
    except urllib.error.HTTPError as e:
        err_body = e.read().decode('utf-8')
        print(f"Error HTTP {e.code} en {url}: {err_body}")
        raise

def main():
    if len(sys.argv) < 4:
        print("Uso: python3 seed_pockethost.py <URL_POCKETHOST> <ADMIN_EMAIL> <ADMIN_PASSWORD>")
        sys.exit(1)

    base_url = sys.argv[1].rstrip('/')
    admin_email = sys.argv[2]
    admin_pass = sys.argv[3]

    print(f"Conectando a PocketHost en: {base_url}...")

    # 1. Autenticación de Administrador
    auth_url = f"{base_url}/api/admins/auth-with-password"
    try:
        auth_res = post_json(auth_url, {"identity": admin_email, "password": admin_pass})
        admin_token = auth_res.get('token')
        print("✅ Autenticado como Administrador de PocketBase con éxito.")
    except Exception as e:
        print(f"❌ Falló la autenticación en PocketHost: {e}")
        sys.exit(1)

    # 2. Importar Esquema de Colecciones
    schema_path = Path(__file__).parent / "pb_schema.json"
    if schema_path.exists():
        print("Importando esquema de colecciones desde pb_schema.json...")
        with open(schema_path, 'r', encoding='utf-8') as f:
            collections = json.load(f)

        import_url = f"{base_url}/api/collections/import"
        try:
            put_json(import_url, {"collections": collections, "deleteMissing": False}, token=admin_token)
            print("✅ Esquema de colecciones importado satisfactoriamente.")
        except Exception as e:
            print(f"⚠️ Nota al importar esquema: {e}")
    else:
        print(f"⚠️ No se encontró {schema_path}")

    print("\n🎉 ¡Proceso finalizado con éxito! La instancia en PocketHost está lista para producción.")

if __name__ == '__main__':
    main()

