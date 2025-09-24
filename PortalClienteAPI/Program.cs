using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PortalClienteAPI.Services.Interface;
using PortalClienteAPI.Middleware;
using Refit;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Adicionar servi�os para o JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("c2296ee5-453d-4e85-9680-b68ffbf5d99c")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Adicionar servi�o CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
            builder.AllowAnyOrigin()                   
                   .AllowAnyHeader()
                   .AllowAnyMethod());
});

// Add services to the container.

builder.Services.AddControllers();

var sapHttpClient = new HttpClient()
{
    BaseAddress = new Uri("http://srv-sap-prd.esgas.com.br:8000/sap/bc/inbound"),
};

var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("DATAGAS:Datagas@2023"));
sapHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAll");

app.Run();
