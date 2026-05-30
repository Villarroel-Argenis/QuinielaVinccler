// Components/Shared/RankingWidget.razor.cs
namespace QuinielaVinccler.UI.Web.Components.Shared;

public partial class RankingWidget : ComponentBase
{
    [Parameter] public int UserId { get; set; }

    [Inject] private IPlanillaService PlanillaSvc { get; set; } = null!;
    [Inject] private IDialogService DialogSvc { get; set; } = null!;

    private bool _cargando = true;
    private List<RankingItemDto> _ranking = [];
    private List<RankingItemDto> _visible = [];
    private RankingItemDto? _usuarioFueraDelTop;
    private int _limite = 10;

    protected override async Task OnParametersSetAsync()
    {
        if (_ranking.Count == 0)
        {
            _cargando = true;
            _ranking = await PlanillaSvc.GetRankingAsync(UserId);
            _cargando = false;
        }

        ActualizarVista();
    }

    private void CambiarLimite(int nuevo)
    {
        _limite = nuevo;
        ActualizarVista();
    }

    private void ActualizarVista()
    {
        _visible = _ranking.Take(_limite).ToList();

        var usuarioItem = _ranking.FirstOrDefault(r => r.EsUsuarioActual);
        _usuarioFueraDelTop = (usuarioItem is not null && !_visible.Contains(usuarioItem))
            ? usuarioItem
            : null;

        StateHasChanged();
    }

    private async Task AbrirModal(RankingItemDto item)
    {
        var parametros = new DialogParameters<PlanillaModal>
        {
            { x => x.PlanillaId, item.PlanillaId },
            { x => x.CodigoPlanilla, item.CodigoPlanilla }
        };

        var opciones = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true,
            CloseOnEscapeKey = true
        };

        await DialogSvc.ShowAsync<PlanillaModal>($"Planilla {item.CodigoPlanilla}", parametros, opciones);
    }
}
