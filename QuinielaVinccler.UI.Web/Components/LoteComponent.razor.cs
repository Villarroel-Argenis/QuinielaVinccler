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

    [Inject]private ILoteService LoteService { get; set; } = default!;

    protected override async  Task OnInitializedAsync()
    {
        _lotes = await LoteService.GetAsync();
    }

    private async Task GenerarPlanillas()
    {
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
