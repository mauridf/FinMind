using Microsoft.OpenApi.Models;
using System.Reflection;
using FinMind.API.Middleware;
using FinMind.Application;
using FinMind.Infrastructure;
using FinMind.Application.Interfaces.Services;
using FinMind.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FinMind API",
        Version = "v1",
        Description = "Sistema Inteligente de Gest�o Financeira Pessoal",
        Contact = new OpenApiContact
        {
            Name = "Suporte FinMind",
            Email = "suporte@finmind.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Configurar JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando o esquema Bearer. 
                      Exemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Incluir coment�rios XML da API
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();
});

// Configurar as camadas da aplica��o
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configurar CORS para o frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// INICIALIZA��O DO MONGODB (deve ser a primeira coisa)
try
{
    using var scope = app.Services.CreateScope();
    var mongoInitializer = scope.ServiceProvider.GetRequiredService<MongoDbInitializer>();
    await mongoInitializer.InitializeAsync();
    Console.WriteLine("MongoDB inicializado com sucesso!");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao inicializar MongoDB: {ex.Message}");
    throw;
}

// SEED AUTOM�TICO (apenas desenvolvimento)
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var seedService = scope.ServiceProvider.GetRequiredService<ISeedService>();
        await seedService.SeedDataAsync();
        Console.WriteLine("Seed autom�tico executado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no seed autom�tico: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinMind API v1");
        c.RoutePrefix = "swagger"; // Mudar para "swagger" (padr�o)
        c.DocumentTitle = "FinMind API Documentation";

        // Configura��es adicionais para melhor experi�ncia
        c.DefaultModelsExpandDepth(-1); // Ocultar schemas por padr�o
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
    });

    Console.WriteLine("Swagger dispon�vel em: https://localhost:7244/swagger");
}
else
{
    // Em produ��o, usar Swagger apenas se configurado
    var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger");
    if (enableSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinMind API v1");
            c.RoutePrefix = "swagger";
        });
    }
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

// Adicionar endpoint fallback para melhor mensagem de erro
app.MapFallback(async context =>
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Endpoint n�o encontrado. Acesse /swagger para documenta��o da API.");
});

Console.WriteLine("FinMind API iniciada com sucesso!");
Console.WriteLine($"URLs dispon�veis:");
Console.WriteLine($"   - API: https://localhost:7244");
Console.WriteLine($"   - Swagger: https://localhost:7244/swagger");
Console.WriteLine($"   - Health Check: https://localhost:7244/api/health");

app.Run();