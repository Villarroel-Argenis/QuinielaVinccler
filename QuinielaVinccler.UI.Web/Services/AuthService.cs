namespace QuinielaVinccler.UI.Web.Services;

public class AuthService(AppDbContext db)
{
    private const string DummyHash = "$2a$11$jEZEOC5QdetIWnW7WW1fk.SKhMoJdLI9kFXSnWqj25zKO.BzR3sJe";

    public async Task<AppUser?> LoginAsync(string email, string password)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

        // Hash dummy para normalizar el tiempo de respuesta cuando el usuario no existe.
        // Sin esto, la diferencia de tiempo entre "no existe" (~0ms) y
        // "contraseña incorrecta" (~200ms de BCrypt) permite enumerar emails registrados.
        var hash = user?.PasswordHash ?? DummyHash;

        var valid = BCrypt.Net.BCrypt.Verify(password, hash);

        return (user is not null && valid) ? user : null;
    }

    public async Task<(AppUser? User, string? Error)> RegisterAsync(
    string email, string password, string fullName, string ci, string telefono)
    {
        if (!IsValidEmail(email))
            return (null, "Correo electrónico no válido.");

        if (password.Length < 6)
            return (null, "La contraseña debe tener al menos 6 caracteres.");

        if (string.IsNullOrWhiteSpace(fullName))
            return (null, "El nombre completo es requerido.");

        email = email.ToLower().Trim();

      

        var user = new AppUser
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName.Trim(),
            CI = ci.Trim(),
            Telefono = telefono.Trim(),
            Role = AppRoles.Common,
        };

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
            return (user, null);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("duplicate key") == true
               || ex.InnerException?.Message.Contains("unique constraint") == true)
        {
            return (null, "Ya existe una cuenta registrada con este correo.");
        }
    }

    public ClaimsPrincipal BuildPrincipal(AppUser user)
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("FullName", user.FullName)
        ];

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "cookie"));
    }

    private static bool IsValidEmail(string email)
    {
        try { _ = new MailAddress(email); return true; }
        catch { return false; }
    }
}