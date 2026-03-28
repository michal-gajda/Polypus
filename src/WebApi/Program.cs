namespace Polypus.WebApi;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public sealed class Program
{
    private const string SERVICE_NAME = "Polypus Service";
    private const string SERVICE_NAMESPACE = "Polypus";

    private Program()
    {
    }

    public static async Task<int> Main(string[] args)
    {
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(serviceName: SERVICE_NAME, serviceVersion: "1.0.0")
            .AddAttributes(
            [
                new("service.namespace", SERVICE_NAMESPACE),
                new("service.instance.id", Environment.MachineName),
            ]);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        });

        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = SERVICE_NAME;
        });

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Configure(builder.Configuration.GetSection("Kestrel"));
        });

        builder.Logging.AddOpenTelemetry(options => options.SetResourceBuilder(resourceBuilder).AddOtlpExporter());

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(resourceBuilder)
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation(opt => opt.RecordException = true)
                .AddHttpClientInstrumentation(opt => opt.RecordException = true)
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(resourceBuilder)
                .AddProcessInstrumentation()
                .AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
                ;

        builder.Services.AddHealthChecks();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        var app = builder.Build();

        app.UseHealthChecks("/healthz");

        app.MapReverseProxy();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", $"{SERVICE_NAMESPACE} API v1");
            });
        }

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseAuthorization();

        app.MapControllers();

        app.MapFallbackToFile("index.html");

        await app.RunAsync();

        return 0;
    }
}
