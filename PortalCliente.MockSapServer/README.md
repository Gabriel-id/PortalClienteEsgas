# PortalCliente.MockSapServer

Mock Server que simula as APIs do SAP para desenvolvimento local do Portal Cliente Esgas.

## Descrição

Este é um servidor HTTP standalone desenvolvido com **ASP.NET Core Minimal APIs** que simula os endpoints do SAP necessários para o funcionamento do Portal Cliente. Permite desenvolvimento e testes sem necessidade de acesso ao servidor SAP real.

## Como Executar

### Opção 1: Executar Ambos os Projetos (Recomendado para Desenvolvimento Local)

Para desenvolvimento local, configure a solution para iniciar o Mock Server e a aplicação principal simultaneamente:

**Visual Studio 2022:**
1. Abra a solution `PortalCliente.sln`
2. Clique com botão direito na Solution → **Properties**
3. Em **Startup Project**, selecione **Multiple startup projects**
4. Configure:
   - `PortalCliente.MockSapServer` → **Start**
   - `PortalCliente` → **Start**
5. Pressione F5 ou Ctrl+F5

**Visual Studio Code / Linha de Comando:**

Terminal 1 (Mock Server):
```bash
cd PortalCliente.MockSapServer
dotnet run
```

Terminal 2 (Aplicação Principal):
```bash
cd PortalCliente
dotnet run
```

### Opção 2: Script Batch (Windows)

Na raiz do projeto principal:

```bash
run-mock-server.bat
```

### Opção 3: Executar Somente o Mock Server

```bash
cd PortalCliente.MockSapServer
dotnet run
```

## Endpoints Disponíveis

O servidor roda em **http://localhost:8080** e expõe os seguintes endpoints:

### 1. Autenticação
- **Método**: POST
- **URL**: `/sap/bc/inbound/DATAGAS003?sap-client=600`
- **Body**: Requer `ClientNumber` + (`Cpf` OU `Cnpj`)
- **Comportamento**: Aceita qualquer credencial e retorna `ClientCode`, `ClientName` e `Token` mockados

### 2. Listar Faturas
- **Método**: GET
- **URL**: `/sap/bc/inbound/DATAGAS004?sap-client=600`
- **Headers**: `token: {valor}`
- **Comportamento**: Retorna array com 3 faturas mockadas (DOC001, DOC002, DOC003). Propriedades em camelCase correspondendo aos `JsonPropertyName` dos DTOs

### 3. Conteúdo da Fatura
- **Método**: GET (com body - comportamento não convencional mas necessário)
- **URL**: `/sap/bc/inbound/DATAGAS005?sap-client=600`
- **Headers**: `token: {valor}`
- **Body**: `document`, `clientNumber`, `invoiceNumber`
- **Comportamento**:
  - **DOC002**: Retorna fatura COM `pdfContent` em base64 (PDF válido)
  - **Outros documentos**: Retorna fatura SEM `pdfContent` (campo vazio)
  - Propriedades em camelCase correspondendo aos `JsonPropertyName` dos DTOs

### 4. Health Check
- **Método**: GET
- **URL**: `/health`
- **Comportamento**: Retorna status `healthy` com timestamp

### 5. Documentação
- **Método**: GET
- **URL**: `/`
- **Comportamento**: Retorna JSON com informações sobre todos os endpoints disponíveis

## Comportamentos Especiais

### Download de PDF

O mock server retorna PDF em base64 de forma condicional:

- **DOC002**: Retorna COM PDF válido em base64
- **DOC001, DOC003 e outros**: Retornam SEM PDF (campo `PdfContent` vazio)

Isso permite testar ambos os cenários:
- Download bem-sucedido (DOC002)
- Tratamento de erro quando PDF não está disponível (DOC001, DOC003)

### Logs Detalhados

O servidor registra logs detalhados de todas as requisições:

```
Request: POST /sap/bc/inbound/DATAGAS003 from ::1
Authentication request body: {"ClientNumber":"12345","Password":"senha123"}
Authentication request for ClientNumber: 12345
Authentication successful for client: 12345
```

## Configurar a Aplicação Principal

Para usar o Mock Server, configure o `appsettings.Development.json` da aplicação principal:

```json
{
  "SapService": {
    "BaseUrl": "http://localhost:8080/sap/bc/inbound/",
    "SapClient": "600",
    "Username": "mock",
    "Password": "mock",
    "TimeoutSeconds": 30,
    "Endpoints": {
      "Authentication": "DATAGAS003",
      "GetInvoices": "DATAGAS004",
      "GetInvoiceContent": "DATAGAS005"
    }
  }
}
```

## Tecnologias

- **.NET 8** - Framework
- **ASP.NET Core Minimal APIs** - Endpoints HTTP
- **System.Text.Json** - Serialização JSON

## Estrutura do Código

```csharp
Program.cs
├── Configuração do WebHost (porta 8080)
├── Logging (Console)
├── CORS (habilitado)
├── Middleware de logging de requisições
├── Endpoints (3 endpoints SAP + health + docs)
├── Record types (AuthRequest)
└── Funções auxiliares (GetMockInvoices, etc.)
```

## Notas Importantes

1. **GET com Body**: O endpoint DATAGAS005 aceita body em requisição GET (comportamento não convencional no HTTP, mas necessário para simular o SAP real)

2. **CORS Habilitado**: Permite requisições de qualquer origem (útil para testes com frontend)

3. **Sem Autenticação Real**: O mock aceita qualquer credencial para facilitar testes. Requer apenas `ClientNumber` + (`Cpf` OU `Cnpj`)

4. **PDF Válido**: O PDF retornado para DOC002 é um PDF mínimo válido em base64 que pode ser aberto

5. **Stateless**: O servidor não mantém estado entre requisições

6. **Formato camelCase**: Todas as respostas JSON usam propriedades em camelCase para corresponder exatamente aos atributos `JsonPropertyName` dos DTOs (exemplo: `negativo` ao invés de `Negative`, `clientnumber` ao invés de `ClientNumber`, `duedate` ao invés de `DueDate`)

## Troubleshooting

### Porta 8080 já está em uso

```bash
# Windows: Encontrar processo usando a porta
netstat -ano | findstr :8080

# Matar o processo
taskkill /PID {numero_do_pid} /F
```

### Aplicação não consegue conectar ao mock

Verifique:
1. Mock server está rodando (`http://localhost:8080`)
2. `appsettings.Development.json` aponta para `http://localhost:8080`
3. Firewall não está bloqueando a porta 8080
