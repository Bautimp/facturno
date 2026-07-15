using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.Infrastructure.Supabase;

public class PacienteRepository : IPacienteRepository
{
    private readonly global::Supabase.Client _supabaseClient;

    public PacienteRepository(global::Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<Paciente> CrearAsync(Paciente paciente)
    {
        // 1. Insertamos en la tabla base (usuarios)
        var usuarioEntity = new UsuarioEntity
        {
            Nombre = paciente.Nombre,
            Apellido = paciente.Apellido,
            Email = paciente.Email,
            Contrasena = paciente.Contrasena,
            Rol = RolUsuario.Paciente.ToString()
        };

        var usuarioResponse = await _supabaseClient.From<UsuarioEntity>().Insert(usuarioEntity);
        var usuarioInsertado = usuarioResponse.Models.First();

        // 2. Insertamos en la tabla específica (pacientes) usando el ID generado
        var pacienteEntity = new PacienteEntity
        {
            IdUsuario = usuarioInsertado.IdUsuario,
            NumDniCuil = paciente.NumDniCuil,
            TipoDniCuil = paciente.TipoDniCuil,
            Telefono = paciente.Telefono,
            ObraSocial = paciente.ObraSocial,
            NumObraSocial = paciente.NumObraSocial
        };

        await _supabaseClient.From<PacienteEntity>().Insert(pacienteEntity);

        // 3. Devolvemos el modelo actualizado
        paciente.IdUsuario = usuarioInsertado.IdUsuario;
        paciente.Rol = RolUsuario.Paciente;
        return paciente;
    }

    public async Task<Paciente?> ObtenerPorIdAsync(int id)
    {
        // Buscamos los datos base
        var usuarioEntity = await _supabaseClient.From<UsuarioEntity>().Where(x => x.IdUsuario == id).Single();
        if (usuarioEntity == null) return null;

        // Buscamos los datos específicos
        var pacienteEntity = await _supabaseClient.From<PacienteEntity>().Where(x => x.IdUsuario == id).Single();
        if (pacienteEntity == null) return null;

        // Combinamos ambas partes en el modelo de dominio
        return new Paciente
        {
            IdUsuario = usuarioEntity.IdUsuario,
            Nombre = usuarioEntity.Nombre,
            Apellido = usuarioEntity.Apellido,
            Email = usuarioEntity.Email,
            Contrasena = usuarioEntity.Contrasena,
            Rol = Enum.Parse<RolUsuario>(usuarioEntity.Rol),
            NumDniCuil = pacienteEntity.NumDniCuil,
            TipoDniCuil = pacienteEntity.TipoDniCuil,
            Telefono = pacienteEntity.Telefono,
            ObraSocial = pacienteEntity.ObraSocial,
            NumObraSocial = pacienteEntity.NumObraSocial
        };
    }

    public async Task<List<Paciente>> ObtenerTodosAsync()
    {
        var pacientesEntity = await _supabaseClient.From<PacienteEntity>().Get();
        var usuariosEntity = await _supabaseClient.From<UsuarioEntity>().Where(x => x.Rol == RolUsuario.Paciente.ToString()).Get();

        var listaPacientes = new List<Paciente>();

        foreach (var pac in pacientesEntity.Models)
        {
            var user = usuariosEntity.Models.FirstOrDefault(u => u.IdUsuario == pac.IdUsuario);
            if (user != null)
            {
                listaPacientes.Add(new Paciente
                {
                    IdUsuario = user.IdUsuario,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Email = user.Email,
                    Rol = RolUsuario.Paciente,
                    NumDniCuil = pac.NumDniCuil,
                    TipoDniCuil = pac.TipoDniCuil,
                    Telefono = pac.Telefono,
                    ObraSocial = pac.ObraSocial,
                    NumObraSocial = pac.NumObraSocial
                });
            }
        }
        return listaPacientes;
    }
}