#TUW #Facturno
Facturno es el proyecto de agenda, turnero y facturación semi y/o automática para el Centro Psicológico y Psiquiátrico Buen Pastor.
En un principio este proyecto tiene el objetivo de ser desarrollado como reemplazo de las Prácticas Supervisadas para la carrera de [[Tecnicatura Universitaria en Web||Tecnicatura Universitaria en desarrollo Web]].

---

El proyecto desarrollado es una aplicación web para realizar tareas de gestión de un centro médico, en donde las funcionalidades principales son las de turnero y facturador. Para llevar a cabo estas funcionalidades el sistema está construido alrededor de agendas, correspondiendo una a cada profesional.

Dentro del sistema existen tres roles; Administrativo, Profesional y Operador.
- Administrativo: el personal administrativo tendrá acceso a todas las agendas.
- Profesional: los profesionales dentro del centro tendrán acceso únicamente a su agenda.
- Operador: los operadores del sistema tendrán acceso a todas las agendas.

Las agendas permitirán visualizar, crear, modificar y eliminar turnos de pacientes, permitiendo establecer distintos estados (activo, cancelado, falta, completo).

Las funcionalidades de facturación serán llevadas a cabo a través del ARCA (en un entorno de desarrollo) como un módulo al que tendrán acceso algunos profesionales, pudiendo facturar únicamente sobre su agenda personal. Este debe permitir, en función de los datos del paciente, el precio de la consulta, y otras variables, realizar una facturación parcial o completamente automática en función de los pacientes de su agenda.

Debido a los problemas que puede traer el realizar facturas utilizando la API del ARCA (solo ciertos profesionales podrían debido a los requisitos establecidos por la entidad recaudadora), se plantea la utilización del entorno de desarrollo del mismo, el cual permitiría realizar facturas ficticias. En caso que se presente alguna dificultad ante este módulo, se podría reemplazar por un generador de archivos en formato pdf que simula una factura.

---

Este proyecto cuenta con distintas aspectos a desarrollar.
1. Sitio web con el acceso de distintos usuarios (Profesional, Secretaria)
2. [[Agenda]] con alta, baja y modificación de turnos.
3. Conexión con la [[API de ARCA]].

---

Para poder extraer los requerimientos del sistema de desarrollan los siguientes diagramas:
1. [[Diagrama de Casos de Uso]]
2. [[Diagrama de Clases]]

---

El Stack utilizado para el desarrollo consiste de:
- Frontend
	- Blazor
- Backend
	- Lógica
		- .NET (C#)
	- Base de Datos
		- Supabase
	- API
		- ARCA web service SOAP

Para este desarrollo se utilizará como IDE Visual Studio Code, debido a su versatilidad con lenguajes y facilidad de vinculación con extensiones útiles.

Estructura del proyecto