using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Models;

namespace PasswordManager.Data;

public class PasswordDbContext : DbContext /*IdentityDbContext<IdentityUser>*/
{
    public PasswordDbContext(DbContextOptions<PasswordDbContext> options) : base(options)
    {

    }

    // increment the id of the model
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        // fluent api stuff would go in this method
        // modelBuilder.Entity<IdentityUserLogin<string>>()
        // .HasKey(l => new { l.LoginProvider, l.ProviderKey });

        // modelBuilder.Entity<IdentityUserRole<string>>()
        // .HasKey(r => new { r.UserId, r.RoleId });

        // TODO: add a foreign key not null constraint

        modelBuilder.Entity<AccountModel>()
        .Property<string>("userId");

        modelBuilder.Entity<AccountModel>()
            .HasOne(account => account.user)
            .WithMany(user => user.accounts)
            .HasForeignKey("userId")
            .IsRequired(true);

    }

    public DbSet<AccountModel> PasswordTableEF { get; set; }
    public DbSet<UserModel> UserTableEF { get; set; }

}



/*

CREATE TABLE "PasswordTableEF" (
          "accountId" text NOT NULL,
          "userId" text NOT NULL,
          title character varying(32) NOT NULL,
          username character varying(32) NOT NULL,
          password character varying(512) NOT NULL,
          "aesKey" text NULL,
          "aesIV" text NULL,
          "insertedDateTime" text NULL,
          "lastModifiedDateTime" text NULL,
          CONSTRAINT "PK_PasswordTableEF" PRIMARY KEY ("accountId"),
          CONSTRAINT "FK_PasswordTableEF_UserTableEF_userId" FOREIGN KEY ("userId") REFERENCES "UserTableEF" ("userId") ON DELETE CASCADE
);

CREATE TABLE "UserTableEF" (
    "userId" text NOT NULL,
    username character varying(32) NOT NULL,
    password character varying(512) NOT NULL,
    "aesKey" text NULL,
    "aesIV" text NULL,
    "currentJwtToken" text NOT NULL,
    "tokenCreated" text NULL,
    "tokenExpires" text NULL,
    CONSTRAINT "PK_UserTableEF" PRIMARY KEY ("userId")
);

*/