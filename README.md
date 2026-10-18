# Facturno — Centro Médico Buen Pastor

Sistema integral de gestión de turnos médicos, agendas profesionales, pacientes y facturación para centros de salud.

Desarrollado en **.NET 10** bajo una arquitectura limpia y modular en capas, con persistencia y autenticación 100% local a través de **PocketBase**.

---

## 🏗️ Arquitectura de la Solución

El proyecto está estructurado en una única solución unificada (`Facturno.slnx`) organizada en 4 capas desacopladas:

```
facturno/
├── src/
│   ├── UI/
│   │   └── Facturno.Blazor/          # Interfaz web rica en Blazor WebAssembly (.NET 10)
│   ├── API/
│   │   └── Facturno.API/             # Web API RESTful con autenticación JWT simétrica
│   ├── Core/
│   │   └── Facturno.Shared/          # Modelos de dominio, DTOs, Enums e Interfaces
│   └── Infraestructura/
│       └── Facturno.Infrastructure/  # Repositorios PocketBase y generación de PDF (QuestPDF)
tools/
└── pocketbase/                       # Servidor PocketBase embebido local y migraciones
start_pocketbase.sh                   # Script de inicio rápido de la base de datos local
```

---

## 🛠️ Stack Tecnológico

- **Frontend:** Blazor WebAssembly (.NET 10), CSS modular moderno con estética visual elevada, barras translúcidas (`backdrop-filter: blur`), indicadores de carga glassmorphic y modales interactivos.
- **Backend:** ASP.NET Core Web API (.NET 10), autenticación JWT estándar con validación de roles (`[Authorize(Roles = "...")]`).
- **Base de Datos y Autenticación:** [PocketBase v0.22](https://pocketbase.io/) (SQLite local, puerto `8090`), sin dependencias de servicios externos en la nube.
- **Generación de Documentos:** QuestPDF para emisión de comprobantes de atención en formato PDF.

---

## 🚀 Guía de Inicio Rápido

### 1. Iniciar el Servidor de Base de Datos (PocketBase)

En la raíz del proyecto, ejecuta el script de arranque:

```bash
./start_pocketbase.sh
```

- **API de PocketBase:** `http://127.0.0.1:8090`
- **Panel Administrativo Web:** `http://127.0.0.1:8090/_/`

*(Las colecciones y datos iniciales de prueba se cargan automáticamente desde `tools/pocketbase/pb_migrations/` en el primer inicio).*

### 2. Iniciar la API Backend

En una nueva terminal:

```bash
cd facturno/src/API/Facturno.API
dotnet run --launch-profile http
```

- **URL de la API:** `http://localhost:5082`

### 3. Iniciar el Frontend (Blazor)

En otra terminal:

```bash
cd facturno/src/UI/Facturno.Blazor
dotnet run
```

- **URL de la Aplicación:** `http://localhost:5226`

---

## 👥 Usuarios de Prueba Preconfigurados

| Rol | Correo Electrónico | Contraseña | Descripción |
| :--- | :--- | :--- | :--- |
| **Operador (Admin)** | `admin@facturno.com` | `admin123456` | Acceso total al sistema, gestión de personal y configuración |
| **Profesional** | `laura.gomez@facturno.com` | `laura123!` | Dra. Laura Gómez (Psicología) — Gestión de turnos y agenda |
| **Profesional** | `carlos.rossi@facturno.com` | `carlos123!` | Dr. Carlos Rossi (Psiquiatría) — Gestión de turnos y agenda |
| **Administrativo** | `marcos.admin@facturno.com` | `marcos123!` | Marcos Benítez — Gestión de agendas compartidas y recepción |

---

## 📖 Documentación

- **Bitácora de Cambios y Soluciones:** Consulta [`DocumentacionFac/Bitacora_Cambios_y_Soluciones.md`](DocumentacionFac/Bitacora_Cambios_y_Soluciones.md) para detalles de arquitectura, problemas resueltos y el proceso de migración de Supabase a PocketBase.
