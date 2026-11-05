using AI.Agent.Office.Api.Extensions;
using AI.Agent.Office.Infrastructure.Configuration;

// Standalone API host (optional). The Web project hosts the same API in-process by default.
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddOfficeConfiguration();
builder.Services.AddOfficeApi(builder.Configuration);

var app = builder.Build();
app.UseOfficeApi();
app.Run();
