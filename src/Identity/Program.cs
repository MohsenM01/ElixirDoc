
using Identity.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder);

var app = builder.Build();
app.ConfigureApplication();
app.RegisterEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
