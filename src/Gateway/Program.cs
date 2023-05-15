using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Caching.Distributed;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORSPolicy", builder => builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((hosts) => true));
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(options =>
 {
     options.Configuration = "localhost";
     options.InstanceName = "SampleInstance";
 });

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CORSPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseOcelot().Wait();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (IDistributedCache distributedCache) =>
{
    var item = await distributedCache.GetAsync("Key1");
    if (item != null)
        return JsonSerializer.Deserialize<List<WeatherForecast>>(item);
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToList();

    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(forecast));
    await distributedCache.SetAsync("Key1", bytes);
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}