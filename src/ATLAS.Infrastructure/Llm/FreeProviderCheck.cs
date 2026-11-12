using Microsoft.Extensions.Hosting;

namespace ATLAS.Infrastructure.Llm;

/// <summary>
/// Al arrancar, verifica en segundo plano que el modelo configurado de cada proveedor gratuito esté
/// disponible para su clave.
/// </summary>
public sealed class FreeProviderCheck : IHostedService
{
    private readonly FreeApiService _free;

    public FreeProviderCheck(FreeApiService free) => _free = free;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => _free.ValidateModelsAsync(CancellationToken.None)); // never delays startup
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
