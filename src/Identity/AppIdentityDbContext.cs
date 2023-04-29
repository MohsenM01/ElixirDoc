using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppIdentityDbContext : IdentityDbContext
{
    public AppIdentityDbContext
       (DbContextOptions<AppIdentityDbContext> options)
        : base(options)
    {
    }
}