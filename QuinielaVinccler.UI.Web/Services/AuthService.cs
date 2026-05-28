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

    public async Task<(bool Success, string? Error)> RegisterAsync(
        string email, string password, string fullName, string ci, string telefono)
    {
        if (!IsValidEmail(email))
            return (false, "Correo electrónico no válido.");

        if (password.Length < 6)
            return (false, "La contraseña debe tener al menos 6 caracteres.");

        if (string.IsNullOrWhiteSpace(fullName))
            return (false, "El nombre completo es requerido.");

        email = email.ToLower().Trim();

        if (await db.Users.AnyAsync(u => u.Email == email))
            return (false, "Ya existe una cuenta registrada con este correo.");

        db.Users.Add(new AppUser
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName.Trim(),
            CI = ci.Trim(),
            Telefono = telefono.Trim(),
            Role = AppRoles.Common,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return (true, null);
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