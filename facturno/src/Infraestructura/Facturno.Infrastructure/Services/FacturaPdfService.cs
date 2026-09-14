using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Facturno.Shared.Interfaces;
using Facturno.Shared.Models;

namespace Facturno.Infrastructure.Services;

public class FacturaPdfService : IFacturaPdfService
{
    static FacturaPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerarComprobanteTurnoPdf(Turno turno, Paciente paciente, Profesional profesional, ObraSocial? obraSocial)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                page.Header().Element(headerContainer =>
                {
                    headerContainer.Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("FACTURNO").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
                            col.Item().Text("Centro Médico - Gestor de Turnos").FontSize(10).Italic();
                        });

                        row.ConstantItem(200).Column(col =>
                        {
                            col.Item().AlignRight().Text("COMPROBANTE DE ATENCIÓN").FontSize(14).Bold();
                            col.Item().AlignRight().Text($"Nº Turno: #{turno.IdTurno:D6}").FontSize(10);
                            col.Item().AlignRight().Text($"Fecha Emisión: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                        });
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    col.Spacing(15);

                    // Información del Profesional
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(pCol =>
                    {
                        pCol.Item().Text("DATOS DEL PROFESIONAL").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                        
                        var nombreProf = profesional.Usuario?.Persona != null 
                            ? $"{profesional.Usuario.Persona.Nombre} {profesional.Usuario.Persona.Apellido}"
                            : "Profesional Médico";

                        pCol.Item().Text($"Nombre: {nombreProf}");
                        pCol.Item().Text($"Especialidad: {profesional.Especialidad?.ToString() ?? "General"}");
                        pCol.Item().Text($"Matrícula: {profesional.Matricula ?? "N/A"}");
                        pCol.Item().Text($"CUIT: {profesional.Cuit ?? "N/A"}");
                        pCol.Item().Text($"Condición IVA: {profesional.CondicionIva?.ToString() ?? "Responsable Inscripto"}");
                    });

                    // Información del Paciente
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(pCol =>
                    {
                        pCol.Item().Text("DATOS DEL PACIENTE").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                        var nombrePac = paciente.Persona != null 
                            ? $"{paciente.Persona.Nombre} {paciente.Persona.Apellido}"
                            : "Paciente";

                        pCol.Item().Text($"Nombre: {nombrePac}");
                        pCol.Item().Text($"Documento: {paciente.TipoDocumento?.ToString() ?? "DNI"} {paciente.NumDocumento ?? "N/A"}");
                        pCol.Item().Text($"Obra Social: {obraSocial?.Nombre ?? "Particular"}");
                        pCol.Item().Text($"Nº Afiliado: {paciente.NumObraSocial ?? "N/A"}");
                    });

                    // Detalle del Servicio / Turno
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Concepto").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Fecha y Hora").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Importe").Bold();
                        });

                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"Consulta Médica - {profesional.Especialidad?.ToString() ?? "General"}");
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"{turno.Fecha:dd/MM/yyyy} - {turno.Hora:HH:mm} hs");
                        
                        var importeStr = profesional.PrecioConsulta.HasValue 
                            ? $"$ {profesional.PrecioConsulta.Value:N2}" 
                            : "$ 0.00";

                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                            .Text(importeStr).Bold();
                    });

                    // Total
                    col.Item().AlignRight().Text(text =>
                    {
                        var totalStr = profesional.PrecioConsulta.HasValue 
                            ? $"$ {profesional.PrecioConsulta.Value:N2}" 
                            : "$ 0.00";

                        text.Span("Total a Abonar: ").FontSize(13).Bold();
                        text.Span(totalStr).FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                    });

                    if (!string.IsNullOrWhiteSpace(turno.Observacion))
                    {
                        col.Item().Text($"Observaciones: {turno.Observacion}").Italic().FontSize(10);
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().AlignCenter().Text("Este documento es un comprobante de atención generado electrónicamente por Facturno.")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();

        return document;
    }
}
