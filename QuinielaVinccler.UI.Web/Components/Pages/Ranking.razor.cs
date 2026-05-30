// Components/Pages/Ranking.razor.cs
namespace QuinielaVinccler.UI.Web.Components.Pages;

public partial class Ranking : ComponentBase
{
    [Inject] private IPlanillaService PlanillaSvc { get; set; } = null!;
    [Inject] private IDialogService DialogSvc { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthSp { get; set; } = null!;

    private bool _cargando = true;
    private List<RankingItemDto> _ranking = [];
    private int _total;
    private int _ultimaPosicion;
    private int _userId;

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthSp.GetAuthenticationStateAsync();
        var claim = auth.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _userId = claim is not null ? int.Parse(claim) : 0;

        _ranking = await PlanillaSvc.GetRankingAsync(_userId);
        _total = _ranking.Count;
        _ultimaPosicion = _ranking.Count > 0 ? _ranking[^1].Posicion : 0;
        _cargando = false;
    }

    private string FilaClass(RankingItemDto item, int _)
        => item.EsUsuarioActual ? "ranking-fila-actual" : "";

    private async Task AbrirModal(RankingItemDto item)
    {
        var parametros = new DialogParameters<PlanillaModal>
        {
            { x => x.PlanillaId, item.PlanillaId },
            { x => x.UserId,  item.UserId },
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
