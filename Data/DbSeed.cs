using AuthProject.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace AuthProject.Data
{

    public class DbSeed
    {
        private readonly ApplicationDbContext _dbContext;

        public DbSeed(ApplicationDbContext _DbContext)
        {
            _dbContext = _DbContext;
        }

        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {
                if (!_dbContext.Roles.Any())
                {
                    var roles = GetRoles();
                    _dbContext.Roles.AddRange(roles);
                    _dbContext.SaveChanges();
                }
            }
        }

        private IEnumerable<IdentityRole> GetRoles()
        {
            var roles = new List<IdentityRole>()
            {
                new IdentityRole()
                {
                    Name="User",
                    NormalizedName= "User".ToUpper(),
                },
                new IdentityRole()
                {
                    Name = "Admin",
                    NormalizedName= "Admin".ToUpper(),
                },

            };

            return roles;
        }
        public async  Task AddUsers(IApplicationBuilder applicationBuilder)
        {

            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                //Roles
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();



                //Users
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                string AdminUserEmail = "Admin@email.com";

                var AdminUser = await userManager.FindByEmailAsync(AdminUserEmail);
                if (AdminUser == null)
                {
                    var newAdminUser = new AppUser()
                    {
                        UserName = "AdminTest",
                        Email = AdminUserEmail,
                        EmailConfirmed = true,
                        FirstLogin = true,

                    };
                    await userManager.CreateAsync(newAdminUser, "TestPassword12#$");
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }

                string appUserEmail = "user@email.com";

                var appUser = await userManager.FindByEmailAsync(appUserEmail);
                if (appUser == null)
                {
                    var newAppUser = new AppUser()
                    {
                        UserName = "UserTest",
                        Email = appUserEmail,
                        EmailConfirmed = true,
                        FirstLogin = true,
                    };
                    await userManager.CreateAsync(newAppUser, "TestPassword12#$");
                    await userManager.AddToRoleAsync(newAppUser, "User");
                }
            }
        }
    }
}

