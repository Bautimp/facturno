/// <reference path="../pb_data/types.d.ts" />
migrate((db) => {
  const dao = new Dao(db);
  const turnos = dao.findCollectionByNameOrId("turnos");

  let modified = false;
  try {
    if (!turnos.schema.getFieldByName("es_online")) {
      turnos.schema.addField(new SchemaField({
        name: "es_online",
        type: "bool",
        required: false
      }));
      modified = true;
    }
  } catch (e) {
    turnos.schema.addField(new SchemaField({
      name: "es_online",
      type: "bool",
      required: false
    }));
    modified = true;
  }

  try {
    if (!turnos.schema.getFieldByName("modalidad")) {
      turnos.schema.addField(new SchemaField({
        name: "modalidad",
        type: "text",
        required: false
      }));
      modified = true;
    }
  } catch (e) {
    turnos.schema.addField(new SchemaField({
      name: "modalidad",
      type: "text",
      required: false
    }));
    modified = true;
  }

  try {
    if (!turnos.schema.getFieldByName("enlace_reunion")) {
      turnos.schema.addField(new SchemaField({
        name: "enlace_reunion",
        type: "text",
        required: false
      }));
      modified = true;
    }
  } catch (e) {
    turnos.schema.addField(new SchemaField({
      name: "enlace_reunion",
      type: "text",
      required: false
    }));
    modified = true;
  }

  if (modified) {
    dao.saveCollection(turnos);
  }
  console.log("[MIGRATION] Campos de modalidad online verificados en 'turnos'.");

  try {
    const records = dao.findRecordsByFilter("turnos", "id_turno=1", "", 1);
    if (records && records.length > 0) {
      records[0].set("es_online", true);
      records[0].set("modalidad", "Online");
      records[0].set("enlace_reunion", "https://meet.jit.si/facturno-consulta-evaluacion");
      dao.saveRecord(records[0]);
      console.log("[MIGRATION] Turno #1 configurado como modalidad Online con enlace.");
    }
  } catch (e) {
    console.log("[MIGRATION] Nota: No se pudo actualizar turno inicial #1:", e);
  }
}, (db) => {
  // rollback
});
