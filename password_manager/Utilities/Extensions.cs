using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Models;

namespace PasswordManager.Utils;

public static class Extensions
{
    public static void Seed(this ModelBuilder builder)
    {
        var pwd = "TestPassword123!";
        var passwordHasher = new PasswordHasher<ApplicationUser>();

        // Seed Roles
        var adminRole = new IdentityRole(Constants.ADMIN);
        adminRole.NormalizedName = adminRole.Name?.ToUpper();

        var memberRole = new IdentityRole(Constants.USER);
        memberRole.NormalizedName = memberRole.Name?.ToUpper();

        List<IdentityRole> roles = new List<IdentityRole>
        {
            adminRole,
            memberRole
        };

        builder.Entity<IdentityRole>().HasData(roles);

        // Seed Users
        var adminUser = new ApplicationUser
        {
            FirstName = "admin_first_name",
            LastName = "admin_last_name",
            UserName = Constants.ADMIN,
            Email = "boviner1990@gmail.com",
            EmailConfirmed = true,
        };

        adminUser.NormalizedUserName = adminUser.UserName.ToUpper();
        adminUser.NormalizedEmail = adminUser.Email.ToUpper();
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, pwd);

        var memberUser = new ApplicationUser
        {
            FirstName = "regular_user_first_name",
            LastName = "regular_user_last_name",
            UserName = Constants.USER,
            Email = "dummyreceiver66@gmail.com",
            EmailConfirmed = true,
        };

        memberUser.NormalizedUserName = memberUser.UserName.ToUpper();
        memberUser.NormalizedEmail = memberUser.Email.ToUpper();
        memberUser.PasswordHash = passwordHasher.HashPassword(memberUser, pwd);

        List<ApplicationUser> users = new List<ApplicationUser>
        {
            adminUser,
            memberUser,
        };

        builder.Entity<ApplicationUser>().HasData(users);

        // Seed UserRoles
        List<IdentityUserRole<string>> userRoles = new List<IdentityUserRole<string>>();

        users.ForEach(user =>
        {
            userRoles.Add(new IdentityUserRole<string>
            {
                UserId = user.Id,
                //! username must be a role name for this to work !!
                RoleId = roles.First(role => role.Name == user.UserName).Id
            });
        });

        builder.Entity<IdentityUserRole<string>>().HasData(userRoles);
    }
}