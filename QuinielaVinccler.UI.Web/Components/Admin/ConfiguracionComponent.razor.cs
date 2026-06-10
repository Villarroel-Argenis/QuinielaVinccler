namespace QuinielaVinccler.UI.Web.Components.Admin;

public partial class ConfiguracionComponent : ComponentBase
{
    [Inject] private IConfiguracionService ConfigSvc { get; set; } = default!;

    private bool _cargando = true;
    private bool _guardando;
    private bool _cierreManual;
    private DateTime? _fechaCierreLocal;
    private TimeSpan? _horaCierre = new(23, 59, 0);

    private string _mensaje = "";
    private Severity _mensajeSeveridad = Severity.Info;

    private bool _permitirIncompletas;
    private bool _quinielaCerrada;

    protected override async Task OnInitializedAsync()
    {
        await CargarConfiguracionAsync();
        _cargando = false;
    }

    private async Task CargarConfiguracionAsync()
    {
        var incompletasStr = await ConfigSvc.GetAsync(ConfiguracionKeys.PermitirIncompletasEnRanking);
        _permitirIncompletas = incompletasStr == "true";

        // Para _quinielaCerrada usa QuinielaCerradaAsync:
        _quinielaCerrada = await ConfigSvc.QuinielaCerradaAsync();

        // Toggle manual
        var manualStr = await ConfigSvc.GetAsync(ConfiguracionKeys.QuinielaCerrada);
        _cierreManual = manualStr == "true";

        // Fecha programada
        var fechaStr = await ConfigSvc.GetAsync(ConfiguracionKeys.FechaCierreUtc);
        if (fechaStr is not null && DateTimeOffset.TryParse(fechaStr, out var fechaUtc))
        {
            var local = fechaUtc.ToLocalTime();
            _fechaCierreLocal = local.Date;
            _horaCierre = local.TimeOfDay;
        }
        else
        {
            _fechaCierreLocal = null;
            _horaCierre = new(23, 59, 0);
        }
    }

    private async Task TogglePermitirIncompletas(bool valor)
    {
        _guardando = true;
        _mensaje = "";
        try
        {
            _permitirIncompletas = valor;
            await ConfigSvc.SetAsync(ConfiguracionKeys.PermitirIncompletasEnRanking, valor ? "true" : "false");
            MostrarMensaje(
                valor ? "Todas las planillas asignadas participarán en el ranking."
                      : "Solo planillas con 149 predicciones participarán en el ranking.",
                Severity.Success);
        }
        catch (Exception ex)
        {
            _permitirIncompletas = !valor;
            MostrarMensaje($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _guardando = false;
        }
    }

    // ── Toggle manual ────────────────────────────────────────────────────────
    private async Task ToggleCierreManual(bool valor)
    {
        _guardando = true;
        _mensaje = "";
        try
        {
            _cierreManual = valor;
            await ConfigSvc.SetAsync(ConfiguracionKeys.QuinielaCerrada, valor ? "true" : "false");
            MostrarMensaje(
                valor ? "Quiniela cerrada manualmente." : "Cierre manual desactivado.",
                Severity.Success);
        }
        catch (Exception ex)
        {
            _cierreManual = !valor; // revertir si falla
            MostrarMensaje($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _guardando = false;
        }
    }

    // ── Fecha programada ─────────────────────────────────────────────────────
    private void OnFechaChanged(DateTime? fecha)
    {
        _fechaCierreLocal = fecha;
    }

    private void OnHoraChanged(TimeSpan? hora)
    {
        _horaCierre = hora ?? new TimeSpan(23, 59, 0);
    }

    private async Task GuardarFecha()
    {
        if (_fechaCierreLocal is null) return;

        _guardando = true;
        _mensaje = "";
        try
        {
            var hora = _horaCierre ?? new TimeSpan(23, 59, 0);
            var fechaLocal = _fechaCierreLocal.Value.Date.Add(hora);
            var fechaUtc = new DateTimeOffset(fechaLocal, TimeZoneInfo.Local.GetUtcOffset(fechaLocal))
                .ToUniversalTime();

            await ConfigSvc.SetAsync(ConfiguracionKeys.FechaCierreUtc, fechaUtc.ToString("O"));
            MostrarMensaje(
                $"Fecha de cierre guardada: {fechaLocal:dd/MM/yyyy HH:mm} (hora local).",
                Severity.Success);
        }
        catch (Exception ex)
        {
            MostrarMensaje($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _guardando = false;
        }
    }

    private async Task LimpiarFecha()
    {
        _guardando = true;
        _mensaje = "";
        try
        {
            await ConfigSvc.SetAsync(ConfiguracionKeys.FechaCierreUtc, "");
            _fechaCierreLocal = null;
            _horaCierre = new(23, 59, 0);
            MostrarMensaje("Fecha de cierre eliminada.", Severity.Success);
        }
        catch (Exception ex)
        {
            MostrarMensaje($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _guardando = false;
        }
    }

    // ── Estado consolidado ───────────────────────────────────────────────────
    private (Severity Severidad, string Icono, string Titulo, string Detalle) GetEstadoConsolidado()
    {
        // Prioridad 1: cierre manual
        if (_cierreManual)
        {
            return (
                Severity.Error,
                Icons.Material.Filled.Lock,
                "Quiniela CERRADA manualmente",
                "El cierre manual está activo. Desactívalo para que la fecha programada vuelva a aplicar."
            );
        }

        // Prioridad 2: fecha pasada
        if (_fechaCierreLocal is not null)
        {
            var hora = _horaCierre ?? new TimeSpan(23, 59, 0);
            var fechaLocal = _fechaCierreLocal.Value.Date.Add(hora);
            var fechaUtc = new DateTimeOffset(fechaLocal, TimeZoneInfo.Local.GetUtcOffset(fechaLocal))
                .ToUniversalTime();

            if (DateTimeOffset.UtcNow >= fechaUtc)
            {
                return (
                    Severity.Error,
                    Icons.Material.Filled.Schedule,
                    "Quiniela CERRADA por fecha",
                    $"La fecha límite ({fechaLocal:dd/MM/yyyy HH:mm} hora local) ya pasó."
                );
            }

            var restante = fechaUtc - DateTimeOffset.UtcNow;
            return (
                Severity.Warning,
                Icons.Material.Filled.Timer,
                "Quiniela ABIERTA — cierre programado",
                $"Cierra automáticamente el {fechaLocal:dd/MM/yyyy HH:mm} (hora local). " +
                $"Tiempo restante: {FormatRestante(restante)}."
            );
        }

        return (
            Severity.Success,
            Icons.Material.Filled.LockOpen,
            "Quiniela ABIERTA",
            "No hay cierre manual ni fecha programada."
        );
    }

    private static string FormatRestante(TimeSpan t)
    {
        if (t.TotalDays >= 1) return $"{(int)t.TotalDays}d {t.Hours}h";
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes}m";
        return $"{t.Minutes}m";
    }

    private void MostrarMensaje(string texto, Severity severidad)
    {
        _mensaje = texto;
        _mensajeSeveridad = severidad;
    }
}