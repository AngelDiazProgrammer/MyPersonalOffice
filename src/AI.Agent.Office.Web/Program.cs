using AI.Agent.Office.Api.Extensions;
using AI.Agent.Office.Web.Components;
using AI.Agent.Office.Web.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddOfficeConfiguration();
builder.Logging.AddOfficeLogging();

// Por defecto el host Web también aloja la API en el mismo proceso, así un solo comando ejecuta todo.
// Con Office:HostApi=false y Api:BaseUrl se consume en su lugar una AI.Agent.Office.Api que corra por separado.
var hostApi = builder.Configuration.GetValue("Office:HostApi", true);
if (hostApi) builder.Services.AddOfficeApi(builder.Configuration);

builder.Services.AddOfficeRendering();   // contrato IMarkdownRenderer (Application); la implementación se compone en Api
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.Configure<WebApiOptions>(builder.Configuration.GetSection(WebApiOptions.SectionName));
builder.Services.AddHttpClient<OfficeApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<WebApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromMinutes(10); // una solicitud delegada puede tardar
    client.DefaultRequestHeaders.Add("X-Office-Client", "web"); // la API exige esta cabecera a las peticiones que cambian algo
});
builder.Services.AddScoped<OfficeStateService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseOfficeSecurity();     // cabeceras de seguridad y bloqueo de peticiones de otros sitios (también protege el canal de Blazor)
app.UseStaticFiles();
app.UseAntiforgery();

if (hostApi) app.UseOfficeApi();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
