# Guía de Inicialización y Arquitectura del Proyecto .NET - Facturno

Este documento recopila la estructura de solución C# .NET 10 y los comandos ejecutados para crear la arquitectura multicapa del sistema Facturno.

---

## Estructura de Proyectos de la Solución (`Facturno.sln`)

La solución está organizada en 4 proyectos desacoplados bajo el directorio `/facturno/`:

```
facturno/
├── Facturno.slnx / Facturno.sln
└── src/
    ├── Core/
    │   └── Facturno.Shared/            (Biblioteca de Clases .NET 10 - Modelos, Enums, DTOs, Interfaces, ApiResponse<T>)
    ├── Infraestructura/
    │   └── Facturno.Infrastructure/    (Biblioteca de Clases .NET 10 - Supabase.Client SDK, Repositorios, SOAP ARCA, PDF)
    ├── API/
    │   └── Facturno.API/               (ASP.NET Core Web API .NET 10 - Controladores REST, JWT Auth, TurnoService)
    └── UI/
        └── Facturno.Blazor/            (Blazor WebAssembly .NET 10 - Vistas Razor Code-Behind, DelegatingHandler)
```

---

## Comandos de Creación y Vinculación

### 1. Creación de la Solución y Proyectos
```bash
dotnet new sln -n Facturno
dotnet new classlib -n Facturno.Shared -o src/Core/Facturno.Shared
dotnet new classlib -n Facturno.Infrastructure -o src/Infraestructura/Facturno.Infrastructure
dotnet new webapi -n Facturno.API -o src/API/Facturno.API
dotnet new blazorwasm -n Facturno.Blazor -o src/UI/Facturno.Blazor
```

### 2. Vinculación de Referencias de Proyectos
```bash
# Conectar API con Shared e Infrastructure
dotnet add src/API/Facturno.API reference src/Core/Facturno.Shared
dotnet add src/API/Facturno.API reference src/Infraestructura/Facturno.Infrastructure

# Conectar Infrastructure con Shared
dotnet add src/Infraestructura/Facturno.Infrastructure reference src/Core/Facturno.Shared

# Conectar Blazor Frontend con Shared
dotnet add src/UI/Facturno.Blazor reference src/Core/Facturno.Shared
```

### 3. Agregado a la Solución
```bash
dotnet sln add src/Core/Facturno.Shared/Facturno.Shared.csproj
dotnet sln add src/Infraestructura/Facturno.Infrastructure/Facturno.Infrastructure.csproj
dotnet sln add src/API/Facturno.API/Facturno.API.csproj
dotnet sln add src/UI/Facturno.Blazor/Facturno.Blazor.csproj
```

### 4. Compilación de Verificación
```bash
dotnet build
```

---

## Patrones de Código Aplicados
- **Core Compartido (`Facturno.Shared`)**: Reutilización de DTOs, Enums y del wrapper `ApiResponse<T>` entre el frontend Blazor y la API REST.
- **Patrón Repositorio Específico**: Encapsulamiento del cliente Supabase C# mediante interfaces `ITurnoRepository`, `IPacienteRepository`, `IProfesionalRepository`, `IUsuarioRepository`.
- **Patrón Code-Behind en Blazor**: Separación de componentes visuales `.razor` y lógica `.razor.cs`.
- **Reglas de Negocio en `TurnoService.cs`**: Validación previa de solapamientos de 30 minutos mínimos y agendamiento de turnos recurrentes.
