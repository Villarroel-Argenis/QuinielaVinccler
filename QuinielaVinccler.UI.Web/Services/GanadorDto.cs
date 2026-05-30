namespace QuinielaVinccler.UI.Web.Services;

public enum TipoPremio
{
    Primero,
    Segundo,
    Tercero,
    MenorPuntaje
}

public sealed record GanadorDto(
    TipoPremio Tipo,
    int Posicion,           // En el ranking (1, 2, 3, último)
    string CodigoPlanilla,
    int PlanillaId,
    string NombreUsuario,
    int PuntajeTotal,
    decimal MontoBase,      // El premio base completo
    decimal MontoGanado,    // Lo que se lleva (si hay empate, es base / cantidadEmpatados)
    int CantidadEmpatados   // 1 si no hay empate, 2+ si lo hay
);