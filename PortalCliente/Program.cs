using PortalCliente.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging
builder.ConfigureLogging();

// Configure Authentication
builder.Services.AddSecureAuthentication(builder.Configuration);

// Configure Application Services
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure Request Pipeline
app.ConfigurePipeline();

app.Run();
