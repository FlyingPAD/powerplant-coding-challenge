using FluentValidation;
using MediatR;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Interfaces;
using powerplant_coding_challenge.Middleware;
using powerplant_coding_challenge.Services;
using Serilog;
using System.Reflection;

namespace powerplant_coding_challenge;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog.
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        try
        {
            Log.Information(" => Starting up the service");

            builder.Host.UseSerilog();

            // Enregistrer FluentValidation et MediatR
            builder.Services.AddValidatorsFromAssemblyContaining<ProductionPlanCommandValidator>();
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

            // Ajouter le ValidationBehavior au pipeline MediatR
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Enregistrer les services personnalisés
            builder.Services.AddScoped<ICostCalculator, CostCalculatorService>();
            builder.Services.AddScoped<IProductionCalculator, ProductionCalculatorService>();

            // Ajouter les contrôleurs
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<ExceptionHandler>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
