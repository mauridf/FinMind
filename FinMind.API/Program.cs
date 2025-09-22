using FinMind.API.Middleware;
using FinMind.Application;
using FinMind.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

// Middleware customizado para tratamento de erros
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();