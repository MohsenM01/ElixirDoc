
using Identity.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder);

//var connectionString = builder.Configuration.GetConnectionString("AppIdentityDbContext");
//builder.Services.AddDbContext<AppIdentityDbContext>(o => o.UseSqlServer(connectionString));
//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//               .AddEntityFrameworkStores<AppIdentityDbContext>()
//                .AddDefaultTokenProviders();

var app = builder.Build();
app.ConfigureApplication();
app.RegisterEndpoints();

app.Run();
