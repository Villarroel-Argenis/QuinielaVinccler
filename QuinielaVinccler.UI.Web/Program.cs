var builder = WebApplication.CreateBuilder(args);

// ── Razor + Blazor ───────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// ── Base de datos ────────────────────────────────────────────────────────────
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    poolSize: 128);

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

// ── Registrar servicios ────
builder.Services.AddScoped<LoteService>();
builder.Services.AddScoped<PdfService>();

// ── HttpContext (necesario para CustomAuthStateProvider en Blazor Server) ────
builder.Services.AddHttpContextAccessor();

// ── Auth state provider personalizado ───────────────────────────────────────
// Scoped: una instancia por circuito Blazor Server.
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();

// ── Servicios de la app ──────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<PendingLoginService>();   // Singleton: puente Blazor ↔ HTTP

// ────────────────────────────────────────────────────────────────────────────
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();
// ────────────────────────────────────────────────────────────────────────────

// ── Seed inicial ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DataSeeder.SeedAsync(db, config);
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