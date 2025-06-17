using Microsoft.OpenApi.Models;
using ExcelApi.Services;
using System.Reflection;
using System.Text;

// fix error encoding 1252 cho ExcelDataReader
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Add NewtonsoftJson support for more flexible JSON handling
builder.Services.AddControllers().AddNewtonsoftJson();

// Register ExcelProcessingService
builder.Services.AddScoped<IExcelProcessingService, ExcelProcessingService>();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Excel API",
        Version = "v1",
        Description = "API for processing Excel and CSV files."
    });

    // Enable XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();

    // Map IFormFile to binary format for Swagger UI
    c.MapType<IFormFile>(() => new OpenApiSchema 
    { 
        Type = "string", 
        Format = "binary" 
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Excel API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
