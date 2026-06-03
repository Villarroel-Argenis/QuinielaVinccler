// Components/Admin/PartidosGrupoComponent.razor.cs
namespace QuinielaVinccler.UI.Web.Components.Admin;

public partial class PartidosGrupoComponent : ComponentBase
{
    [Inject] private IPartidoAdminService Svc { get; set; } = null!;

    private bool _cargandoGrupo = true;
    private bool _cargandoKo = true;
    private bool _procesando;

    // Grupos
    private Dictionary<string, List<Partido>> _grupos = [];

    // Swap equipos
    private bool _showConfirmSwap;
    private Partido? _partidoSwap;

    // Swap números
    private int? _swapPartidoAId;
    private int? _swapPartidoBId;
    private bool _showConfirmSwapNumero;
    private string _swapDescripcion = "";

    // Knockout slots
    private Dictionary<Fase, List<Partido>> _koPartidos = [];
    private int? _editandoSlot;
    private string _slotLocalEdit = "";
    private string _slotVisitanteEdit = "";

    // Feedback
    private string? _mensaje;
    private Severity _severidad;

    private static readonly Dictionary<Fase, List<string>> _slotsPorFase = new()
    {
        [Fase.RoundOf16] = Enumerable.Range(73, 16).Select(n => $"G{n}").ToList(),
        [Fase.Cuartos] = Enumerable.Range(89, 8).Select(n => $"G{n}").ToList(),
        [Fase.Semis] = Enumerable.Range(97, 4).Select(n => $"G{n}").ToList(),
        [Fase.TercerPuesto] = ["P101", "P102"],
        [Fase.Final] = ["G101", "G102"],
    };

    protected override async Task OnInitializedAsync()
    {
        await CargarGruposAsync();
        await CargarKoAsync();
    }

    private async Task CargarGruposAsync()
    {
        _cargandoGrupo = true;
        var partidos = await Svc.GetPartidosGrupoAsync();
        _grupos = partidos
            .GroupBy(p => p.EquipoLocal!.Grupo)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.NumeroPartido).ToList());
        _cargandoGrupo = false;
    }

    private async Task CargarKoAsync()
    {
        _cargandoKo = true;
        var partidos = await Svc.GetPartidosKnockoutAsync();
        _koPartidos = partidos
            .GroupBy(p => p.Fase)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.NumeroPartido).ToList());
        _cargandoKo = false;
    }

    private void IniciarSwapNumero(Partido p)
    {
        _swapPartidoAId = p.Id;
        _swapPartidoBId = null;
    }

    private void CancelarSwapNumero()
    {
        _swapPartidoAId = null;
        _swapPartidoBId = null;
    }

    private void SeleccionarSegundoPartido(Partido p)
    {
        if (_swapPartidoAId == p.Id) return;
        _swapPartidoBId = p.Id;

        var todos = _grupos.Values.SelectMany(x => x).ToList();
        var a = todos.First(x => x.Id == _swapPartidoAId);
        var b = p;
        _swapDescripcion = $"#{a.NumeroPartido} ({a.EquipoLocal?.Nombre} vs {a.EquipoVisitante?.Nombre})" +
                           $" ↔ #{b.NumeroPartido} ({b.EquipoLocal?.Nombre} vs {b.EquipoVisitante?.Nombre})";
        _showConfirmSwapNumero = true;
    }

    private async Task ConfirmarSwapNumero()
    {
        if (_swapPartidoAId is null || _swapPartidoBId is null) return;
        _procesando = true;
        var (ok, msg) = await Svc.SwapNumerosAsync(_swapPartidoAId.Value, _swapPartidoBId.Value);
        _mensaje = msg;
        _severidad = ok ? Severity.Success : Severity.Error;
        _showConfirmSwapNumero = false;
        _swapPartidoAId = null;
        _swapPartidoBId = null;
        _procesando = false;
        if (ok) await CargarGruposAsync();
    }

    private void AbrirConfirmSwap(Partido p)
    {
        _partidoSwap = p;
        _showConfirmSwap = true;
    }

    private async Task ConfirmarSwap()
    {
        if (_partidoSwap is null) return;
        _procesando = true;
        var (ok, msg) = await Svc.SwapLocalVisitanteAsync(_partidoSwap.Id);
        _mensaje = msg;
        _severidad = ok ? Severity.Success : Severity.Error;
        _showConfirmSwap = false;
        _partidoSwap = null;
        _procesando = false;
        if (ok) await CargarGruposAsync();
    }

    private void IniciarEditarSlots(Partido p)
    {
        _editandoSlot = p.Id;
        _slotLocalEdit = p.SlotLocal ?? "";
        _slotVisitanteEdit = p.SlotVisitante ?? "";
    }

    private void CancelarSlots() => _editandoSlot = null;

    private async Task ConfirmarSlots(int partidoId)
    {
        _procesando = true;
        var (ok, msg) = await Svc.CambiarSlotsAsync(partidoId, _slotLocalEdit, _slotVisitanteEdit);
        _mensaje = msg;
        _severidad = ok ? Severity.Success : Severity.Error;
        _editandoSlot = null;
        _procesando = false;
        if (ok) await CargarKoAsync();
    }

    internal List<string> GetSlotsParaFase(Fase fase) =>
        _slotsPorFase.TryGetValue(fase, out var slots) ? slots : [];

    internal static string FaseLabel(Fase fase) => fase switch
    {
        Fase.RoundOf32 => "Dieciseisavos",
        Fase.RoundOf16 => "Octavos de Final",
        Fase.Cuartos => "Cuartos de Final",
        Fase.Semis => "Semifinales",
        Fase.TercerPuesto => "3° y 4° Puesto",
        Fase.Final => "Gran Final",
        _ => fase.ToString()
    };
}