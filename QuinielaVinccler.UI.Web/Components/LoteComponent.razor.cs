namespace QuinielaVinccler.UI.Web.Components;

public partial class LoteComponent
{
    private int _cantidad = 1;
    private bool _generando = false;
    private Lote? _ultimoLote;
    private List<Lote> _lotes = [];
    private Lote? _seleccionado;
    private bool _showConfirmDelete = false;
    private string _error = "";

    [Inject]private LoteService LoteService { get; set; } = default!;

    protected override async  Task OnInitializedAsync()
    {
        _lotes = await LoteService.GetAsync();
    }

    private async Task GenerarPlanillas()
    {
        _generando = true;
        _ultimoLote = await LoteService.CreateAsync(_cantidad);
        _lotes = await LoteService.GetAsync();
        _generando = false;
    }

    private void ConfirmarEliminarLote(Lote lote)
    {
        _seleccionado = lote;
        _showConfirmDelete = true;
    }

    private async Task Eliminar()
    {
        if (_seleccionado is null) return;
        var (Exito, Mensaje) = await LoteService.EliminarAsync(_seleccionado.Id);
        if (Exito)
        {
            _lotes = await LoteService.GetAsync();
            _showConfirmDelete = false;
            _seleccionado = null;
            _ultimoLote = null;
            _error = "";
        }
        else
        {
            _error = Mensaje;
        }
    }

}
