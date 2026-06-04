namespace QuinielaVinccler.UI.Web.Components.Admin;

public partial class LoteComponent
{
    private int _cantidad = 1;
    private bool _generando = false;
    private Lote? _ultimoLote;
    private List<Lote> _lotes = [];
    private Lote? _seleccionado;
    private bool _showConfirmDelete = false;
    private string _error = "";
    private bool _quinielaCerrada = false;
    private FiltroPlanilla _filtro = FiltroPlanilla.Total;

    [Inject] private ILoteService LoteService { get; set; } = default!;
    [Inject] private IConfiguracionService ConfigSvc { get; set; } = default!;

    [Inject] private IDialogService DialogSvc { get; set; } = default!;

    private async Task AbrirModal(Planilla planilla)
    {
        var nombre = planilla.User?.FullName ?? "";
        
        var parametros = new DialogParameters<PlanillaModal>
    {
        { x => x.PlanillaId, planilla.Id },
        { x => x.UserId, planilla.UserId ?? 0 },
        { x => x.CodigoPlanilla, planilla.Codigo },
        { x => x.NombreUsuario, nombre }
    };

        var opciones = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true,
            CloseOnEscapeKey = true
        };

        await DialogSvc.ShowAsync<PlanillaModal>($"Planilla {planilla.Codigo}", parametros, opciones);
    }

    public enum FiltroPlanilla
    {
        Total,
        Asignadas,
        SinAsignar
    }

    protected override async Task OnInitializedAsync()
    {
        _lotes = await LoteService.GetAsync();
        _quinielaCerrada = await ConfigSvc.QuinielaCerradaAsync();
    }

    private void AplicarFiltro(FiltroPlanilla filtro)
    {
        // Click en el filtro activo lo desactiva (vuelve a Total)
        if (_filtro == filtro && filtro != FiltroPlanilla.Total)
        {
            _filtro = FiltroPlanilla.Total;
        }
        else
        {
            _filtro = filtro;
        }
    }

    private IEnumerable<Lote> LotesFiltrados()
    {
        return _filtro switch
        {
            FiltroPlanilla.Asignadas  => _lotes.Where(l => l.Planillas.Any(p => p.IsAssigned)),
            FiltroPlanilla.SinAsignar => _lotes.Where(l => l.Planillas.Any(p => !p.IsAssigned)),
            _                         => _lotes
        };
    }

    private List<Planilla> PlanillasFiltradas(Lote lote)
    {
        return _filtro switch
        {
            FiltroPlanilla.Asignadas  => lote.Planillas.Where(p => p.IsAssigned).ToList(),
            FiltroPlanilla.SinAsignar => lote.Planillas.Where(p => !p.IsAssigned).ToList(),
            _                         => lote.Planillas.ToList()
        };
    }

    private async Task GenerarPlanillas()
    {
        if (_quinielaCerrada)
        {
            _error = "La quiniela está cerrada. No se pueden generar más planillas.";
            return;
        }

        _generando = true;
        _error = "";
        try
        {
            _ultimoLote = await LoteService.CreateAsync(_cantidad);
            _lotes = await LoteService.GetAsync();
        }
        catch (Exception ex)
        {
            _error = $"Error al generar el lote: {ex.Message}";
        }
        finally
        {
            _generando = false;
        }
    }

    private void ConfirmarEliminarLote(Lote lote)
    {
        _seleccionado = lote;
        _showConfirmDelete = true;
    }

    private async Task Eliminar()
    {
        if (_seleccionado is null) return;

        if (_quinielaCerrada)
        {
            _error = "La quiniela está cerrada. No se pueden eliminar lotes.";
            return;
        }

        try
        {
            var (exito, mensaje) = await LoteService.EliminarAsync(_seleccionado.Id);
            if (exito)
            {
                _lotes = await LoteService.GetAsync();
                _showConfirmDelete = false;
                _seleccionado = null;
                _ultimoLote = null;
                _error = "";
            }
            else
            {
                _error = mensaje;
            }
        }
        catch (Exception ex)
        {
            _error = $"Error al eliminar: {ex.Message}";
        }
    }
}
