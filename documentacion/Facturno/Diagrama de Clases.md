Para el sistema Facturno se establece el siguiente diagrama de clases, mantenido en total consistencia con la estructura de Base de Datos y Casos de Uso:

``` PlantUML
@startuml
hide circle
skinparam classAttributeIconSize 0
left to right direction
skinparam shadowing false

' --- Clases Base ---

class Persona {
  + id_persona: int
  + nombre: string
  + apellido: string
  + correo: string
  + telefono: long
}

class Usuario {
  + id_usuario: Guid
  + id_persona: int
  + rol: RolUsuario
  + activo: boolean
}

class Paciente {
  + id_paciente: int
  + num_documento: string
  + tipo_documento: TipoDocumento
  + id_obra_social: int
  + num_obra_social: string
  + porcentaje_iva: PorcentajeIVA
}

class Profesional {
  + id_profesional: Guid
  + especialidad: TipoEspecialidad
  + matricula: string
  + cuit: string
  + precio_consulta: decimal
  + condicion_iva: CondicionIVA
  + tipo_comprobante: TipoComprobante
}

class Turno {
  + id_turno: int
  + fecha: DateOnly
  + hora: TimeOnly
  + estado: EstadoTurno
  + observacion: string
  + id_paciente: int
  + id_profesional: Guid
}

class ObraSocial {
  + id_obra_social: int
  + nombre: string
  + activo: boolean
}

' --- Relaciones de Herencia ---
Persona <|-- Usuario
Persona <|-- Paciente

Usuario <|-- Profesional

' --- Asociaciones y Cardinalidades ---
Paciente "0..*" --> "1" ObraSocial : posee >
Turno "0..*" --> "1" Paciente : pertenece a >
Turno "0..*" --> "1" Profesional : asignado a >

@enduml
```