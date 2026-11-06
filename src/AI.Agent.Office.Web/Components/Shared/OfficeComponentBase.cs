using AI.Agent.Office.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AI.Agent.Office.Web.Components.Shared;

/// <summary>Vuelve a dibujar el componente cada vez que cambia el estado de la oficina (eventos de SignalR).</summary>
public abstract class OfficeComponentBase : ComponentBase, IDisposable
{
    [Inject] protected OfficeStateService Office { get; set; } = default!;

    protected override void OnInitialized() => Office.Changed += HandleChanged;

    private void HandleChanged()
    {
        _ = InvokeAsync(() =>
        {
            try { StateHasChanged(); }
            catch (ObjectDisposedException) { /* circuit closed */ }
        });
    }

    public virtual void Dispose() => Office.Changed -= HandleChanged;
}
