namespace QuinielaVinccler.UI.Web.Components.Layout;

public partial class NavMenu
{
    private static string GetInitials(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "?";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
            : $"{parts[0][0]}".ToUpper();
    }
}