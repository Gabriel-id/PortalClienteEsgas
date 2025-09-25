# Portal Cliente Esgas

Sistema web para consulta de faturas e gestão de clientes de gás desenvolvido em .NET 8.

## Sobre o Projeto

Portal web que permite aos clientes da empresa de gás:

- Consultar faturas e histórico
- Download de PDFs das faturas
- Autenticação segura com CPF/CNPJ e número do cliente
- Interface responsiva para dispositivos móveis
- Integração em tempo real com sistema SAP

## Tecnologias

- **.NET 8** - Framework principal
- **ASP.NET Core MVC** - Interface web
- **Bootstrap 5** - Framework CSS responsivo
- **SAP ERP** - Integração para dados de faturas
- **Serilog** - Logging estruturado
- **FluentValidation** - Validação de dados

## Estrutura do Projeto

```
PortalClienteEsgas/
├── PortalCliente/                 # Aplicação MVC Web
├── PortalCliente.Core/            # Camada de Domínio (DTOs, Services, Validators)
└── PortalCliente.Infrastructure/  # Camada de Infraestrutura (SAP Integration)
```

## Instalação e Execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Acesso à rede do servidor SAP

### Comandos

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/PortalClienteEsgas.git
cd PortalClienteEsgas

# Restaure as dependências
dotnet restore

# Configure certificado SSL (primeira execução)
dotnet dev-certs https --trust

# Execute a aplicação
cd PortalCliente
dotnet run
```

**Acesse:** https://localhost:7187

### Build e Testes

```bash
# Build da solução
dotnet build PortalCliente.sln

# Executar testes
dotnet test

# Build de produção
dotnet publish -c Release
```

## Configuração

### appsettings.json

```json
{
  "SapService": {
    "BaseUrl": "http://srv-sap-prd.esgas.com.br:8000/sap/bc/inbound/",
    "Username": "Username",
    "Password": "Password",
    "TimeoutSeconds": 30,
    "Endpoints": {
      "Authentication": "DATAGAS003",
      "GetInvoices": "DATAGAS004",
      "GetInvoiceContent": "DATAGAS005"
    }
  }
}
```

### Variáveis de Ambiente (Produção)

```bash
ASPNETCORE_ENVIRONMENT=Production
SAP_SERVICE_USERNAME=your_username
SAP_SERVICE_PASSWORD=your_password
```

## Como Usar

1. Acesse https://localhost:7187
2. Faça login com:
   - **Username**: CPF (11 dígitos) ou CNPJ (14 dígitos)
   - **Password**: Número do cliente
3. Visualize suas faturas na dashboard
4. Clique em "Download" para baixar o PDF da fatura

## Arquitetura

O projeto segue uma arquitetura em camadas:

- **Presentation Layer** (PortalCliente): Controllers MVC e Views
- **Core Layer** (PortalCliente.Core): Lógica de negócio e DTOs
- **Infrastructure Layer** (PortalCliente.Infrastructure): Integração SAP

## Publicação no IIS

### Pré-requisitos do Servidor

- Windows Server com IIS instalado
- .NET 8 Hosting Bundle instalado
- ASP.NET Core Module v2

### Passos para Publicação

1. **Build e Publish:**

```bash
dotnet publish PortalCliente/PortalCliente.csproj -c Release -o "C:\inetpub\wwwroot\PortalClienteEsgas"
```

2. **Configurar Application Pool:**

```powershell
# Criar Application Pool
New-WebAppPool -Name "PortalClienteEsgasAppPool"
Set-ItemProperty -Path "IIS:\AppPools\PortalClienteEsgasAppPool" -Name "managedRuntimeVersion" -Value ""
```

3. **Configurar Site:**

```powershell
# Criar site no IIS
New-Website -Name "PortalClienteEsgas" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\PortalClienteEsgas" -ApplicationPool "PortalClienteEsgasAppPool"
```

4. **Configurar Permissões:**

```powershell
# Dar permissões para IIS
icacls "C:\inetpub\wwwroot\PortalClienteEsgas" /grant "IIS_IUSRS:(OI)(CI)F" /T
icacls "C:\inetpub\wwwroot\PortalClienteEsgas" /grant "IIS AppPool\PortalClienteEsgasAppPool:(OI)(CI)F" /T
```

5. **Configurar Variável de Ambiente:**

Edite o `web.config` para definir o ambiente como Production:

```xml
<environmentVariables>
  <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
</environmentVariables>
```

### Troubleshooting

**Error 500.30 - Process startup failure:**

```bash
# Testar aplicação via command line
cd C:\inetpub\wwwroot\PortalClienteEsgas
dotnet PortalCliente.dll --environment=Production
```

**Reiniciar Application Pool:**

```powershell
Restart-WebAppPool -Name "PortalClienteEsgasAppPool"
```

## Logs

Os logs são escritos em:

- **Console** (desenvolvimento)
- **Arquivos** `logs/portal-cliente-{date}.txt` (produção)
