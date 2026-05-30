var builder = WebApplication.CreateBuilder(args);

// ── Razor + Blazor ───────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// ── Base de datos ────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Autenticación por cookie ─────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/api/auth/signout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SoloAdmin", p => p.RequireRole(AppRoles.Admin))
    .AddPolicy("Registrado", p => p.RequireAuthenticatedUser());

// ── Registrar servicios ────


// ── HttpContext (necesario para CustomAuthStateProvider en Blazor Server) ────
builder.Services.AddHttpContextAccessor();

// ── Auth state provider personalizado ───────────────────────────────────────
// Scoped: una instancia por circuito Blazor Server.
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();

// ── Servicios de la app ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILoteService,LoteService > ();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();
builder.Services.AddScoped<IPlanillaService, PlanillaService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();
builder.Services.AddScoped<IPuntuacionService, PuntuacionService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();

builder.Services.AddHostedService<PendingLoginCleanupService>(); // BackgroundService: tarea en segundo plano
builder.Services.AddSingleton<PendingLoginService>();   // Singleton: puente Blazor ↔ HTTP

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ────────────────────────────────────────────────────────────────────────────
var app = builder.Build();
// ────────────────────────────────────────────────────────────────────────────

// ── Seed inicial ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        await DataSeeder.SeedAsync(db, config);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SEED ERROR] {ex.Message}");
    }
}

// ── Middleware pipeline ──────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();   // ← antes de UseAuthorization
app.UseAuthorization();    // ← antes de UseAntiforgery
app.UseAntiforgery();

// ── Endpoints ────────────────────────────────────────────────────────────────
app.MapAuthEndpoints();
app.MapLoteEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();