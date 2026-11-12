using AI.Agent.Office.Api.Auth;
using AI.Agent.Office.Api.Extensions;
using AI.Agent.Office.Web.Components;
using AI.Agent.Office.Web.Services;
using Microsoft.Extensions.Options;

// Camila IA (spec 009): `dotnet run --project src/AI.Agent.Office.Web -- --camila-generar` genera CamilaIA.md desde Bots/ y termina.
if (OfficeCommands.TryRun(args, out var commandExitCode)) return commandExitCode;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddOfficeConfiguration();
builder.Logging.AddOfficeLogging();

// Por defecto el host Web también aloja la API en el mismo proceso, así un solo comando ejecuta todo.
// Con Office:HostApi=false y Api:BaseUrl se consume en su lugar una AI.Agent.Office.Api que corra por separado.
var hostApi = builder.Configuration.GetValue("Office:HostApi", true);
if (hostApi) builder.Services.AddOfficeApi(builder.Configuration);

builder.Services.AddOfficeAuth(builder.Configuration);      // acceso por usuario y contraseña (spec 010)
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddOfficeRendering();   // contrato IMarkdownRenderer (Application); la implementación se compone en Api
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.Configure<WebApiOptions>(builder.Configuration.GetSection(WebApiOptions.SectionName));
builder.Services.AddHttpClient<OfficeApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<WebApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromMinutes(10); // una solicitud delegada puede tardar
    client.DefaultRequestHeaders.Add("X-Office-Client", "web"); // la API exige esta cabecera a las peticiones que cambian algo
    client.DefaultRequestHeaders.Add(InternalApiToken.HeaderName, sp.GetRequiredService<InternalApiToken>().Value); // identifica a la interfaz ante su propia API (nunca sale al navegador)
});
builder.Services.AddScoped<OfficeStateService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseOfficeSecurity();     // cabeceras de seguridad y bloqueo de peticiones de otros sitios (también protege el canal de Blazor)
app.UseStaticFiles();
app.UseOfficeAuth();         // sesión obligatoria en todo lo que no sea /login, /auth y /health
app.UseAntiforgery();
app.MapOfficeAuth();

if (hostApi) app.UseOfficeApi();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
return 0;
