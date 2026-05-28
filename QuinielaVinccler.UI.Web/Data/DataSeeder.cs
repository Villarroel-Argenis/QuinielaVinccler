namespace QuinielaVinccler.UI.Web.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        await db.Database.MigrateAsync();

        await SeedAdminAsync(db, config);
    }

    private static async Task SeedAdminAsync(AppDbContext db, IConfiguration config)
    {
        var email = config["Seed:AdminEmail"] ?? "admin@quinielavinccler.com";
        var password = config["Seed:AdminPassword"] ?? "admin2026";

        if (await db.Users.AnyAsync(u => u.Email == email))
            return;

        db.Users.Add(new AppUser
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = "Administrador",
            CI = "00000000",
            Telefono = "00000000000",
            Role = AppRoles.Admin
        });

        await db.SaveChangesAsync();
    }
}