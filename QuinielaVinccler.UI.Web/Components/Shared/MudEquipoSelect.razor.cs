namespace QuinielaVinccler.UI.Web.Components.Shared;

public partial class MudEquipoSelect : ComponentBase
{
    [Parameter, EditorRequired] public int? Value { get; set; }
    [Parameter, EditorRequired] public EventCallback<int?> ValueChanged { get; set; }
    [Parameter, EditorRequired] public IReadOnlyList<EquipoSelectItem> Items { get; set; } = [];
    [Parameter, EditorRequired] public Dictionary<int, Equipo> EquiposById { get; set; } = [];

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Clearable { get; set; }
    [Parameter] public string Placeholder { get; set; } = "-- Seleccionar --";
    [Parameter] public string MinWidth { get; set; } = "200px";

    // ── Estado interno ────────────────────────────────────────────────────────
    private bool _open;

    // Valor interno que siempre sigue al parámetro Value
    private int? _valorInterno;

    private Equipo? _equipoSeleccionado =>
        _valorInterno.HasValue && EquiposById.TryGetValue(_valorInterno.Value, out var e) ? e : null;

    // ── Sincronización con parámetro ──────────────────────────────────────────
    protected override void OnParametersSet()
    {
        // Sincroniza el valor interno con el parámetro cada vez que el padre actualiza
        _valorInterno = Value;
    }

    // ── CSS ───────────────────────────────────────────────────────────────────
    private string TriggerClass =>
        $"equipo-select-trigger {(Disabled ? "equipo-select-trigger--disabled" : "")}";

    private string TriggerStyle => "width:100%;";

    // ── Interacción ───────────────────────────────────────────────────────────
    private void ToggleOpen()
    {
        if (!Disabled) _open = !_open;
    }

    private void Cerrar() => _open = false;

    private async Task SeleccionarAsync(int id)
    {
        _open = false;
        _valorInterno = id;
        await ValueChanged.InvokeAsync(id);
    }

    private async Task LimpiarAsync()
    {
        _open = false;
        _valorInterno = null;
        await ValueChanged.InvokeAsync(null);
    }
}