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
    { id: 7, nombre: "Lucas", apellido: "Gómez", correo: "lucas.gomez@yahoo.com", tel: 1188990011 }
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
    { id: 7, dni: "42111222", td: "DNI", os: 1, numOs: "", iva: "ConsumidorFinal" }
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

  // --- Turnos de Prueba ---
  const hoy = new Date();
  const formatFecha = (d) => {
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  const manana = new Date(hoy);
  manana.setDate(hoy.getDate() + 1);

  const pasadoManana = new Date(hoy);
  pasadoManana.setDate(hoy.getDate() + 2);

  const turnosData = [
    { id: 1, pac: 5, prof: idLaura, fecha: formatFecha(hoy), hora: "09:00:00", estado: "Activo", obs: "Primera consulta de evaluación" },
    { id: 2, pac: 6, prof: idLaura, fecha: formatFecha(hoy), hora: "10:00:00", estado: "Completo", obs: "Sesión de seguimiento semanal" },
    { id: 3, pac: 7, prof: idLaura, fecha: formatFecha(hoy), hora: "11:30:00", estado: "Falta", obs: "Paciente avisó tarde" },
    { id: 4, pac: 5, prof: idCarlos, fecha: formatFecha(manana), hora: "10:00:00", estado: "Activo", obs: "Control de medicación" },
    { id: 5, pac: 6, prof: idLaura, fecha: formatFecha(pasadoManana), hora: "15:00:00", estado: "Activo", obs: "Sesión quincenal" }
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
    dao.saveRecord(tRec);
  }

  console.log("[MIGRATION] Seed data insertada con éxito.");
}, (db) => {
  // rollback
});
