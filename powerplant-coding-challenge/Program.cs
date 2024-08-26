using FluentValidation;
using MediatR;
using powerplant_coding_challenge.Features;
using powerplant_coding_challenge.Middlewares;
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

            // Register FluentValidation validators and MediatR
            builder.Services.AddValidatorsFromAssemblyContaining<ProductionPlanCommandValidator>();
            builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            builder.Services.AddTransient<ProductionPlanService>();
            builder.Services.AddTransient<ProductionPlanValidatorService>();

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<LoggingMiddleware>();
            app.UseMiddleware<ExceptionsMiddleware>();

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
