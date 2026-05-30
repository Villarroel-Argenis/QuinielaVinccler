namespace QuinielaVinccler.UI.Web.Services;

public class UserService(AppDbContext db) : IUserService
{
    public async Task<List<UserAdminDto>> BuscarAsync(string? termino, bool incluirAdmins)
    {
        var query = db.Users.AsNoTracking().AsQueryable();

        if (!incluirAdmins)
            query = query.Where(u => u.Role != AppRoles.Admin);

        if (!string.IsNullOrWhiteSpace(termino))
        {
            var t = termino.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(t) ||
                u.FullName.ToLower().Contains(t) ||
                u.CI.Contains(t));
        }

        // Count de planillas vinculadas por usuario en una sola query
        var resultado = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserAdminDto(
                u.Id,
                u.Email,
                u.FullName,
                u.CI,
                u.Telefono,
                u.Role,
                u.IsBlocked,
                u.CreatedAt,
                db.Planillas.Count(p => p.UserId == u.Id)
            ))
            .ToListAsync();

        return resultado;
    }

    public async Task<(bool Exito, string? Error)> ToggleBloqueoAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null)
            return (false, "Usuario no encontrado.");

        if (user.Role == AppRoles.Admin)
            return (false, "No se puede bloquear a un administrador.");

        user.IsBlocked = !user.IsBlocked;
        await db.SaveChangesAsync();

        return (true, null);
    }
}
