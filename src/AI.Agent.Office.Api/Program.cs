using AI.Agent.Office.Api.Extensions;
using AI.Agent.Office.Infrastructure.Configuration;

// Host de la API independiente (opcional). Por defecto el proyecto Web aloja la misma API en su proceso.
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddOfficeConfiguration();
builder.Services.AddOfficeApi(builder.Configuration);

var app = builder.Build();
app.UseOfficeApi();
app.Run();
