namespace QuinielaVinccler.UI.Web.Components.Shared;

public partial class MudEquipoSelect : ComponentBase
{
    // ── Parámetros ────────────────────────────────────────────────────────────

    /// <summary>Id del equipo seleccionado actualmente.</summary>
    [Parameter, EditorRequired]
    public int? Value { get; set; }

    /// <summary>Callback al cambiar la selección. Recibe null al limpiar.</summary>
    [Parameter, EditorRequired]
    public EventCallback<int?> ValueChanged { get; set; }

    /// <summary>
    /// Lista de items a mostrar en el dropdown.
    /// Puede incluir headers de grupo (EsHeader=true) intercalados con equipos.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<EquipoSelectItem> Items { get; set; } = [];

    /// <summary>Diccionario Id→Equipo para resolver el valor seleccionado.</summary>
    [Parameter, EditorRequired]
    public Dictionary<int, Equipo> EquiposById { get; set; } = [];

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Clearable { get; set; }
    [Parameter] public string Placeholder { get; set; } = "-- Seleccionar --";
    [Parameter] public string MinWidth { get; set; } = "200px";

    // ── Estado interno ────────────────────────────────────────────────────────

    private bool _open;

    private Equipo? _equipoSeleccionado =>
        Value.HasValue && EquiposById.TryGetValue(Value.Value, out var e) ? e : null;

    // ── CSS helper ────────────────────────────────────────────────────────────

    private string TriggerClass =>
        $"equipo-select-trigger {(Disabled ? "equipo-select-trigger--disabled" : "")}";

    private string TriggerStyle => "width:100%;";

    // ── Interacción ───────────────────────────────────────────────────────────

    private void ToggleOpen()
    {
        if (!Disabled)
            _open = !_open;
    }

    private void Cerrar() => _open = false;

    private async Task SeleccionarAsync(int id)
    {
        _open = false;
        if (Value == id) return; // misma selección, no disparar callback
        await ValueChanged.InvokeAsync(id);
    }

    private async Task LimpiarAsync()
    {
        _open = false;
        await ValueChanged.InvokeAsync(null);
    }
}
