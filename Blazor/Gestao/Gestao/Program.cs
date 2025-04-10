// Importação dos namespaces necessários para a aplicação
using Blazored.LocalStorage;
using CensusFieldSurvey.DataBase;
using CensusFieldSurvey.DataBase.Repositories;
using CensusFieldSurvey.Model.EntitesBD;
using Coravel;
using Gestao.Client.Libraries.Notifications;
using Gestao.Components;
using Gestao.Components.Account;
using Gestao.Data;
using Gestao.Domain;
using Gestao.Domain.Repositories;
using Gestao.Libraries.Mail;
using Gestao.Libraries.Queues;
using Gestao.Libraries.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

// Cria uma nova instância do WebApplication builder para configurar a aplicação
var builder = WebApplication.CreateBuilder(args);

// Configura o Blazor para suportar componentes Server e WebAssembly
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()      // Habilita Blazor Server
    .AddInteractiveWebAssemblyComponents(); // Habilita Blazor WebAssembly

#region Configuração de Autenticação
// Configura o gerenciamento do estado de autenticação para o Blazor
builder.Services.AddCascadingAuthenticationState();

// Adiciona serviços para gerenciamento de identidade do usuário
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();

// Configura o provedor de estado de autenticação para autenticação persistente
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

// Configura esquemas de autenticação e provedores externos
builder.Services.AddAuthentication(options =>
{
    // Define os esquemas de autenticação padrão para a aplicação
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
// Provedores de autenticação externa (atualmente comentados)
// .AddGoogle(), .AddFacebook(), .AddMicrosoftAccount()
.AddIdentityCookies();
#endregion

#region Configuração do Banco de Dados e Autenticação
// Obtém a string de conexão do banco de dados da configuração
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("String de conexão 'DefaultConnection' não encontrada.");

// Configura o contexto do banco de dados PostgreSQL
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Adiciona página de erro de banco de dados para desenvolvimento
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configura ASP.NET Core Identity para autenticação de usuários
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
#endregion

#region Injeção de Dependência (Email, Repositórios, Bibliotecas Extras: LocalStorage, Fila)
// Adiciona suporte a armazenamento local para Blazor
builder.Services.AddBlazoredLocalStorage();
// Adiciona serviço de fila para tarefas em segundo plano
builder.Services.AddQueue();

// Configura tarefa agendada para transações financeiras
builder.Services.AddScoped<FinancialTransactionRepeatInvocable>();

// Configura serviço de email com configurações SMTP
builder.Services.AddSingleton<SmtpClient>(options =>
{
    var smtp = new SmtpClient();
    smtp.Host = builder.Configuration.GetValue<string>("EmailSender:Server")!;
    smtp.Port = builder.Configuration.GetValue<int>("EmailSender:Port");
    smtp.EnableSsl = builder.Configuration.GetValue<bool>("EmailSender:SSL");

    smtp.Credentials = new NetworkCredential(
        builder.Configuration.GetValue<string>("EmailSender:User"),
        builder.Configuration.GetValue<string>("EmailSender:Password")
    );

    return smtp;
});

// Registra vários serviços e repositórios
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();
builder.Services.AddSingleton<ICepService, CepService>();
builder.Services.AddScoped<CompanyOnSelectedNotification>();

// Registra implementações dos repositórios
builder.Services.AddScoped<IRepository<Account>, AccountRepository>();
builder.Services.AddScoped<IRepository<Category>, CategoryRepository>();
builder.Services.AddScoped<IRepository<Company>, CompanyRepository>();
builder.Services.AddScoped<IRepository<Document>, DocumentRepository>();
builder.Services.AddScoped<IRepository<FinancialTransaction>, FinancialTransactionRepository>();




#endregion

// Constrói a aplicação
var app = builder.Build();

// Configura o pipeline de middleware baseado no ambiente
if (app.Environment.IsDevelopment())
{
    // Habilita recursos de depuração para desenvolvimento
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    // Configura tratamento de erro e segurança para produção
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Configura middleware comum
app.UseHttpsRedirection();    // Redireciona HTTP para HTTPS
app.UseStaticFiles();         // Serve arquivos estáticos
app.UseAntiforgery();         // Habilita proteção antifalsificação

// Configura roteamento e componentes do Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Gestao.Client._Imports).Assembly);

// Mapeia endpoints relacionados à Identity
app.MapAdditionalIdentityEndpoints();

#region APIs Mínimas
// Obtém tamanho da página da configuração para paginação
int pageSize = builder.Configuration.GetValue<int>("Pagination:PageSize");

// Define endpoints da API para várias entidades
// Endpoint de Categorias - Obtém todas as categorias de uma empresa
app.MapGet("/api/categories", async (/*...*/) => { /*...*/ });

// Endpoint de Empresas - Obtém todas as empresas de um usuário
app.MapGet("/api/companies", async (/*...*/) => { /*...*/ });

// Endpoint de Contas - Obtém todas as contas de uma empresa
app.MapGet("/api/accounts", async (/*...*/) => { /*...*/ });

// Endpoint de Transações Financeiras - Obtém todas as transações de uma empresa
app.MapGet("/api/financialtransactions", async (/*...*/) => { /*...*/ });
#endregion

// Inicia a aplicação
app.Run();