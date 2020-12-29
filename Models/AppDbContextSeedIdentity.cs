using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Models;
using Models.User;
using Models.Users.Roles;
using streamingservice.Helper;

public static class AppDbContextSeedIdentity {
    public static void SeedUser(UserManager<UserModel> userManager , RoleManager<IdentityRole<int>> roleManager , AppDbContext appDbContext)
    {
        List<IdentityRole<int>> roles = new List<IdentityRole<int>>{
            new IdentityRole<int>{
                Id = 1,
                Name = Models.Users.Roles.Roles.Admin,
                NormalizedName = Models.Users.Roles.Roles.Admin.ToUpper()
            },
            new IdentityRole<int>{
                Id = 2,
                Name = Models.Users.Roles.Roles.User,
                NormalizedName = Models.Users.Roles.Roles.User.ToUpper()
            },
            new IdentityRole<int>{
                Id = 3,
                Name = Models.Users.Roles.Roles.Trial,
                NormalizedName = Models.Users.Roles.Roles.Trial.ToUpper()
            }
        };

        foreach (var role in roles)
        {
            if(roleManager.FindByNameAsync(role.Name).Result == null)
            {
                roleManager.CreateAsync(role).Wait();
            }
        }
        
        UserModel newAdmin = new UserModel{
            UserName = "admin",
            FirstName = "مدیرکل",
            LastName = "تست",
            ConfirmedAcc = true,
            MelliCode = "admin",
        };

        UserModel admin = userManager.FindByNameAsync("admin").Result;

        if(admin == null)
        {
            string token = TokenCreator.CreateToken("admin" , new List<string>{Roles.Admin});
            newAdmin.Token = token;
            
            userManager.CreateAsync(newAdmin , "Saleh-1379").Wait();
            userManager.AddToRoleAsync(newAdmin , Roles.Admin).Wait();
        }
        
    }
}