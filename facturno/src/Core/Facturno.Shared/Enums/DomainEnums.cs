namespace Facturno.Shared.Enums;

public enum RolUsuario
{
    Administrativo,
    Profesional,
    Operador
}

public enum CondicionIVA
{
    ConsumidorFinal,
    Monotributo,
    ResponsableInscripto,
    Exento
}

public enum EstadoTurno
{
    Activo,
    Completo,
    Falta,
    Cancelado
}

public enum TipoComprobante
{
    FacturaB,
    FacturaA
}

public enum TipoDocumento
{
    DNI,
    CUIL,
    Pasaporte
}

public enum TipoEspecialidad
{
    Psicologo,
    Psiquiatra,
    Otro
}
