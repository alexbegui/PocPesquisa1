using CensusFieldSurvey.API;
using CensusFieldSurvey.DataBase;
using CensusFieldSurvey.Model.EntitesBD;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiEndpointsOptions>(builder.Configuration.GetSection("ApiEndpoints"));

var configuration = builder.Configuration;

// Configura o cliente HTTP para comunica��o com o servi�o SMTI.
builder.Services.AddHttpClient();

// Configura os controladores da API para usar System.Text.Json e ignora refer�ncias circulares durante a serializa��o JSON.
// Isso evita erros quando h� relacionamentos recursivos entre objetos.
builder.Services.AddControllers().AddJsonOptions(op => op.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Adiciona o suporte para a descoberta de endpoints da API.
builder.Services.AddEndpointsApiExplorer();

// Adicionando Swagger para documentar e testar a API
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API Bora Pesquisar", Version = "v1" });
    // Configura��es de seguran�a para JWT
    // c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    // {
    //     Description = "Informe o token desta forma: Bearer {seu token}",
    //     Name = "Authorization",
    //     In = ParameterLocation.Header,
    //     Type = SecuritySchemeType.ApiKey,
    //     Scheme = "Bearer"
    // });

    // c.AddSecurityRequirement(new OpenApiSecurityRequirement
    // {
    //     {
    //         new OpenApiSecurityScheme
    //         {
    //             Reference = new OpenApiReference
    //             {
    //                 Type = ReferenceType.SecurityScheme,
    //                 Id = "Bearer"
    //             },
    //             Scheme = "Bearer",
    //             Name = "Bearer",
    //             In = ParameterLocation.Header,
    //         },
    //         new List<string>()
    //     }
    // });
});

var ambienteConnectionString = configuration.GetValue<string>("ConnectionStrings:AmbienteConnectionStrings");
var connectionString = ambienteConnectionString switch
{
    "Local" => configuration.GetConnectionString("Banco_local"),
    "Homologacao" => configuration.GetConnectionString("Banco_homologacao"),
    "Producao" => configuration.GetConnectionString("Banco_producao"),
    _ => throw new InvalidOperationException("Ambiente de conex�o desconhecido.")
};

if (connectionString == null)
    throw new InvalidOperationException("Ambiente de conex�o desconhecido.");

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(connectionString)
);

// Reposit�rios para acesso aos dados.
// Registra AssetRepository e ParameterRepository como implementa��es de IRepository para InventoryEntry e Parameter, respectivamente.
// O escopo 'Scoped' garante que uma nova inst�ncia do reposit�rio seja criada para cada requisi��o HTTP.
builder.Services.AddScoped<IRepository<Research>, ResearchRepository>();
builder.Services.AddScoped<IResearchRepository, ResearchRepository>();

// Configura��o do CORS (Cross-Origin Resource Sharing) para permitir requisi��es de diferentes origens.
if (builder.Environment.IsDevelopment())
{
    // Configura��o para ambiente de desenvolvimento: permite qualquer origem, m�todo e cabe�alho.
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}
else
{
    // Configura��o para ambiente de produ��o:  permite apenas origens especificadas na configura��o 'AllowedOrigins'.
    // Garante mais seguran�a, restringindo o acesso � API apenas para dom�nios autorizados.
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

    if (allowedOrigins == null || allowedOrigins.Length == 0)
    {
        throw new InvalidOperationException("A configura��o 'AllowedOrigins' est� ausente ou vazia no arquivo de configura��o.");
    }

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

// Configura o AutoMapper para mapear objetos entre diferentes tipos.
builder.Services.AddAutoMapper(typeof(Program));

// Adicionando autoriza��o global
// builder.Services.AddAuthorization();

var app = builder.Build();

// Esse aqui � para interceptar as exce��es que tem para depois jogar no Swagger, para facilitar o problema.
//app.UseMiddleware<ErrorHandlingMiddleware>();

// Aplica a pol�tica CORS configurada.
//app.UseCors("CorsPolicy");

// Configura o pipeline de middleware HTTP.
// if (app.Environment.IsDevelopment())
// {

// }

// Em ambiente de desenvolvimento, usa as p�ginas de erro do desenvolvedor e o Swagger para auxiliar no desenvolvimento e teste da API.
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection(); // Redireciona requisi��es HTTP para HTTPS.
// app.UseAuthentication(); // Adiciona a autentica��o ao pipeline.
// app.UseAuthorization(); // Adiciona a autoriza��o ao pipeline.
app.MapControllers(); // Mapeia os controllers para os endpoints da API.
app.Run(); // Inicia a aplica��o.