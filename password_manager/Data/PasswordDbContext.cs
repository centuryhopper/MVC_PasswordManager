using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Models;

namespace PasswordManager.Data;

public class PasswordDbContext : IdentityDbContext<ApplicationUser>
{
    public PasswordDbContext(DbContextOptions<PasswordDbContext> options) : base(options)
    {

    }

    // increment the id of the model
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // https://stackoverflow.com/questions/40703615/the-entity-type-identityuserloginstring-requires-a-primary-key-to-be-defined
        base.OnModelCreating(modelBuilder);

        // increment the id column for every newly added object
        // modelBuilder.Entity<AccountModel>()
        // .Property(x => x.accountId)
        // .UseSerialColumn();
        // modelBuilder.UseIdentityColumns();

        // identify the foreign key primary key relationship
        modelBuilder.Entity<AccountModel>()
            .HasOne(account => account.user)
            .WithMany(user => user.accounts)
            .HasForeignKey(nameof(AccountModel.userId))
            .IsRequired(true);
    }

    public DbSet<AccountModel> PasswordTableEF { get; set; }

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