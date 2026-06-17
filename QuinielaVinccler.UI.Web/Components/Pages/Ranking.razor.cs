// Components/Pages/Ranking.razor.cs
namespace QuinielaVinccler.UI.Web.Components.Pages;

public partial class Ranking : ComponentBase
{
    [Inject] private IPlanillaService PlanillaSvc { get; set; } = null!;
    [Inject] private IDialogService DialogSvc { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthSp { get; set; } = null!;
    [Inject] private IConfiguracionService ConfigSvc { get; set; } = null!;

    private bool _quinielaCerrada;

    private bool _cargando = true;
    private List<RankingItemDto> _ranking = [];
    private List<RankingItemDto> _filtradas = [];
    private List<RankingItemDto> _paginadas = [];

    private int _total;
    private int _ultimaPosicion;
    private int _userId;

    private bool _soloMias;
    private int _pageSize = 20;
    private int _paginaActual = 1;

    private const int UmbralOcultarPaginador = 19;

    private int TotalPaginas => _filtradas.Count == 0 ? 1 :
        (int)Math.Ceiling(_filtradas.Count / (double)_pageSize);

    private bool MostrarPaginador => !_soloMias && _filtradas.Count > UmbralOcultarPaginador;

    protected override async Task OnInitializedAsync()
    {

        var auth = await AuthSp.GetAuthenticationStateAsync();
        var claim = auth.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _userId = claim is not null ? int.Parse(claim) : 0;

        _quinielaCerrada = await ConfigSvc.QuinielaCerradaAsync();

        if (!_quinielaCerrada)
        {
            _cargando = false;
            return;
        }
        _ranking = await PlanillaSvc.GetRankingAsync(_userId);
        _total = _ranking.Count;
        _ultimaPosicion = _ranking.Count > 0 ? _ranking[^1].Posicion : 0;

        // Saltar a la página donde está la mejor planilla del usuario
        AplicarFiltroYPagina(saltarAUsuario: true);

        _cargando = false;
    }

    private void ToggleSoloMias(bool valor)
    {
        _soloMias = valor;
        _paginaActual = 1;
        AplicarFiltroYPagina(saltarAUsuario: false);
    }

    private void CambiarPageSize(int nuevo)
    {
        _pageSize = nuevo;
        AplicarFiltroYPagina(saltarAUsuario: true);
    }

    private void CambiarPagina(int nueva)
    {
        _paginaActual = nueva;
        AplicarPaginacion();
        StateHasChanged();
    }

    private void AplicarFiltroYPagina(bool saltarAUsuario)
    {
        _filtradas = _soloMias
            ? _ranking.Where(r => r.EsUsuarioActual).ToList()
            : _ranking;

        if (saltarAUsuario && !_soloMias)
        {
            // Buscar la mejor posición del usuario (menor número = mejor)
            var mejor = _ranking.Where(r => r.EsUsuarioActual)
                                .OrderBy(r => r.Posicion)
                                .FirstOrDefault();
            if (mejor is not null)
            {
                int indice = _ranking.IndexOf(mejor);
                _paginaActual = (indice / _pageSize) + 1;
            }
            else
            {
                _paginaActual = 1;
            }
        }

        AplicarPaginacion();
        StateHasChanged();
    }

    private void AplicarPaginacion()
    {
        if (_soloMias)
        {
            _paginadas = _filtradas;
            return;
        }

        _paginadas = _filtradas
            .Skip((_paginaActual - 1) * _pageSize)
            .Take(_pageSize)
            .ToList();
    }

    private string FilaClass(RankingItemDto item, int _)
        => item.EsUsuarioActual ? "ranking-fila-actual" : "";

    private async Task AbrirModal(RankingItemDto item)
    {
        var parametros = new DialogParameters<PlanillaModal>
        {
            { x => x.PlanillaId, item.PlanillaId },
            { x => x.UserId, item.UserId  },
            { x => x.CodigoPlanilla, item.CodigoPlanilla },
            { x => x.NombreUsuario, item.Nombre  }
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

    // En Ranking.razor.cs
    private int GetCorrelativo(RankingItemDto item)
    {
        return _paginadas.IndexOf(item) + 1 + (_paginaActual - 1) * _pageSize;
    }
}
