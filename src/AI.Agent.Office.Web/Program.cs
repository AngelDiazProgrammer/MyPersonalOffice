using AI.Agent.Office.Api.Extensions;
using AI.Agent.Office.Infrastructure.Configuration;
using AI.Agent.Office.Web.Components;
using AI.Agent.Office.Web.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddOfficeConfiguration();

// By default the Web host also hosts the API in-process, so a single command runs everything.
// Set Office:HostApi=false and Api:BaseUrl to consume a separately running AI.Agent.Office.Api instead.
var hostApi = builder.Configuration.GetValue("Office:HostApi", true);
if (hostApi) builder.Services.AddOfficeApi(builder.Configuration);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.Configure<WebApiOptions>(builder.Configuration.GetSection(WebApiOptions.SectionName));
builder.Services.AddHttpClient<OfficeApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<WebApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromMinutes(10); // a delegated request can take a while
});
builder.Services.AddScoped<OfficeStateService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

if (hostApi) app.UseOfficeApi();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
