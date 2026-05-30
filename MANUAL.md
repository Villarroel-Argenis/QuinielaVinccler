# Manual de Usuario — Quiniela Vinccler FIFA 2026

Guía completa de uso del portal de la quiniela. Dividida en dos secciones según el tipo de cuenta.

## Índice

- [Para usuarios comunes](#para-usuarios-comunes)
  - [Registro e inicio de sesión](#registro-e-inicio-de-sesión)
  - [Vincular una planilla](#vincular-una-planilla)
  - [Hacer predicciones](#hacer-predicciones)
  - [Cambiar tu contraseña](#cambiar-tu-contraseña)
  - [Ver el ranking](#ver-el-ranking)
  - [Premios](#premios)
- [Para administradores](#para-administradores)
  - [1. Generar Planillas](#1-generar-planillas)
  - [2. Gestión de Planillas](#2-gestión-de-planillas)
  - [3. Usuarios](#3-usuarios)
  - [4. Resultados](#4-resultados)
  - [5. Configuración](#5-configuración)
- [Flujo típico de un torneo](#flujo-típico-de-un-torneo)

---

# Para usuarios comunes

## Registro e inicio de sesión

1. Entra a la URL del portal.
2. Si no tienes cuenta, haz clic en **Registrarse**.
3. Completa: nombre completo, email, cédula, teléfono, contraseña (mínimo 6 caracteres).
4. Inicia sesión con tu email y contraseña.

> Si olvidas tu contraseña, contacta al administrador para que te genere una temporal. No hay recuperación automática por email.

## Vincular una planilla

1. El admin te entrega una planilla física con un código tipo `P-12345678`.
2. En **Mis Planillas**, ingresa el código en el campo y presiona **Vincular**.
3. La planilla queda asociada a tu cuenta.

> Una vez cerrada la quiniela ya no podrás vincular nuevas planillas, solo consultar las que ya tienes.

## Hacer predicciones

1. En **Mis Planillas**, haz clic en **Predecir** sobre una planilla.
2. Recorre las pestañas:
   - **Fase de Grupos** — selecciona 1/X/2 para cada uno de los 72 partidos.
   - **Dieciseisavos** — elige los 32 equipos que pasarán a R32 (16 primeros de grupo + 16 segundos + 8 mejores terceros).
   - **Octavos / Cuartos / Semis / 3°-4°** — selecciona los ganadores de cada partido (dropdowns en cascada).
   - **Gran Final** — define campeón, posiciones finales, equipos goleadores y marcadores exactos.
3. Cada predicción se guarda automáticamente al seleccionarla.
4. El contador arriba muestra cuántas casillas llevas completadas.

> **Recomendación para R32:** completa primero los **primeros** y **segundos** de cada grupo (slots libres). Deja para el final los **mejores terceros**, que dependen de cuáles grupos ya hayan sido usados.

## Cambiar tu contraseña

1. Ve a **Mi Perfil** en el menú lateral.
2. Ingresa tu contraseña actual.
3. Define la nueva (mínimo 6 caracteres).
4. Confirma y guarda.

## Ver el ranking

En **Ranking** verás el listado de todas las planillas ordenadas por puntaje.

- 🥇🥈🥉 Las primeras 3 posiciones se muestran con medallas.
- 🙃 La posición con menor puntaje también se marca.
- Tu fila aparece resaltada en azul.
- Al cargar, la página salta automáticamente a donde está tu mejor planilla.
- Puedes filtrar **"Ver solo mis planillas"** para ver únicamente las tuyas (mantienen sus posiciones reales en el ranking global).
- Selector de **10 / 20 / 50 filas por página**.
- Click en el código de cualquier planilla abre el **detalle completo**:
  - ✓ verde donde acertaste.
  - ✗ rojo donde fallaste (pasa el cursor sobre la ✗ para ver el equipo real).
  - Puntos obtenidos por partido.
  - Barras de progreso de predicciones completadas y resultados disponibles.

## Premios

- 🥇 **1er Premio:** $2,000 + 50% de lo recaudado
- 🥈 **2do Premio:** $1,000 + 30% de lo recaudado
- 🥉 **3er Premio:** $500 + 20% de lo recaudado
- 🙃 **Menor puntaje:** $2,000

En caso de empate, el premio se reparte proporcionalmente entre los empatados.

---

# Para administradores

El panel admin está en `/admin` (solo accesible con cuenta de rol Admin).

## 1. Generar Planillas

Crea lotes de planillas físicas para imprimir y repartir.

1. Define la **cantidad** (1 a 50).
2. Click en **Generar Lote**.
3. El sistema crea los códigos únicos.
4. Haz clic en **Imprimir PDF** sobre el lote generado para descargar el archivo imprimible.

Para **eliminar un lote**, usa el botón rojo de **Eliminar**. Solo se pueden eliminar lotes sin planillas asignadas — si una planilla del lote ya fue vinculada a un usuario, primero hay que desvincularla.

> **Bloqueo:** cuando la quiniela esté cerrada, no se pueden generar ni eliminar lotes.

## 2. Gestión de Planillas

Busca cualquier planilla por código o por usuario. Útil cuando:
- Hubo un error de asignación y necesitas **desvincular** una planilla del usuario equivocado.
- Necesitas confirmar a qué usuario está asignada determinada planilla.

Una vez desvinculada, la planilla queda libre y el usuario original puede volver a vincularla, o se le puede asignar al usuario correcto.

## 3. Usuarios

Mantenimiento de cuentas registradas:

- **Buscar** por email, nombre o cédula.
- **Filtro "Incluir administradores"** para ocultar/mostrar otros admins en el listado.
- **Paginación** de 20 por página.
- **Bloquear/Desbloquear** un usuario. Un usuario bloqueado no podrá iniciar sesión. **No se puede bloquear a otros administradores.**
- **Resetear contraseña** — genera una contraseña temporal aleatoria de 10 caracteres y la muestra en pantalla.

> **Importante:** la contraseña temporal solo se muestra una vez. **Cópiala y compártela con el usuario por un canal seguro** (WhatsApp, en persona, llamada). No se guarda en ningún log y no la podrás recuperar después. Recomiéndale al usuario cambiarla desde su perfil al iniciar sesión.

## 4. Resultados

Registra los resultados oficiales del Mundial. Una pestaña por fase:

- **Fase de Grupos** — selecciona 1/X/2 para cada partido.
- **Dieciseisavos** — selecciona los 32 equipos que clasificaron a R32.
- **Octavos / Cuartos / Semis / 3°-4° / Gran Final** — selecciona los ganadores de cada partido. Los dropdowns muestran solo los equipos válidos según las fases anteriores.
- **Posiciones Finales** — campeón, sub-campeón, 3°, 4°, equipo más goleador, más goleado y menos goleado (todos de la fase eliminatoria).

Cada pestaña tiene su propio botón **Guardar** flotante. Al guardar, el sistema **recalcula automáticamente los puntos** de todas las planillas afectadas por ese cambio.

### Resetear todos los puntos y resultados

Botón rojo arriba a la derecha. **Borra todo:**
- Todos los puntos de todas las planillas.
- Todos los resultados de todos los partidos.
- Las posiciones finales y marcadores exactos.
- Todos los puntajes totales.

Pide confirmación antes de ejecutar. **No se puede deshacer.** Úsalo solo si necesitas empezar desde cero (por error grave en la captura de resultados, prueba inicial, etc.).

## 5. Configuración

Controla el cierre de la quiniela.

### Cartel de estado

Arriba aparece un cartel coloreado indicando el estado actual:
- 🟢 **Quiniela ABIERTA** — sin restricciones activas.
- 🟡 **Quiniela ABIERTA — cierre programado** — muestra fecha y tiempo restante hasta el cierre.
- 🔴 **Quiniela CERRADA** — ya sea por fecha pasada o por cierre manual.

### Cierre manual

Toggle que fuerza el cierre **inmediatamente**, sin importar la fecha programada. Útil si necesitas adelantar el cierre por alguna razón (problema técnico, cambio de fecha del torneo, etc.).

### Cierre programado

Define una fecha y hora límite. Al pasar ese momento, la quiniela se cierra automáticamente.

> Ambos mecanismos son **independientes**: cualquiera de los dos basta para cerrar la quiniela. Por ejemplo, puedes tener una fecha programada y además activar el cierre manual antes de que llegue esa fecha.

### Efectos del cierre

Cuando la quiniela está cerrada:
- Los usuarios no pueden vincular nuevas planillas (verán un aviso).
- No se pueden generar nuevos lotes.
- No se pueden eliminar lotes existentes.
- Los usuarios solo pueden **ver** sus planillas en modo lectura, no editar predicciones.

---

# Flujo típico de un torneo

### Antes del torneo

1. **Admin genera lotes** de planillas en `/admin → Generar Planillas`.
2. **Imprime los PDFs** y reparte las planillas físicas a los empleados.
3. **Configura la fecha de cierre** en `/admin → Configuración` (ejemplo: 11 de junio antes del primer partido).

### Los usuarios

1. **Se registran** en el portal.
2. **Vinculan su planilla física** con el código.
3. **Completan sus 149 predicciones** antes de la fecha límite.

### Durante el torneo

1. Cada vez que termina un partido, el admin entra a `/admin → Resultados`.
2. Selecciona el resultado correspondiente y guarda.
3. Los puntos se **recalculan automáticamente** para todas las planillas.
4. Los usuarios ven sus puntos actualizados en el ranking.

### Al final del torneo

1. Admin registra los **marcadores exactos** de semifinales y final.
2. Admin registra las **posiciones finales** y los equipos extras (goleador, goleado, menos goleado).
3. El **ranking final** determina los ganadores de los premios.
