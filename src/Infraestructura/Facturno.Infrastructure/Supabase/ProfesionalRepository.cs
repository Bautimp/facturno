using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;
using Facturno.Shared.Enums;
using Facturno.Infrastructure.Supabase.Entities;

namespace Facturno.Infrastructure.Supabase;

public class ProfesionalRepository : IProfesionalRepository
{
    private readonly global::Supabase.Client _supabaseClient;

    public ProfesionalRepository(global::Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<Profesional> CrearAsync(Profesional profesional)
    {
        var usuarioEntity = new UsuarioEntity
        {
            Nombre = profesional.Nombre,
            Apellido = profesional.Apellido,
            Email = profesional.Email,
            Contrasena = profesional.Contrasena,
            Rol = RolUsuario.Profesional.ToString()
        };

        var usuarioResponse = await _supabaseClient.From<UsuarioEntity>().Insert(usuarioEntity);
        var usuarioInsertado = usuarioResponse.Models.First();

        var profesionalEntity = new ProfesionalEntity
        {
            IdUsuario = usuarioInsertado.IdUsuario,
            Matricula = profesional.Matricula,
            Especialidad = profesional.Especialidad
        };

        await _supabaseClient.From<ProfesionalEntity>().Insert(profesionalEntity);

        profesional.IdUsuario = usuarioInsertado.IdUsuario;
        profesional.Rol = RolUsuario.Profesional;
        return profesional;
    }

    public async Task<Profesional?> ObtenerPorIdAsync(int id)
    {
        var usuarioEntity = await _supabaseClient.From<UsuarioEntity>().Where(x => x.IdUsuario == id).Single();
        var profesionalEntity = await _supabaseClient.From<ProfesionalEntity>().Where(x => x.IdUsuario == id).Single();

        if (usuarioEntity == null || profesionalEntity == null) return null;

        return new Profesional
        {
            IdUsuario = usuarioEntity.IdUsuario,
            Nombre = usuarioEntity.Nombre,
            Apellido = usuarioEntity.Apellido,
            Email = usuarioEntity.Email,
            Contrasena = usuarioEntity.Contrasena,
            Rol = Enum.Parse<RolUsuario>(usuarioEntity.Rol),
            Matricula = profesionalEntity.Matricula,
            Especialidad = profesionalEntity.Especialidad
        };
    }

    public async Task<List<Profesional>> ObtenerTodosAsync()
    {
        var profesionalesEntity = await _supabaseClient.From<ProfesionalEntity>().Get();
        var usuariosEntity = await _supabaseClient.From<UsuarioEntity>().Where(x => x.Rol == RolUsuario.Profesional.ToString()).Get();

        var lista = new List<Profesional>();

        foreach (var prof in profesionalesEntity.Models)
        {
            var user = usuariosEntity.Models.FirstOrDefault(u => u.IdUsuario == prof.IdUsuario);
            if (user != null)
            {
                lista.Add(new Profesional
                {
                    IdUsuario = user.IdUsuario,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Email = user.Email,
                    Rol = RolUsuario.Profesional,
                    Matricula = prof.Matricula,
                    Especialidad = prof.Especialidad
                });
            }
        }
        return lista;
    }
}