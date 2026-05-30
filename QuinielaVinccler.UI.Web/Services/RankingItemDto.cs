namespace QuinielaVinccler.UI.Web.Services;

public sealed record RankingItemDto(
    int Posicion,
    string Nombre,
    string CodigoPlanilla,
    int PlanillaId,
    int PuntajeTotal,
    bool EsUsuarioActual,
    int UserId
);