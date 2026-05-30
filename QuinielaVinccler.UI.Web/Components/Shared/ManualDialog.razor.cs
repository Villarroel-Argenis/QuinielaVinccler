using Markdig;

namespace QuinielaVinccler.UI.Web.Components.Shared;

public partial class ManualDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    /// <summary>
    /// Si true, muestra el manual completo (común + admin).
    /// Si false, solo la sección de usuario común.
    /// </summary>
    [Parameter] public bool MostrarAdmin { get; set; }

    private string _html = "";

    protected override void OnParametersSet()
    {
        var markdown = MostrarAdmin ? ManualContent.Completo : ManualContent.Comun;

        // Pipeline con extensiones útiles: tablas, blockquotes, listas, etc.
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        _html = Markdown.ToHtml(markdown, pipeline);
    }

    private void Cerrar() => MudDialog.Close();
}
