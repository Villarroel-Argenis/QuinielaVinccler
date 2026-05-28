# QuinielaVinccler — Copa Mundial FIFA 2026

Portal interno de quiniela para **Vinccler C.A.** — Copa Mundial FIFA 2026 (USA · México · Canadá).

---

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Frontend | Blazor Server Interactive (.NET 10) + MudBlazor |
| Backend | ASP.NET Core Minimal APIs |
| Base de datos | PostgreSQL + EF Core (code-first) |
| Autenticación | Cookie authentication + BCrypt |
| PDF | QuestPDF (Community License) |

---

## Arquitectura

```
QuinielaVinccler.sln
└── QuinielaVinccler.UI.Web/
    ├── Components/
    │   ├── Layout/          # MainLayout, NavMenu, AuthLayout
    │   ├── Pages/
    │   │   ├── Auth/        # Login.razor + Login.razor.cs
    │   │   │                # Register.razor + Register.razor.cs
    │   │   ├── Admin/       # Admin.razor (SoloAdmin policy)
    │   │   └── MisPlanillas/# MisPlanillas.razor (Registrado policy)
    │   └── Shared/          # Componentes reutilizables
    ├── Data/
    │   ├── Models/          # Entidades EF Core
    │   ├── Migrations/      # EF Core migrations
    │   └── AppDbContext.cs
    ├── Endpoints/           # Minimal API endpoints
    ├── Services/            # Lógica de negocio
    └── wwwroot/
        └── images/          # ANVERSO.jpg, REVERSO.jpg, logo
```

### Convención de componentes Blazor

Todo componente usa **code-behind obligatorio**:
- `Login.razor` → solo markup
- `Login.razor.cs` → `partial class Login : ComponentBase` con toda la lógica

Nunca usar bloques `@code { }` inline.

---

## Flujo de autenticación

```
Login.razor
  → AuthService.LoginAsync (BCrypt verify)
  → BuildPrincipal (claims: userId, email, role)
  → PendingLoginService.Store (token GUID, TTL 30s)
  → Form POST oculto → /api/auth/signin
  → HttpContext.SignInAsync (cookie 7 días)
  → Redirect a /mis-planillas o /admin
```

**Logout:** Form POST → `/api/auth/signout` → `SignOutAsync` → `/login`

---

## Modelo de dominio

### Entidades principales

| Entidad | Descripción |
|---------|-------------|
| `AppUser` | Usuario registrado (Admin / Common) |
| `Lote` | Lote de planillas generado por el admin |
| `Planilla` | Planilla física vinculada a un usuario |
| `Equipo` | 48 equipos del mundial con código ISO de bandera |
| `Partido` | 104 partidos (72 grupos + 32 eliminatoria) |
| `PrediccionGrupo` | 72 predicciones 1/X/2 por planilla |
| `PrediccionKnockout` | 32 predicciones de equipo ganador por planilla |
| `PrediccionFinal` | Campeón, posiciones finales y extras (1 por planilla) |

### Estados de planilla

```
SinAsignar → Asignada → EnProgreso → Completa → Cerrada
```

### Estructura de partidos

- **Grupos (1-72):** 12 grupos × 6 partidos = 72 partidos
- **R32 (73-88):** 16 partidos de dieciseisavos
- **R16 (89-96):** 8 partidos de octavos
- **Cuartos (97-100):** 4 partidos
- **Semis (101-102):** 2 partidos
- **Tercer puesto (103):** 1 partido
- **Final (104):** 1 partido

---

## Sistema de puntuación

| Fase | Puntos por acierto |
|------|--------------------|
| Fase de grupos (1/X/2) | 60 pts |
| Dieciseisavos + Octavos | 70 pts |
| Cuartos de final | 80 pts |
| Semifinales + 3er puesto | 100 pts |
| Campeón | 300 pts |
| 2do lugar | 200 pts |
| 3er lugar | 100 pts |
| 4to lugar | 50 pts |
| Equipo más goleador (grupos) | 100 pts |
| Equipo más goleado (grupos) | 100 pts |
| Equipo menos goleado (grupos) | 100 pts |
| Resultado exacto Gran Final (90') | 100 pts |
| Resultado exacto Semifinal 1 (90') | 100 pts |
| Resultado exacto Semifinal 2 (90') | 100 pts |

### Premios

| Premio | Monto |
|--------|-------|
| 1er lugar | $2.000 + 50% de lo recaudado |
| 2do lugar | $1.000 + 30% de lo recaudado |
| 3er lugar | $500 + 20% de lo recaudado |
| Menor puntaje | $2.000 |

**Costo de participación:** $5 USD por planilla

---

## Flujo de la quiniela

```
1. Admin genera lote de planillas (códigos P-XXXXXXXX)
2. Admin imprime el PDF (ANVERSO + REVERSO por planilla)
3. Participante paga $5 USD y recibe su planilla física
4. Participante se registra en el portal
5. Participante vincula su planilla ingresando el código
6. Participante llena predicciones antes del 11 de junio
7. Admin registra resultados partido a partido
8. Sistema calcula puntajes automáticamente
9. Ranking visible para todos los participantes
```

---

## Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=quiniela_vinccler;Username=admin;Password=admin123"
  },
  "Seed": {
    "AdminEmail": "admin@quinielavinccler.com",
    "AdminPassword": "admin2026"
  },
  "Quiniela": {
    "FechaCierreUtc": "2026-06-11T23:59:59Z"
  }
}
```

---

## Base de datos

### Comandos EF Core

```bash
# Crear migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Resetear base de datos completa
dotnet ef database drop --force
dotnet ef database update
```

### Seed automático

Al iniciar la aplicación se ejecuta automáticamente:
- Usuario administrador
- 48 equipos con códigos ISO de banderas
- 104 partidos (grupos + eliminatoria)

---

## Seguridad implementada

- Autenticación por cookie HttpOnly + SameSite Strict
- BCrypt para hash de contraseñas (work factor 11)
- Protección contra timing attacks en login
- Token de sesión en body (POST) — nunca en URL
- Antiforgery en logout
- Autorización por políticas nombradas (`SoloAdmin`, `Registrado`)
- Validación de `returnUrl` contra open redirect

---

## Servicios

| Servicio | Responsabilidad |
|----------|----------------|
| `IAuthService` | Login, registro, construcción de claims |
| `ILoteService` | Creación y gestión de lotes de planillas |
| `IPdfService` | Generación de PDF con ANVERSO/REVERSO |
| `PendingLoginService` | Puente Blazor↔HTTP para autenticación (Singleton) |
| `CustomAuthStateProvider` | Estado de auth por circuito Blazor Server |
| `PendingLoginCleanupService` | Limpieza de tokens expirados (BackgroundService) |

---

## Banderas

Las banderas de equipos se sirven desde [flagcdn.com](https://flagcdn.com):

```
https://flagcdn.com/24x18/{codigoIso}.png
```

Ejemplos: `mx` → México, `br` → Brasil, `gb-sct` → Escocia, `gb-eng` → Inglaterra

---

## Estado del proyecto

- [x] Autenticación y autorización
- [x] Gestión de lotes y planillas (admin)
- [x] Generación de PDF
- [x] Modelo de dominio completo
- [x] Seed de equipos y partidos
- [ ] Flujo de vinculación de planilla (usuario)
- [ ] Formulario de predicciones
- [ ] Servicio de puntuación
- [ ] Panel admin para registrar resultados
- [ ] Ranking de participantes