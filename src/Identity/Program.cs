using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("AppIdentityDbContext");

builder.Services.AddDbContext<AppIdentityDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

var app = builder.Build();
app.MapGet("/", () => "Hello World!");

app.Run();
