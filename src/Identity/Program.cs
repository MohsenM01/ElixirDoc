using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorizationBuilder()
.AddPolicy("admin_greetings", policy =>
 policy
    .RequireRole("admin")
    .RequireScope("greetings_api"));

//builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/hello", () => "Hello world!")
.RequireAuthorization("admin_greetings");

app.Run();



