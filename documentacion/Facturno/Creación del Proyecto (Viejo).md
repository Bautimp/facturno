Para crear el proyecto en Visual Studio Code se realizaron los siguientes pasos:
### 1. Creación de la Solución Principal

Primero, creaste el contenedor principal que agrupará todos los proyectos.

```
dotnet new sln -n Facturno
```

- **Qué hace:** Genera el archivo de solución (`Facturno.sln` o `Facturno.slnx`) en la raíz del directorio. Este archivo le permite a tu IDE (Visual Studio Code) entender que todos los subproyectos forman parte de un mismo ecosistema.
    

### 2. Creación de los Proyectos (Capas)

Luego, fuiste creando cada uno de los proyectos y asignándolos a sus respectivas carpetas dentro de `src/`.

- **Frontend (Blazor):**

    ```
    dotnet new blazorwasm -n facturno.Blazor -o src/UI/facturno.Blazor
    ```
    
    _Crea la aplicación web independiente que se ejecutará en el navegador._
    
- **Backend (API):**

    ```
    dotnet new webapi -n Facturno.API -o src/API/Facturno.API
    ```
    
    _Crea la API REST. Aquí apareció una advertencia (`NU1903`) sobre una vulnerabilidad en el paquete `Microsoft.OpenApi`. Esto es normal en las plantillas por defecto de .NET; se soluciona actualizando los paquetes NuGet más adelante._
    
- **Core (Modelos Compartidos):**

    ```
    dotnet new classlib -n Facturno.Shared -o src/Core/Facturno.Shared
    ```
    
    _Crea la biblioteca de clases que contendrá tus entidades (Turno, Paciente, etc.) para ser usadas tanto por el frontend como por el backend._
    
- **Infraestructura (Base de datos y ARCA):**

    ```
    dotnet new classlib -n Facturno.Infrastructure -o src/Infraestructura/Facturno.Infrastructure
    ```
    
    _Crea la biblioteca donde vivirá la lógica de conexión a Supabase y el servicio SOAP._
    

### 3. Vinculación de Proyectos (Agregando Referencias)

En este paso, indico a los proyectos cómo deben comunicarse entre sí.
```
# 1. Conectar la API con los Modelos (Core)
dotnet add src/API/Facturno.API reference src/Core/Facturno.Shared

# 2. Conectar la API con la Infraestructura
dotnet add src/API/Facturno.API reference src/Infraestructura/Facturno.Infrastructure

# 3. Conectar la Infraestructura con los Modelos (Core)
dotnet add src/Infraestructura/Facturno.Infrastructure reference src/Core/Facturno.Shared

# 4. Conectar la el Frontend (Blazor) con los Modelos (Core)
dotnet add src/UI/Facturno.Blazor reference src/Core/Facturno.Shared
```

### 4. Agrupación en la Solución

Con los proyectos creados y vinculados, los agregaste todos al archivo de la solución para poder gestionarlos juntos:

```
dotnet sln add src/UI/Facturno.Blazor/Facturno.Blazor.csproj
dotnet sln add src/API/Facturno.API/Facturno.API.csproj
dotnet sln add src/Core/Facturno.Shared/Facturno.Shared.csproj
dotnet sln add src/Infraestructura/Facturno.Infrastructure/Facturno.Infrastructure.csproj
```

### 5. Compilación General (Build)

Finalmente, verificaste que toda la estructura fuera válida y se pudiera ensamblar.

```
dotnet build
```

- **Resultado:** ¡Éxito total! El proyecto compiló en 23.9 segundos. Los 4 proyectos (`Facturno.Shared`, `Facturno.Infrastructure`, `Facturno.API`, y `facturno.Blazor`) generaron sus respectivos archivos `.dll` y binarios para `.NET 10.0`. Se mantuvieron las 2 advertencias sobre la dependencia de Swagger/OpenAPI, lo cual no impide el desarrollo y se puede actualizar luego.