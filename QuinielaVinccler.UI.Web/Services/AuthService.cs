namespace QuinielaVinccler.UI.Web.Services;

public class AuthService(AppDbContext db) : IAuthService
{
    private const string DummyHash = "$2a$11$jEZEOC5QdetIWnW7WW1fk.SKhMoJdLI9kFXSnWqj25zKO.BzR3sJe";

    public async Task<(AppUser? User, string? Error)> LoginAsync(string email, string password)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

        // Hash dummy para normalizar tiempo de respuesta y prevenir enumeración de emails
        var hash = user?.PasswordHash ?? DummyHash;
        var valid = BCrypt.Net.BCrypt.Verify(password, hash);

        if (user is null || !valid)
            return (null, "Correo o contraseña incorrectos.");

        if (user.IsBlocked)
            return (null, "Tu cuenta está bloqueada. Contacta al administrador.");

        return (user, null);
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

    public async Task<(bool Exito, string? Error)> CambiarPasswordAsync(
        int userId, string passwordActual, string passwordNueva)
    {
        if (passwordNueva.Length < 6)
            return (false, "La nueva contraseña debe tener al menos 6 caracteres.");

        if (passwordActual == passwordNueva)
            return (false, "La nueva contraseña no puede ser igual a la actual.");

        var user = await db.Users.FindAsync(userId);
        if (user is null)
            return (false, "Usuario no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(passwordActual, user.PasswordHash))
            return (false, "La contraseña actual es incorrecta.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva);
        await db.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(string? PasswordTemporal, string? Error)> ResetearPasswordAdminAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null)
            return (null, "Usuario no encontrado.");

        var temp = GenerarPasswordTemporal();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temp);
        await db.SaveChangesAsync();

        return (temp, null);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static bool IsValidEmail(string email)
    {
        try { _ = new MailAddress(email); return true; }
        catch { return false; }
    }

    /// <summary>
    /// Genera una contraseña temporal de 10 caracteres con mayúsculas, minúsculas y números.
    /// Excluye caracteres ambiguos (0/O, 1/l/I).
    /// </summary>
    private static string GenerarPasswordTemporal()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var bytes = RandomNumberGenerator.GetBytes(10);
        var sb = new StringBuilder(10);
        foreach (var b in bytes)
            sb.Append(chars[b % chars.Length]);
        return sb.ToString();
    }
}
