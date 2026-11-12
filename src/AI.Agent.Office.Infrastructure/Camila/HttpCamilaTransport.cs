using System.Net.Http.Headers;
using System.Text;
using AI.Agent.Office.Application.Camila;

namespace AI.Agent.Office.Infrastructure.Camila;

/// <summary>
/// Transporte HTTP de los webhooks de Camila. Sin redirecciones (un 30x no puede llevar la petición a otro host fuera de la lista
/// permitida) y con el tiempo límite de cada llamada; la lista de hosts la valida el servicio de webhooks antes de llegar aquí.
/// </summary>
public sealed class HttpCamilaTransport : ICamilaHttpTransport
{
    public const string HttpClientName = "camila-webhooks";

    private readonly IHttpClientFactory _factory;

    public HttpCamilaTransport(IHttpClientFactory factory) => _factory = factory;

    /// <summary>Envía la petición y devuelve el estado y el cuerpo.</summary>
    public async Task<CamilaHttpResponse> SendAsync(CamilaHttpRequest request, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, request.TimeoutSeconds)));

        using var message = new HttpRequestMessage(new HttpMethod(request.Method), request.Url);
        if (request.Body is not null) message.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");

        foreach (var (name, value) in request.Headers)
        {
            if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                if (message.Content is not null && MediaTypeHeaderValue.TryParse(value, out var type)) message.Content.Headers.ContentType = type;
                continue;
            }
            message.Headers.TryAddWithoutValidation(name, value);
        }

        var client = _factory.CreateClient(HttpClientName);
        using var response = await client.SendAsync(message, timeout.Token).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);
        return new CamilaHttpResponse((int)response.StatusCode, body);
    }
}
