using FluentValidation;
using Service.Interfaces;
using Service.Services;
using Service.Validators;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "OrderGenerator",
        Description = "API responsável pela geração de Ordens de Compra e Venda de ativos",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Dependencies
builder.Services.AddScoped<IOrderGeneratorService, OrderGeneratorService>();
builder.Services.AddValidatorsFromAssemblyContaining<OrderValidator>();

// Build
var app = builder.Build();

// Add Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
