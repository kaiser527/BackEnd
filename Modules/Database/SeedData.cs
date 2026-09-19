using Microsoft.AspNetCore.Identity;
using BackEnd.Modules.User.Entities;

namespace BackEnd.Modules.Database
{
    public class SeedRoles
    {
        public static async Task Initialize(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = ["Admin", "User", "Staff"];
 
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }

    public class SeedUsers
    {
        public static async Task Initialize(UserManager<ApplicationUser> userManager)
        {
            // Seed Admin User
            var adminEmail = "admin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    Gender = "Other",
                    Image = "admin.png",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Use a secure password
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Default Normal User
            var userEmail = "user@gmail.com";
            var normalUser = await userManager.FindByEmailAsync(userEmail);

            if (normalUser == null)
            {
                normalUser = new ApplicationUser
                {
                    UserName = "user",
                    Email = userEmail,
                    FirstName = "Normal",
                    LastName = "User",
                    Gender = "Other",
                    Image = "user.png",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(normalUser, "User@123"); // Use a secure password
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                }
            }

            // Seed Default Staff User
            var staffEmail = "staff@gmail.com";
            var staffUser = await userManager.FindByEmailAsync(staffEmail);

            if (staffUser == null)
            {
                staffUser = new ApplicationUser
                {
                    UserName = "staff",
                    Email = staffEmail,
                    FirstName = "Staff",
                    LastName = "User",
                    Gender = "Other",
                    Image = "admin.png",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(staffUser, "Staff@123"); // Use a secure password
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, "Staff");
                }
            }
        }
    }
}
