namespace QuinielaVinccler.UI.Web.Services;

/// <summary>
/// Proyección liviana de una planilla para el panel de administrador.
/// Incluye datos del usuario vinculado para facilitar la búsqueda.
/// </summary>
public sealed record PlanillaAdminDto(
    int Id,
    string Codigo,
    EstadoPlanilla Estado,
    int PuntajeTotal,
    DateTime? AssignedAt,
    string? UsuarioNombre,
    string? UsuarioEmail,
    string? UsuarioCI
);