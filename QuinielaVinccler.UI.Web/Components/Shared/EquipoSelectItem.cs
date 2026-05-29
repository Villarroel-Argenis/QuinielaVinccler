namespace QuinielaVinccler.UI.Web.Components.Shared;

/// <summary>
/// Representa un item en el dropdown de equipo.
/// Puede ser un header de grupo (Disabled, sin equipo) o un equipo seleccionable.
/// </summary>
public sealed record EquipoSelectItem(
    bool EsHeader,
    Equipo? Equipo = null,
    string Grupo = "",
    int? HeaderValue = null);
