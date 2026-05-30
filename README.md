# Quiniela Vinccler FIFA 2026

Portal interno de quiniela para el Mundial FIFA 2026. Los empleados de Vinccler C.A. reciben planillas físicas con un código único, las vinculan a su cuenta en el sistema, registran sus predicciones para los 104 partidos del torneo y compiten por premios en un ranking global.

## Stack

- **.NET 10** + **MudBlazor Server** (interactivity Server, sin WASM)
- **Entity Framework Core** + **PostgreSQL**
- **Autenticación** cookie-based con BCrypt
- **QuestPDF** para generación de planillas imprimibles

## Estructura del proyecto

```
QuinielaVinccler.UI.Web/
├── Components/
│   ├── Admin/              # Componentes del panel admin
│   │   ├── ConfiguracionComponent
│   │   ├── GestionPlanillasComponent
│   │   ├── KnockoutResultadoRow
│   │   ├── LoteComponent
│   │   ├── R32ResultadoRow
│   │   ├── ResultadosComponent
│   │   └── UsuariosComponent
│   ├── Pages/              # Páginas de ruta
│   │   ├── Admin
│   │   ├── MisPlanillas
│   │   ├── Perfil
│   │   ├── PlanillaDetalle
│   │   ├── Ranking
│   │   └── Auth/Login, Auth/Register
│   ├── Shared/             # Componentes compartidos
│   │   ├── FinalEquipoRow
│   │   ├── KnockoutRow
│   │   ├── MudEquipoSelect
│   │   └── PlanillaModal
│   └── Layout/
├── Data/
│   ├── Models/             # Entidades EF Core
│   ├── Migrations/
│   └── AppDbContext.cs
├── Services/               # Lógica de negocio
│   ├── AuthService
│   ├── ConfiguracionService
│   ├── LoteService
│   ├── PlanillaService
│   ├── PredictionService
│   ├── PuntuacionService
│   ├── UserService
│   └── PdfService
└── Program.cs
```

## Configuración local

### 1. Requisitos

- .NET 10 SDK
- PostgreSQL 15+
- Visual Studio 2022 o Rider (opcional)

### 2. Base de datos

Crea una base de datos PostgreSQL llamada `quiniela_vinccler`:

```sql
CREATE DATABASE quiniela_vinccler;
CREATE USER admin WITH PASSWORD 'admin123';
GRANT ALL PRIVILEGES ON DATABASE quiniela_vinccler TO admin;
```

La cadena de conexión está en `appsettings.json`:

```
Host=localhost;Database=quiniela_vinccler;Username=admin;Password=admin123
```

### 3. Migraciones

Aplica las migraciones de Entity Framework:

```bash
dotnet ef database update
```

Esto crea todas las tablas y ejecuta los seeders:
- 48 equipos del Mundial
- 104 partidos pre-configurados
- 12 grupos
- Usuario admin: `admin@quinielavinccler.com` / `admin2026`

### 4. Ejecutar

```bash
dotnet run
```

La app levanta en `http://localhost:5248`.

## Convenciones de código

### Blazor
- **Code-behind siempre**: cada `.razor` tiene su `.razor.cs` con `partial class`. Nunca `@code{}` inline.
- Variables locales antes de atributos en Razor para evitar comillas anidadas
- `@@media` (doble arroba) en bloques `<style>` dentro de Blazor

### Entity Framework
- `AsNoTracking()` en queries de lectura, especialmente en recálculos de puntos
- `UserId != null` en lugar de `IsAssigned` para filtros EF (la propiedad calculada no se traduce a SQL)
- Operaciones masivas con `ExecuteUpdateAsync` para evitar cargar entidades

### Servicios
- Todos `scoped` con interface
- Constructores primarios (`public class Foo(IBar bar) : IFoo`)
- Retornan `(bool Exito, string? Error)` para operaciones con validación

## Modelos de datos clave

| Entidad             | Descripción                                             |
|---------------------|---------------------------------------------------------|
| `AppUser`           | Usuario del sistema con rol y flag `IsBlocked`         |
| `Lote`              | Lote de planillas físicas generadas por el admin       |
| `Planilla`          | Planilla individual con código único y estado          |
| `Partido`           | Partido del Mundial con fase, slots y resultado        |
| `Equipo`            | Equipo participante con grupo y bandera                |
| `PrediccionGrupo`   | Predicción 1/X/2 de un usuario para un partido         |
| `PrediccionKnockout`| Predicción de equipos en eliminatorias                 |
| `PrediccionFinal`   | Posiciones finales + marcadores exactos                |
| `ResultadoFinal`    | Singleton con los resultados oficiales de posiciones   |
| `Configuracion`     | Pares clave/valor (cierre manual, fecha límite)        |

## Sistema de puntuación

Máximo posible: **10,190 puntos**

| Etapa                  | Pts/casilla | Casillas | Total  |
|------------------------|-------------|----------|--------|
| Fase de grupos         | 60          | 72       | 4,320  |
| Dieciseisavos (R32)    | 70          | 32       | 2,240  |
| Octavos (R16)          | 70          | 16       | 1,120  |
| Cuartos                | 80          | 8        | 640    |
| Semifinales            | 100         | 4        | 400    |
| 3°/4° Puesto           | 100         | 2        | 200    |
| Gran Final             | 100         | 2        | 200    |
| Campeón                | 300         | 1        | 300    |
| 2° Lugar               | 200         | 1        | 200    |
| 3er Lugar              | 100         | 1        | 100    |
| 4° Lugar               | 50          | 1        | 50     |
| Extras eliminatoria    | 100 c/u     | 3        | 300    |
| Marcadores exactos     | 100 c/u     | 3        | 300    |

En todas las fases el usuario predice `EquipoLocal` y `EquipoVisitante`. El servicio `PuntuacionService` compara contra los slots reales del partido y otorga puntos por cada slot correcto.

## Autenticación

Flujo cookie-based con bridge entre Blazor (Server) y HTTP:

1. `Login.razor` valida credenciales con `AuthService.LoginAsync`
2. `BuildPrincipal` crea claims (`NameIdentifier`, `Name`, `Role`, `FullName`)
3. `PendingLoginService` guarda el principal en memoria con un token GUID (TTL 30s)
4. Redirect a `/api/auth/signin?token=X&returnUrl=Y` con `forceLoad: true`
5. Endpoint minimal API ejecuta `HttpContext.SignInAsync` (cookie 7 días)
6. Redirect al `returnUrl`

Logout: form POST a `/api/auth/signout` con antiforgery token.

### Roles y políticas

- **Common** → política `Registrado` → acceso a `/mis-planillas`, `/planilla/{id}`, `/ranking`, `/perfil`
- **Admin** → política `SoloAdmin` → acceso a `/admin` con todas sus pestañas

## Despliegue

Actualmente desplegado en **Railway** (`quinielavinccler-production.up.railway.app`).

Variables de entorno requeridas en producción:
- `ConnectionStrings__DefaultConnection`
- `AdminEmail` (seed)
- `AdminPassword` (seed)

---

# Manual de Uso

## Para usuarios comunes

### Registro e inicio de sesión

1. Entra a la URL del portal
2. Si no tienes cuenta, haz clic en **Registrarse**
3. Completa: nombre completo, email, cédula, teléfono, contraseña
4. Inicia sesión con tu email y contraseña

### Vincular una planilla

1. El admin te entrega una planilla física con un código tipo `P-12345678`
2. En **Mis Planillas**, ingresa el código en el campo y presiona **Vincular**
3. La planilla queda asociada a tu cuenta

**Importante:** una vez cerrada la quiniela ya no podrás vincular nuevas planillas.

### Hacer predicciones

1. En **Mis Planillas**, haz clic en **Predecir** sobre una planilla
2. Recorre las pestañas:
   - **Fase de Grupos** — selecciona 1/X/2 para cada uno de los 72 partidos
   - **Dieciseisavos** — elige los 32 equipos que pasarán a R32 (16 primeros de grupo + 16 segundos + 8 mejores terceros)
   - **Octavos / Cuartos / Semis / 3°-4°** — selecciona los ganadores de cada partido (dropdowns en cascada)
   - **Gran Final** — define campeón, posiciones finales, equipos goleadores y marcadores exactos
3. Cada predicción se guarda automáticamente al seleccionarla
4. El contador arriba muestra cuántas casillas llevas completadas

**Recomendación para R32:** completa primero los **primeros** y **segundos** de cada grupo (slots libres), y deja para el final los **mejores terceros** que dependen de cuáles grupos ya hayan sido usados.

### Cambiar tu contraseña

1. Ve a **Mi Perfil** en el menú lateral
2. Ingresa tu contraseña actual
3. Define la nueva (mínimo 6 caracteres)
4. Confirma y guarda

Si olvidaste tu contraseña, contacta al administrador para que te genere una temporal.

### Ver el ranking

En **Ranking** verás el listado de todas las planillas ordenadas por puntaje.

- 🥇🥈🥉 Las primeras 3 posiciones se muestran con medallas
- 🙃 La posición con menor puntaje también se marca
- Tu fila aparece resaltada en azul
- Al cargar, la página salta automáticamente a donde está tu mejor planilla
- Puedes filtrar **"Ver solo mis planillas"** para ver únicamente las tuyas (mantienen sus posiciones reales)
- Selector de **10/20/50 filas por página**
- Click en el código de cualquier planilla abre el detalle completo:
  - ✓ verde donde acertaste
  - ✗ rojo donde fallaste (pasa el cursor para ver el equipo real)
  - Puntos obtenidos por partido
  - Barras de progreso de predicciones y resultados

### Premios

- 🥇 **1er Premio:** $2,000 + 50% de lo recaudado
- 🥈 **2do Premio:** $1,000 + 30% de lo recaudado
- 🥉 **3er Premio:** $500 + 20% de lo recaudado
- 🙃 **Menor puntaje:** $2,000

En caso de empate, el premio se reparte proporcionalmente.

---

## Para administradores

El panel admin está en `/admin` (solo accesible con cuenta admin).

### 1. Generar Planillas

Crea lotes de planillas físicas para imprimir y repartir.

1. Define la **cantidad** (1 a 50)
2. Click en **Generar Lote**
3. El sistema crea los códigos únicos
4. Haz clic en **Imprimir PDF** sobre el lote generado para descargar el PDF imprimible

Para **eliminar un lote**, usa el botón rojo de **Eliminar**. Solo se pueden eliminar lotes sin planillas asignadas.

**Bloqueo:** cuando la quiniela esté cerrada, no se pueden generar ni eliminar lotes.

### 2. Gestión de Planillas

Busca cualquier planilla por código o por usuario, y puedes **desvincularla** de su usuario actual (por si hubo error en la asignación).

### 3. Usuarios

Mantenimiento de cuentas registradas:

- **Buscar** por email, nombre o cédula
- **Filtro "Incluir administradores"** para ocultar/mostrar otros admins
- **Paginación** de 20 por página
- **Bloquear/Desbloquear** un usuario (no permitirá login). No se puede bloquear a otros admins.
- **Resetear contraseña** — genera una contraseña temporal aleatoria. **Cópiala y compártela con el usuario por un canal seguro** (no se guarda en ningún log). El usuario debe cambiarla desde su perfil al iniciar sesión.

### 4. Resultados

Registra los resultados oficiales del Mundial. Una pestaña por fase:

- **Fase de Grupos** — selecciona 1/X/2 para cada partido
- **Dieciseisavos** — selecciona los 32 equipos que clasificaron
- **Octavos / Cuartos / Semis / 3°4° / Gran Final** — selecciona los ganadores de cada partido. Los dropdowns muestran solo los equipos válidos según las fases anteriores.
- **Posiciones Finales** — campeón, sub-campeón, 3°, 4°, equipo más goleador, más goleado y menos goleado

Cada pestaña tiene su propio botón **Guardar** flotante. Al guardar, el sistema **recalcula automáticamente los puntos** de todas las planillas afectadas.

**Resetear todos los puntos y resultados:** botón rojo arriba a la derecha. Borra:
- Todos los puntos de todas las planillas
- Todos los resultados de todos los partidos
- Las posiciones finales y marcadores exactos
- Todos los puntajes totales

Pide confirmación antes de ejecutar. **No se puede deshacer.**

### 5. Configuración

Controla el cierre de la quiniela.

#### Cartel de estado

Arriba aparece un cartel coloreado indicando el estado actual:
- 🟢 **Abierta** — sin restricciones
- 🟡 **Abierta — cierre programado** — muestra fecha y tiempo restante
- 🔴 **Cerrada** — por fecha pasada o por cierre manual

#### Cierre manual

Toggle que fuerza el cierre **inmediatamente**, sin importar la fecha programada. Útil si necesitas adelantar el cierre por alguna razón.

#### Cierre programado

Define una fecha y hora límite. Al pasar ese momento, la quiniela se cierra automáticamente.

Ambos mecanismos son **independientes**: cualquiera de los dos basta para cerrar la quiniela.

#### Efectos del cierre

Cuando la quiniela está cerrada:
- Los usuarios no pueden vincular nuevas planillas
- No se pueden generar nuevos lotes
- No se pueden eliminar lotes existentes
- Los usuarios solo pueden **ver** sus planillas en modo lectura (no editar predicciones)

---

## Flujo típico de un torneo

1. **Antes del torneo:**
   - Admin genera lotes de planillas en `/admin → Generar Planillas`
   - Imprime los PDFs y reparte las planillas físicas a los empleados
   - Configura la fecha de cierre en `/admin → Configuración` (ej: 11 de junio antes del primer partido)
2. **Los usuarios:**
   - Se registran en el portal
   - Vinculan su planilla física con el código
   - Completan sus 149 predicciones antes de la fecha límite
3. **Durante el torneo:**
   - Cada vez que termina un partido, el admin entra a `/admin → Resultados`
   - Selecciona el resultado y guarda
   - Los puntos se recalculan automáticamente
   - Los usuarios ven sus puntos actualizados en el ranking
4. **Al final del torneo:**
   - Admin registra los marcadores exactos de semifinales y final
   - Admin registra las posiciones finales y los equipos extras (goleador, etc.)
   - El ranking final determina los ganadores de los premios

---

## Soporte y reportes

Para reportar bugs o sugerir mejoras, contacta al equipo de TI de Vinccler.