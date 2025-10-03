using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configura para escutar em uma porta específica
builder.WebHost.UseUrls("http://localhost:8080");

// Configura logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Adiciona suporte a CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Habilita CORS
app.UseCors();

// Middleware para logar todas as requisições
app.Use(async (context, next) =>
{
    app.Logger.LogInformation("Request: {Method} {Path} from {RemoteIP}",
        context.Request.Method,
        context.Request.Path,
        context.Connection.RemoteIpAddress);

    // Log headers importantes
    if (context.Request.Headers.ContainsKey("token"))
    {
        app.Logger.LogInformation("Token header: {Token}", context.Request.Headers["token"]);
    }

    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        app.Logger.LogInformation("Authorization header present");
    }

    await next();
});

// ========================================
// 1. ENDPOINT DE AUTENTICAÇÃO
// POST /sap/bc/inbound/DATAGAS003
// ========================================
app.MapPost("/sap/bc/inbound/DATAGAS003", async (HttpContext context) =>
{
    try
    {
        // Lê o body da requisição
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        app.Logger.LogInformation("Authentication request body: {Body}", body);

        if (string.IsNullOrWhiteSpace(body))
        {
            app.Logger.LogWarning("Empty body received in authentication request");
            return Results.BadRequest(new { error = "Empty request body" });
        }

        // Parse do AuthRequest
        AuthRequest? authRequest = null;
        try
        {
            authRequest = JsonSerializer.Deserialize<AuthRequest>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException jsonEx)
        {
            app.Logger.LogError(jsonEx, "Failed to parse JSON body: {Body}", body);
            return Results.BadRequest(new { error = "Invalid JSON format", detail = jsonEx.Message });
        }

        app.Logger.LogInformation("Authentication request - ClientNumber: {ClientNumber}, Cpf: {Cpf}, Cnpj: {Cnpj}",
            authRequest?.ClientNumber,
            authRequest?.Cpf,
            authRequest?.Cnpj);

        // Valida credenciais (ClientNumber é obrigatório, e CPF ou CNPJ deve estar preenchido)
        if (string.IsNullOrEmpty(authRequest?.ClientNumber))
        {
            app.Logger.LogWarning("Invalid authentication request - missing ClientNumber");
            return Results.BadRequest(new { error = "ClientNumber is required" });
        }

        if (string.IsNullOrEmpty(authRequest?.Cpf) && string.IsNullOrEmpty(authRequest?.Cnpj))
        {
            app.Logger.LogWarning("Invalid authentication request - missing Cpf or Cnpj");
            return Results.BadRequest(new { error = "Cpf or Cnpj is required" });
        }

        // Retorna resposta de sucesso
        var authResponse = new
        {
            ClientCode = authRequest.ClientNumber,
            ClientName = $"Cliente Mock - {authRequest.ClientNumber}",
            Token = $"mock-token-{authRequest.ClientNumber}"
        };

        app.Logger.LogInformation("Authentication successful for client: {ClientNumber}", authRequest.ClientNumber);
        return Results.Ok(authResponse);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error in authentication endpoint");
        return Results.StatusCode(500);
    }
});

// ========================================
// 2. ENDPOINT DE LISTAR FATURAS
// GET /sap/bc/inbound/DATAGAS004
// ========================================
app.MapGet("/sap/bc/inbound/DATAGAS004", (HttpContext context) =>
{
    try
    {
        // Verifica se tem o header token
        if (!context.Request.Headers.ContainsKey("token"))
        {
            app.Logger.LogWarning("Missing token header in get invoices request");
            return Results.Unauthorized();
        }

        var token = context.Request.Headers["token"].ToString();
        app.Logger.LogInformation("Get invoices request with token: {Token}", token);

        // Retorna lista de faturas mock
        var invoicesData = new
        {
            Invoices = GetMockInvoices()
        };

        app.Logger.LogInformation("Returning {Count} invoices", invoicesData.Invoices.Length);
        return Results.Ok(invoicesData);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error in get invoices endpoint");
        return Results.StatusCode(500);
    }
});

// ========================================
// 3. ENDPOINT DE CONTEÚDO DA FATURA
// GET /sap/bc/inbound/DATAGAS005 (com body!)
// ========================================
app.MapGet("/sap/bc/inbound/DATAGAS005", async (HttpContext context) =>
{
    try
    {
        // Verifica se tem o header token
        if (!context.Request.Headers.ContainsKey("token"))
        {
            app.Logger.LogWarning("Missing token header in get invoice content request");
            return Results.Unauthorized();
        }

        var token = context.Request.Headers["token"].ToString();
        app.Logger.LogInformation("Get invoice content request with token: {Token}", token);

        string bodyContent = "";

        // Lê o body mesmo sendo GET (comportamento não convencional mas necessário)
        if (context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body);
            bodyContent = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            app.Logger.LogInformation("GET request body: {Body}", bodyContent);
        }
        else
        {
            app.Logger.LogWarning("GET request without body - ContentLength: {Length}", context.Request.ContentLength);
        }

        // Parse do body para extrair o document
        string document = "DOC001";
        string invoiceNumber = "INV001";

        if (!string.IsNullOrEmpty(bodyContent))
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(bodyContent);

                if (jsonDoc.RootElement.TryGetProperty("document", out var docElement))
                {
                    document = docElement.GetString() ?? "DOC001";
                }

                if (jsonDoc.RootElement.TryGetProperty("invoiceNumber", out var invElement))
                {
                    invoiceNumber = invElement.GetString() ?? "INV001";
                }

                app.Logger.LogInformation("Parsed request - Document: {Document}, Invoice: {Invoice}",
                    document, invoiceNumber);
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Error parsing JSON body: {Body}", bodyContent);
            }
        }

        // Decide qual invoice retornar baseado no document
        object invoice;

        if (document == "DOC002")
        {
            app.Logger.LogInformation("Returning invoice WITH PDF for DOC002");
            invoice = GetMockInvoiceWithPdf(document, invoiceNumber);
        }
        else
        {
            app.Logger.LogInformation("Returning invoice WITHOUT PDF for document: {Document}", document);
            invoice = GetMockInvoiceContent(document, invoiceNumber);
        }

        return Results.Ok(invoice);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error in get invoice content endpoint");
        return Results.StatusCode(500);
    }
});

// ========================================
// ENDPOINT DE HEALTH CHECK
// ========================================
app.MapGet("/health", () =>
{
    app.Logger.LogInformation("Health check requested");
    return Results.Ok(new { status = "healthy", timestamp = DateTime.Now });
});

// ========================================
// ENDPOINT RAIZ
// ========================================
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        service = "Mock SAP Server",
        version = "1.0.0",
        endpoints = new[]
        {
            "POST /sap/bc/inbound/DATAGAS003?sap-client=600 - Authentication",
            "GET  /sap/bc/inbound/DATAGAS004?sap-client=600 - List Invoices (requires token header)",
            "GET  /sap/bc/inbound/DATAGAS005?sap-client=600 - Invoice Content (requires token header and body)",
            "GET  /health - Health check"
        },
        notes = new[]
        {
            "DOC002 returns invoice WITH PDF",
            "Other documents return invoice WITHOUT PDF",
            "GET with body is supported for DATAGAS005 endpoint"
        }
    });
});

// ========================================
// FUNÇÕES AUXILIARES - DADOS MOCK
// ========================================
static object[] GetMockInvoices()
{
    return new[]
    {
        new
        {
            document = "DOC001",
            invoiceNumber = "INV001",
            barcodeNumber = "123456789012345678901234567890123456789012345",
            createdAt = "2024-01-15",
            duedate = "2024-02-15",
            value = "150.75",
            penalty = "0.00",
            negativo = "NAO",
            clientnumber = "12345",
            address = "Rua das Flores, 123 - Centro - São Paulo/SP",
            status = "Em aberto",
            invoiceStatus = "PENDENTE",
            pdfContent = ""
        },
        new
        {
            document = "DOC002",
            invoiceNumber = "INV002",
            barcodeNumber = "098765432109876543210987654321098765432109876",
            createdAt = "2024-02-15",
            duedate = "2024-03-15",
            value = "89.50",
            penalty = "2.50",
            negativo = "NAO",
            clientnumber = "12345",
            address = "Rua das Flores, 123 - Centro - São Paulo/SP",
            status = "Vencida",
            invoiceStatus = "VENCIDA",
            pdfContent = ""
        },
        new
        {
            document = "DOC003",
            invoiceNumber = "INV003",
            barcodeNumber = "567890123456789012345678901234567890123456789",
            createdAt = "2024-03-15",
            duedate = "2024-04-15",
            value = "205.25",
            penalty = "0.00",
            negativo = "NAO",
            clientnumber = "12345",
            address = "Rua das Flores, 123 - Centro - São Paulo/SP",
            status = "Paga",
            invoiceStatus = "PAGA",
            pdfContent = ""
        }
    };
}

static object GetMockInvoiceContent(string document, string invoiceNumber)
{
    return new
    {
        document = document,
        invoiceNumber = invoiceNumber,
        barcodeNumber = "123456789012345678901234567890123456789012345",
        createdAt = "2024-01-15",
        duedate = "2024-02-15",
        value = "150.75",
        penalty = "0.00",
        negativo = "NAO",
        clientnumber = "12345",
        address = "Rua das Flores, 123 - Centro - São Paulo/SP",
        status = "Em aberto",
        invoiceStatus = "PENDENTE",
        pdfContent = "" // SEM PDF
    };
}

static object GetMockInvoiceWithPdf(string document, string invoiceNumber)
{
    return new
    {
        document = document,
        invoiceNumber = invoiceNumber,
        barcodeNumber = "098765432109876543210987654321098765432109876",
        createdAt = "2024-02-15",
        duedate = "2024-03-15",
        value = "89.50",
        penalty = "2.50",
        negativo = "NAO",
        clientnumber = "12345",
        address = "Rua das Flores, 123 - Centro - São Paulo/SP",
        status = "Vencida",
        invoiceStatus = "VENCIDA",
        // PDF fake em base64 (um PDF mínimo válido)
        pdfContent = "JVBERi0xLjQKMSAwIG9iago8PC9UeXBlL0NhdGFsb2cvUGFnZXMgMiAwIFI+PgplbmRvYmoKMiAwIG9iago8PC9UeXBlL1BhZ2VzL0tpZHNbMyAwIFJdL0NvdW50IDE+PgplbmRvYmoKMyAwIG9iago8PC9UeXBlL1BhZ2UvTWVkaWFCb3hbMCAwIDYxMiA3OTJdL1BhcmVudCAyIDAgUi9SZXNvdXJjZXM8PC9Gb250PDwvRjE8PC9UeXBlL0ZvbnQvU3VidHlwZS9UeXBlMS9CYXNlRm9udC9IZWx2ZXRpY2E+Pj4+Pj4vQ29udGVudHMgNCAwIFI+PgplbmRvYmoKNCAwIG9iago8PC9MZW5ndGggNDQ+PgpzdHJlYW0KQlQKL0YxIDEyIFRmCjcyIDcxMiBUZAooRmFrZSBJbnZvaWNlKSBUagpFVAplbmRzdHJlYW0KZW5kb2JqCnhyZWYKMCA1CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMDc0IDAwMDAwIG4gCjAwMDAwMDAxMzEgMDAwMDAgbiAKMDAwMDAwMDI5MSAwMDAwMCBuIAp0cmFpbGVyCjw8L1NpemUgNS9Sb290IDEgMCBSPj4Kc3RhcnR4cmVmCjM4OQolJUVPRg=="
    };
}

// ========================================
// INICIALIZAÇÃO DO SERVIDOR
// ========================================
app.Logger.LogInformation("=====================================");
app.Logger.LogInformation("Mock SAP Server started successfully!");
app.Logger.LogInformation("=====================================");
app.Logger.LogInformation("Listening on: http://localhost:8080");
app.Logger.LogInformation("");
app.Logger.LogInformation("Available endpoints:");
app.Logger.LogInformation("  POST /sap/bc/inbound/DATAGAS003?sap-client=600 - Authentication");
app.Logger.LogInformation("  GET  /sap/bc/inbound/DATAGAS004?sap-client=600 - List invoices");
app.Logger.LogInformation("  GET  /sap/bc/inbound/DATAGAS005?sap-client=600 - Invoice content");
app.Logger.LogInformation("");
app.Logger.LogInformation("Special behaviors:");
app.Logger.LogInformation("  - DOC002: Returns invoice WITH PDF");
app.Logger.LogInformation("  - Others: Return invoice WITHOUT PDF");
app.Logger.LogInformation("=====================================");

app.Run();

// ========================================
// MODELOS DE DADOS
// ========================================
record AuthRequest(string ClientNumber, string Cpf, string Cnpj);
