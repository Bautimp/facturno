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
      new SchemaField({ name: "telefono", type: "number" }),
      new SchemaField({ name: "activo", type: "bool" })
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

  // 6. Colección 'turnos' (Unificada con campos de modalidad presencial/online)
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

  console.log("[MIGRATION] Colecciones y esquemas creados con éxito.");

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
    { id: 9, nombre: "Martín", apellido: "Rodríguez", correo: "martin.rodriguez@outlook.com", tel: 1123456789, activo: true },
    { id: 10, nombre: "Camila", apellido: "López", correo: "camila.lopez@gmail.com", tel: 1134567890, activo: false },
    { id: 11, nombre: "Diego", apellido: "Morales", correo: "diego.morales@yahoo.com", tel: 1145678901, activo: true },
    { id: 12, nombre: "Valentina", apellido: "Herrera", correo: "valen.herrera@hotmail.com", tel: 1156789012, activo: true },
    { id: 13, nombre: "Agustín", apellido: "Castro", correo: "agustin.castro@gmail.com", tel: 1167890123, activo: true },
    { id: 14, nombre: "Lucía", apellido: "Romero", correo: "lucia.romero@gmail.com", tel: 1178901234, activo: false },
    { id: 15, nombre: "Esteban", apellido: "Díaz", correo: "esteban.diaz@outlook.com", tel: 1189012345, activo: true }
  ];

  for (const p of personasData) {
    const pRec = new Record(personas);
    pRec.set("id_persona", p.id);
    pRec.set("nombre", p.nombre);
    pRec.set("apellido", p.apellido);
    pRec.set("correo", p.correo);
    pRec.set("telefono", p.tel);
    pRec.set("activo", p.activo !== undefined ? p.activo : true);
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
  // Se establece explícitamente el estado 'activo' para cada paciente en el consultorio
  const pacData = [
    { id: 5, dni: "35123456", td: "DNI", os: 2, numOs: "OS-994821", iva: "Exento", activo: true },
    { id: 6, dni: "28987654", td: "DNI", os: 3, numOs: "SM-441029", iva: "Exento", activo: true },
    { id: 7, dni: "42111222", td: "DNI", os: 1, numOs: "", iva: "ConsumidorFinal", activo: true },
    { id: 8, dni: "38999111", td: "DNI", os: 2, numOs: "OS-112233", iva: "Exento", activo: true },
    { id: 9, dni: "31444555", td: "DNI", os: 4, numOs: "GAL-554433", iva: "Exento", activo: true },
    { id: 10, dni: "44222333", td: "DNI", os: 3, numOs: "SM-778899", iva: "Exento", activo: false },
    { id: 11, dni: "26555666", td: "DNI", os: 5, numOs: "PAM-998877", iva: "Exento", activo: true },
    { id: 12, dni: "39111444", td: "DNI", os: 2, numOs: "OS-665544", iva: "Exento", activo: true },
    { id: 13, dni: "41777888", td: "DNI", os: 1, numOs: "", iva: "ConsumidorFinal", activo: true },
    { id: 14, dni: "33888999", td: "DNI", os: 4, numOs: "GAL-221100", iva: "Exento", activo: false },
    { id: 15, dni: "29333222", td: "DNI", os: 5, numOs: "PAM-443322", iva: "Exento", activo: true }
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

  // --- Generación de Turnos de Prueba hasta fines de Noviembre 2026 ---
  const diasNoviembre = [
    // Semana 1
    "2026-11-02", "2026-11-03", "2026-11-04", "2026-11-05", "2026-11-06",
    // Semana 2
    "2026-11-09", "2026-11-10", "2026-11-11", "2026-11-12", "2026-11-13",
    // Semana 3
    "2026-11-16", "2026-11-17", "2026-11-18", "2026-11-19", "2026-11-20",
    // Semana 4
    "2026-11-23", "2026-11-24", "2026-11-25", "2026-11-26", "2026-11-27",
    // Semana 5
    "2026-11-30"
  ];

  // Pacientes activos habilitados para asignación de turnos
  const pacientesActivosPool = [5, 6, 7, 8, 9, 11, 12, 13, 15];

  // Plantillas de turnos para Dra. Laura Gómez (Psicóloga)
  const slotsLaura = [
    { hora: "09:00:00", mod: "Presencial", obs: "Sesión de psicoterapia individual" },
    { hora: "10:30:00", mod: "Online", obs: "Consulta de psicología a distancia", enlace: "https://meet.google.com/fac-psi-laura" },
    { hora: "14:00:00", mod: "Presencial", obs: "Evaluación psicodiagnóstica" },
    { hora: "16:00:00", mod: "Presencial", obs: "Seguimiento terapéutico"}
  ];

  // Plantillas de turnos para Dr. Carlos Rossi (Psiquiatra)
  const slotsCarlos = [
    { hora: "11:00:00", mod: "Presencial", obs: "Evaluación y control psiquiátrico" },
    { hora: "12:30:00", mod: "Presencial", obs: "Consulta psiquiátrica" },
    { hora: "15:30:00", mod: "Presencial", obs: "Ajuste de esquema psicofarmacológico" }
  ];

  const fechaHoyStr = "2026-11-05";
  let idTurnoSeq = 1;
  let pacIndex = 0;

  for (const fecha of diasNoviembre) {
    const esPasado = fecha < fechaHoyStr;

    // 1. Citas para Dra. Laura Gómez
    for (let i = 0; i < slotsLaura.length; i++) {
      const slot = slotsLaura[i];
      const pacId = pacientesActivosPool[pacIndex % pacientesActivosPool.length];
      pacIndex++;

      let estado = "Activo";
      if (esPasado) {
        estado = (i === 2 && fecha === "2026-11-03") ? "Falta" : "Completo";
      } else if (fecha === "2026-11-10" && i === 1) {
        estado = "Cancelado";
      }

      const tRec = new Record(turnos);
      tRec.set("id_turno", idTurnoSeq++);
      tRec.set("id_paciente", pacId);
      tRec.set("id_profesional", idLaura);
      tRec.set("fecha", fecha);
      tRec.set("hora", slot.hora);
      tRec.set("estado", estado);
      tRec.set("observacion", slot.obs);
      tRec.set("es_online", slot.mod === "Online");
      tRec.set("modalidad", slot.mod);
      tRec.set("enlace_reunion", slot.enlace || "");
      dao.saveRecord(tRec);
    }

    // 2. Citas para Dr. Carlos Rossi
    for (let j = 0; j < slotsCarlos.length; j++) {
      const slot = slotsCarlos[j];
      const pacId = pacientesActivosPool[pacIndex % pacientesActivosPool.length];
      pacIndex++;

      let estado = "Activo";
      if (esPasado) {
        estado = "Completo";
      } else if (fecha === "2026-11-17" && j === 0) {
        estado = "Cancelado";
      }

      const tRec = new Record(turnos);
      tRec.set("id_turno", idTurnoSeq++);
      tRec.set("id_paciente", pacId);
      tRec.set("id_profesional", idCarlos);
      tRec.set("fecha", fecha);
      tRec.set("hora", slot.hora);
      tRec.set("estado", estado);
      tRec.set("observacion", slot.obs);
      tRec.set("es_online", slot.mod === "Online");
      tRec.set("modalidad", slot.mod);
      tRec.set("enlace_reunion", slot.enlace || "");
      dao.saveRecord(tRec);
    }
  }

  console.log(`[MIGRATION] Seed data insertada con éxito: ${idTurnoSeq - 1} turnos generados hasta el 30 de noviembre.`);
}, (db) => {
  // rollback
});
