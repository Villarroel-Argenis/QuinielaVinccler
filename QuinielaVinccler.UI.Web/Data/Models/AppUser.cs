namespace QuinielaVinccler.UI.Web.Data.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CI { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Role { get; set; } = "Common";
    public DateTime CreatedAt { get; set; }
}
