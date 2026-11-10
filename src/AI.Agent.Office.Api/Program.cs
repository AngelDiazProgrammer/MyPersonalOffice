using AI.Agent.Office.Api.Extensions;

// Host de la API independiente (opcional). Por defecto el proyecto Web aloja la misma API en su proceso.
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddOfficeConfiguration();
builder.Logging.AddOfficeLogging();
builder.Services.AddOfficeApi(builder.Configuration);

var app = builder.Build();
app.UseOfficeSecurity();
app.UseOfficeApi();
app.Run();
