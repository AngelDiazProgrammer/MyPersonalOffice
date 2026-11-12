using ATLAS.Api.Auth;
using ATLAS.Api.Extensions;

// Host de la API independiente (opcional). Por defecto el proyecto Web aloja la misma API en su proceso.
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddOfficeConfiguration();
builder.Logging.AddOfficeLogging();
builder.Services.AddOfficeApi(builder.Configuration);
builder.Services.AddOfficeAuth(builder.Configuration);

var app = builder.Build();
app.UseOfficeSecurity();
app.UseOfficeAuth();
app.UseAntiforgery();
app.MapOfficeAuth();
app.UseOfficeApi();
app.Run();
