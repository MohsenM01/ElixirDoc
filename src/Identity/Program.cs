using Identity.Extensions;
using Serilog;
using Serilog.Sinks.Elasticsearch;

// Log.Logger = new LoggerConfiguration()
//     .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
//     {
//         DetectElasticsearchVersion = true,
//         AutoRegisterTemplate = true,
//         AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8,
//         ModifyConnectionSettings = x => x.BasicAuthentication("elastic", "changeme"),
//         IndexFormat = "identity-{yyyy.MM.dd:0}"
//     })
//     .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder);

//builder.Logging.ClearProviders();

//builder.Host.UseSerilog();

var app = builder.Build();
app.ConfigureApplication();
app.RegisterEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
