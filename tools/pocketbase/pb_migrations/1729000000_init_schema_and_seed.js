/// <reference path="../pb_data/types.d.ts" />
migrate((db) => {
  const dao = new Dao(db);

  // 1. Configurar colección 'users' existente
  const usersColl = dao.findCollectionByNameOrId("users");
  usersColl.schema.addField(new SchemaField({
    name: "id_usuario",
    type: "text",
    required: false
  }));
  usersColl.schema.addField(new SchemaField({
    name: "rol",
    type: "text",
    required: false
  }));
  usersColl.schema.addField(new SchemaField({
    name: "activo",
    type: "bool"
  }));
  usersColl.schema.addField(new SchemaField({
    name: "id_persona",
    type: "number"
  }));
  usersColl.listRule = "";
  usersColl.viewRule = "";
  usersColl.createRule = "";
  usersColl.updateRule = "";
  usersColl.deleteRule = "";
  dao.saveCollection(usersColl);

  // 2. Colección 'personas'
  const personas = new Collection({
    name: "personas",
    type: "base",
    schema: [
      new SchemaField({ name: "id_persona", type: "number", required: true }),
      new SchemaField({ name: "nombre", type: "text", required: true }),
      new SchemaField({ name: "apellido", type: "text", required: true }),
      new SchemaField({ name: "correo", type: "email" }),
      new SchemaField({ name: "telefono", type: "number" })
    ],
    listRule: "", viewRule: "", createRule: "", updateRule: "", deleteRule: ""
  });
  dao.saveCollection(personas);

  // 3. Colección 'obras_sociales'
  const obrasSociales = new Collection({
    name: "obras_sociales",
    type: "base",
    schema: [
      new SchemaField({ name: "id_obra_social", type: "number", required: true }),
      new SchemaField({ name: "nombre", type: "text", required: true }),
      new SchemaField({ name: "activo", type: "bool" })
    ],
    listRule: "", viewRule: "", createRule: "", updateRule: "", deleteRule: ""
  });
  dao.saveCollection(obrasSociales);

  // 4. Colección 'profesionales'
  const profesionales = new Collection({
    name: "profesionales",
    type: "base",
    schema: [
      new SchemaField({ name: "id_profesional", type: "text", required: true }),
      new SchemaField({ name: "especialidad", type: "text" }),
      new SchemaField({ name: "matricula", type: "text" }),
      new SchemaField({ name: "cuit", type: "text" }),
      new SchemaField({ name: "precio_consulta", type: "number" }),
      new SchemaField({ name: "tipo_comprobante", type: "text" }),
      new SchemaField({ name: "condicion_iva", type: "text" })
    ],
    listRule: "", viewRule: "", createRule: "", updateRule: "", deleteRule: ""
  });
  dao.saveCollection(profesionales);

  // 5. Colección 'pacientes'
  const pacientes = new Collection({
    name: "pacientes",
    type: "base",
    schema: [
      new SchemaField({ name: "id_paciente", type: "number", required: true }),
      new SchemaField({ name: "num_documento", type: "text" }),
      new SchemaField({ name: "tipo_documento", type: "text" }),
      new SchemaField({ name: "id_obra_social", type: "number" }),
      new SchemaField({ name: "num_obra_social", type: "text" }),
      new SchemaField({ name: "porcentaje_iva", type: "text" })
    ],
    listRule: "", viewRule: "", createRule: "", updateRule: "", deleteRule: ""
  });
  dao.saveCollection(pacientes);

  // 6. Colección 'turnos'
  const turnos = new Collection({
    name: "turnos",
    type: "base",
    schema: [
      new SchemaField({ name: "id_turno", type: "number", required: true }),
      new SchemaField({ name: "id_paciente", type: "number", required: true }),
      new SchemaField({ name: "id_profesional", type: "text", required: true }),
      new SchemaField({ name: "fecha", type: "text", required: true }),
      new SchemaField({ name: "hora", type: "text", required: true }),
      new SchemaField({ name: "estado", type: "text", required: true }),
      new SchemaField({ name: "observacion", type: "text" }),
      new SchemaField({ name: "es_online", type: "bool" }),
      new SchemaField({ name: "modalidad", type: "text" }),
      new SchemaField({ name: "enlace_reunion", type: "text" })
    ],
    listRule: "", viewRule: "", createRule: "", updateRule: "", deleteRule: ""
  });
  dao.saveCollection(turnos);

  // 7. Colección 'agendas_compartidas'
  const agendasCompartidas = new Collection({
    name: "agendas_compartidas",
    type: "base",
    schema: [
      new SchemaField({ name: "id_administrativo", type: "text", required: true }),
      new SchemaField({ name: "id_profesional", type: "text", required: true })
    ],
    listRule: "", viewRule: "", createRule: "", updateRule: "", deleteRule: ""
  });
  dao.saveCollection(agendasCompartidas);

  console.log("[MIGRATION] Colecciones creadas con éxito.");

  // ==========================================
  // SEED DATA
  // ==========================================

  // --- Obras Sociales ---
  const osData = [
    { id: 1, nombre: "Particular", activo: true },
    { id: 2, nombre: "OSDE", activo: true },
    { id: 3, nombre: "Swiss Medical", activo: true },
    { id: 4, nombre: "Galeno", activo: true },
    { id: 5, nombre: "PAMI", activo: true }
  ];

  for (const item of osData) {
    const osRec = new Record(obrasSociales);
    osRec.set("id_obra_social", item.id);
    osRec.set("nombre", item.nombre);
    osRec.set("activo", item.activo);
    dao.saveRecord(osRec);
  }

  // --- Personas ---
  const personasData = [
    { id: 1, nombre: "Admin", apellido: "Operador", correo: "admin@facturno.com", tel: 1122334455 },
    { id: 2, nombre: "Laura", apellido: "Gómez", correo: "laura.gomez@facturno.com", tel: 1144556677 },
    { id: 3, nombre: "Carlos", apellido: "Rossi", correo: "carlos.rossi@facturno.com", tel: 1155667788 },
    { id: 4, nombre: "Marcos", apellido: "Benítez", correo: "marcos.admin@facturno.com", tel: 1166778899 },
    { id: 5, nombre: "Juan", apellido: "Pérez", correo: "juan.perez@gmail.com", tel: 1133445566 },
    { id: 6, nombre: "María", apellido: "Fernández", correo: "maria.f@hotmail.com", tel: 1177889900 },
    { id: 7, nombre: "Lucas", apellido: "Gómez", correo: "lucas.gomez@yahoo.com", tel: 1188990011 },
    { id: 8, nombre: "Sofía", apellido: "Martínez", correo: "sofia.martinez@gmail.com", tel: 1199887766 },
    { id: 9, nombre: "Martín", apellido: "Rodríguez", correo: "martin.rodriguez@outlook.com", tel: 1123456789 },
    { id: 10, nombre: "Camila", apellido: "López", correo: "camila.lopez@gmail.com", tel: 1134567890 },
    { id: 11, nombre: "Diego", apellido: "Morales", correo: "diego.morales@yahoo.com", tel: 1145678901 },
    { id: 12, nombre: "Valentina", apellido: "Herrera", correo: "valen.herrera@hotmail.com", tel: 1156789012 },
    { id: 13, nombre: "Agustín", apellido: "Castro", correo: "agustin.castro@gmail.com", tel: 1167890123 },
    { id: 14, nombre: "Lucía", apellido: "Romero", correo: "lucia.romero@gmail.com", tel: 1178901234 },
    { id: 15, nombre: "Esteban", apellido: "Díaz", correo: "esteban.diaz@outlook.com", tel: 1189012345 }
  ];

  for (const p of personasData) {
    const pRec = new Record(personas);
    pRec.set("id_persona", p.id);
    pRec.set("nombre", p.nombre);
    pRec.set("apellido", p.apellido);
    pRec.set("correo", p.correo);
    pRec.set("telefono", p.tel);
    dao.saveRecord(pRec);
  }

  // --- Usuarios (Auth) ---
  const idAdmin = "11111111-1111-1111-1111-111111111111";
  const idLaura = "22222222-2222-2222-2222-222222222222";
  const idCarlos = "33333333-3333-3333-3333-333333333333";
  const idMarcos = "44444444-4444-4444-4444-444444444444";

  const usersData = [
    { email: "admin@facturno.com", pass: "admin123456", rol: "Operador", idPersona: 1, guid: idAdmin },
    { email: "laura.gomez@facturno.com", pass: "laura123!", rol: "Profesional", idPersona: 2, guid: idLaura },
    { email: "carlos.rossi@facturno.com", pass: "carlos123!", rol: "Profesional", idPersona: 3, guid: idCarlos },
    { email: "marcos.admin@facturno.com", pass: "marcos123!", rol: "Administrativo", idPersona: 4, guid: idMarcos }
  ];

  for (const u of usersData) {
    const uRec = new Record(usersColl);
    const username = u.email.split("@")[0].replace(".", "_");
    uRec.setUsername(username);
    uRec.setEmail(u.email);
    uRec.setPassword(u.pass);
    uRec.set("id_usuario", u.guid);
    uRec.set("rol", u.rol);
    uRec.set("activo", true);
    uRec.set("id_persona", u.idPersona);
    dao.saveRecord(uRec);
  }

  // --- Profesionales ---
  const prof1 = new Record(profesionales);
  prof1.set("id_profesional", idLaura);
  prof1.set("especialidad", "Psicologo");
  prof1.set("matricula", "PSI-1024");
  prof1.set("cuit", "27-32104567-4");
  prof1.set("precio_consulta", 15000);
  prof1.set("tipo_comprobante", "FacturaC");
  prof1.set("condicion_iva", "Monotributo");
  dao.saveRecord(prof1);

  const prof2 = new Record(profesionales);
  prof2.set("id_profesional", idCarlos);
  prof2.set("especialidad", "Psiquiatra");
  prof2.set("matricula", "MED-8842");
  prof2.set("cuit", "20-29876543-2");
  prof2.set("precio_consulta", 22000);
  prof2.set("tipo_comprobante", "FacturaC");
  prof2.set("condicion_iva", "Monotributo");
  dao.saveRecord(prof2);

  // --- Pacientes ---
  const pacData = [
    { id: 5, dni: "35123456", td: "DNI", os: 2, numOs: "OS-994821", iva: "Exento" },
    { id: 6, dni: "28987654", td: "DNI", os: 3, numOs: "SM-441029", iva: "Exento" },
    { id: 7, dni: "42111222", td: "DNI", os: 1, numOs: "", iva: "ConsumidorFinal" },
    { id: 8, dni: "38999111", td: "DNI", os: 2, numOs: "OS-112233", iva: "Exento" },
    { id: 9, dni: "31444555", td: "DNI", os: 4, numOs: "GAL-554433", iva: "Exento" },
    { id: 10, dni: "44222333", td: "DNI", os: 3, numOs: "SM-778899", iva: "Exento" },
    { id: 11, dni: "26555666", td: "DNI", os: 5, numOs: "PAM-998877", iva: "Exento" },
    { id: 12, dni: "39111444", td: "DNI", os: 2, numOs: "OS-665544", iva: "Exento" },
    { id: 13, dni: "41777888", td: "DNI", os: 1, numOs: "", iva: "ConsumidorFinal" },
    { id: 14, dni: "33888999", td: "DNI", os: 4, numOs: "GAL-221100", iva: "Exento" },
    { id: 15, dni: "29333222", td: "DNI", os: 5, numOs: "PAM-443322", iva: "Exento" }
  ];

  for (const pac of pacData) {
    const pacRec = new Record(pacientes);
    pacRec.set("id_paciente", pac.id);
    pacRec.set("num_documento", pac.dni);
    pacRec.set("tipo_documento", pac.td);
    pacRec.set("id_obra_social", pac.os);
    pacRec.set("num_obra_social", pac.numOs);
    pacRec.set("porcentaje_iva", pac.iva);
    dao.saveRecord(pacRec);
  }

  // --- Agendas Compartidas (Marcos Benítez vinculado a Dra. Laura Gómez) ---
  const agenda1 = new Record(agendasCompartidas);
  agenda1.set("id_administrativo", idMarcos);
  agenda1.set("id_profesional", idLaura);
  dao.saveRecord(agenda1);

  // --- Turnos de Prueba calculados para la semana activa actual ---
  const hoy = new Date();
  const formatFecha = (d) => {
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  const getLunes = (d) => {
    const copy = new Date(d);
    const day = copy.getDay();
    const diff = (day === 0 ? -6 : 1) - day;
    copy.setDate(copy.getDate() + diff);
    return copy;
  };

  const baseLunes = getLunes(hoy);
  const getFechaSemana = (offsetDias) => {
    const d = new Date(baseLunes);
    d.setDate(baseLunes.getDate() + offsetDias);
    return formatFecha(d);
  };

  const fLunes = getFechaSemana(0);
  const fMartes = getFechaSemana(1);
  const fMiercoles = getFechaSemana(2);
  const fJueves = getFechaSemana(3);
  const fViernes = getFechaSemana(4);
  const fSabado = getFechaSemana(5);
  const fDomingo = getFechaSemana(6);

  const turnosData = [
    // LUNES
    { id: 1, pac: 5, prof: idLaura, fecha: fLunes, hora: "09:00:00", estado: "Completo", obs: "Primera consulta de evaluación", online: false, mod: "Presencial", enlace: "" },
    { id: 2, pac: 6, prof: idLaura, fecha: fLunes, hora: "10:30:00", estado: "Completo", obs: "Sesión de psicoterapia", online: true, mod: "Online", enlace: "https://meet.google.com/abc-defg-hij" },
    { id: 3, pac: 8, prof: idLaura, fecha: fLunes, hora: "12:00:00", estado: "Activo", obs: "Seguimiento quincenal", online: false, mod: "Presencial", enlace: "" },
    { id: 4, pac: 9, prof: idCarlos, fecha: fLunes, hora: "16:00:00", estado: "Activo", obs: "Evaluación psiquiátrica inicial", online: false, mod: "Presencial", enlace: "" },

    // MARTES (HOY)
    { id: 5, pac: 10, prof: idLaura, fecha: fMartes, hora: "09:00:00", estado: "Activo", obs: "Consulta online con enlace Zoom", online: true, mod: "Online", enlace: "https://zoom.us/j/1234567890" },
    { id: 6, pac: 11, prof: idLaura, fecha: fMartes, hora: "10:00:00", estado: "Completo", obs: "Control clínico y emisión de certificado", online: false, mod: "Presencial", enlace: "" },
    { id: 7, pac: 12, prof: idLaura, fecha: fMartes, hora: "11:30:00", estado: "Falta", obs: "Paciente avisó falta a último momento", online: false, mod: "Presencial", enlace: "" },
    { id: 8, pac: 7, prof: idLaura, fecha: fMartes, hora: "14:00:00", estado: "Cancelado", obs: "Cancelado por reprogramación", online: false, mod: "Presencial", enlace: "" },
    { id: 9, pac: 13, prof: idLaura, fecha: fMartes, hora: "15:30:00", estado: "Activo", obs: "Consulta online pendiente de configurar link", online: true, mod: "Online", enlace: "" },
    { id: 10, pac: 14, prof: idLaura, fecha: fMartes, hora: "17:00:00", estado: "Activo", obs: "Sesión regular", online: false, mod: "Presencial", enlace: "" },
    { id: 11, pac: 5, prof: idCarlos, fecha: fMartes, hora: "10:30:00", estado: "Completo", obs: "Ajuste de esquema farmacológico", online: false, mod: "Presencial", enlace: "" },

    // MIÉRCOLES
    { id: 12, pac: 6, prof: idLaura, fecha: fMiercoles, hora: "09:30:00", estado: "Activo", obs: "Seguimiento semanal", online: false, mod: "Presencial", enlace: "" },
    { id: 13, pac: 9, prof: idLaura, fecha: fMiercoles, hora: "11:00:00", estado: "Activo", obs: "Videollamada médica programada", online: true, mod: "Online", enlace: "https://meet.jit.si/facturno-consulta-9" },
    { id: 14, pac: 8, prof: idLaura, fecha: fMiercoles, hora: "15:00:00", estado: "Activo", obs: "Terapia de apoyo", online: false, mod: "Presencial", enlace: "" },

    // JUEVES
    { id: 15, pac: 11, prof: idLaura, fecha: fJueves, hora: "09:00:00", estado: "Activo", obs: "Evaluación psicodiagnóstica", online: false, mod: "Presencial", enlace: "" },
    { id: 16, pac: 12, prof: idLaura, fecha: fJueves, hora: "10:30:00", estado: "Completo", obs: "Informe y devolución", online: true, mod: "Online", enlace: "https://meet.google.com/xyz-uvwx-rst" },
    { id: 17, pac: 15, prof: idLaura, fecha: fJueves, hora: "16:00:00", estado: "Activo", obs: "Entrevista de admisión", online: false, mod: "Presencial", enlace: "" },

    // VIERNES
    { id: 18, pac: 14, prof: idLaura, fecha: fViernes, hora: "09:00:00", estado: "Activo", obs: "Revisión clínica", online: false, mod: "Presencial", enlace: "" },
    { id: 19, pac: 13, prof: idLaura, fecha: fViernes, hora: "11:00:00", estado: "Activo", obs: "Atención remota", online: true, mod: "Online", enlace: "https://meet.jit.si/facturno-viernes" },
    { id: 20, pac: 10, prof: idLaura, fecha: fViernes, hora: "14:30:00", estado: "Completo", obs: "Cierre de tratamiento", online: false, mod: "Presencial", enlace: "" },

    // SÁBADO (Sub-tarjeta superior Fin de Semana)
    { id: 21, pac: 8, prof: idLaura, fecha: fSabado, hora: "09:30:00", estado: "Activo", obs: "Guardia programada de fin de semana", online: false, mod: "Presencial", enlace: "" },
    { id: 22, pac: 9, prof: idLaura, fecha: fSabado, hora: "11:00:00", estado: "Activo", obs: "Teleconsulta sabatina", online: true, mod: "Online", enlace: "https://meet.google.com/sab-cons-11" },
    { id: 23, pac: 5, prof: idLaura, fecha: fSabado, hora: "12:30:00", estado: "Completo", obs: "Urgencia menor ambulatoria", online: false, mod: "Presencial", enlace: "" },

    // DOMINGO (Sub-tarjeta inferior Fin de Semana)
    { id: 24, pac: 11, prof: idLaura, fecha: fDomingo, hora: "10:00:00", estado: "Activo", obs: "Control médico dominical a distancia", online: true, mod: "Online", enlace: "https://meet.jit.si/facturno-domingo-10" },
    { id: 25, pac: 12, prof: idLaura, fecha: fDomingo, hora: "11:30:00", estado: "Activo", obs: "Atención de soporte emocional", online: false, mod: "Presencial", enlace: "" }
  ];

  for (const t of turnosData) {
    const tRec = new Record(turnos);
    tRec.set("id_turno", t.id);
    tRec.set("id_paciente", t.pac);
    tRec.set("id_profesional", t.prof);
    tRec.set("fecha", t.fecha);
    tRec.set("hora", t.hora);
    tRec.set("estado", t.estado);
    tRec.set("observacion", t.obs);
    tRec.set("es_online", t.online);
    tRec.set("modalidad", t.mod);
    tRec.set("enlace_reunion", t.enlace || "");
    dao.saveRecord(tRec);
  }

  console.log("[MIGRATION] Seed data insertada con éxito.");
}, (db) => {
  // rollback
});
