using ClinicalTrialApp.Common.Behaviors;
using ClinicalTrialApp.Data;
using ClinicalTrialApp.Endpoints;
using ClinicalTrialApp.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ClinicalTrialApp.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Clinical Trial API", Version = "v1" });
});

builder.Services.AddDbContext<ClinicalTrialDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddScoped<IJsonSchemaValidator, JsonSchemaValidator>();
builder.Services.AddScoped<IClinicalTrialService, ClinicalTrialService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors();

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinical Trial API V1");
    c.RoutePrefix = "";
});

app.UseSerilogRequestLogging();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapClinicalTrialEndpoints();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClinicalTrialDbContext>();
    db.Database.Migrate();
}

app.Run();

// Required for integration tests
public partial class Program
{ }