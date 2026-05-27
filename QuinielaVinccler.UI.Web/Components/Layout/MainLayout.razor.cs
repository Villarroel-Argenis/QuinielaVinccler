namespace QuinielaVinccler.UI.Web.Components.Layout;

public partial class MainLayout
{
    private bool _drawerOpen = true;
    private bool _isDarkMode;

    private void DrawerToggle() => _drawerOpen = !_drawerOpen;

    private void DarkModeToggle() => _isDarkMode = !_isDarkMode;

    private string DarkLightModeButtonIcon => _isDarkMode
        ? Icons.Material.Rounded.LightMode
        : Icons.Material.Rounded.DarkMode;
}