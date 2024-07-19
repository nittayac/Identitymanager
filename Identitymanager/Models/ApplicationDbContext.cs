using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Identitymanager.Config;

namespace Identitymanager.Models;

public class ApplicationDbContext:IdentityDbContext<ApplicationUsers>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) 
        : base(option)
    {


    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<IdentityRole>().HasData(
            new { Id = Guid.NewGuid().ToString(),Name= RoleName.Admin,NormalizedName = RoleName.Admin.ToUpper()},
            new { Id = Guid.NewGuid().ToString(), Name = RoleName.Member, NormalizedName = RoleName.Member.ToUpper()}
       );
    }
}
