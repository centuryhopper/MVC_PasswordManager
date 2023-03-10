
--  resetting tables
DROP TABLE IF EXISTS "public"."AspNetRoles" CASCADE;
DROP TABLE IF EXISTS "public"."AspNetRoleClaims" CASCADE;
DROP TABLE IF EXISTS "public"."AspNetUserClaims" CASCADE;
DROP TABLE IF EXISTS "public"."AspNetUserLogins" CASCADE;
DROP TABLE IF EXISTS "public"."AspNetUserRoles" CASCADE;
DROP TABLE IF EXISTS "public"."AspNetUserTokens" CASCADE;
DROP TABLE IF EXISTS "public"."AspNetUsers" CASCADE;
DROP TABLE IF EXISTS "public"."__EFMigrationsHistory";
DROP TABLE IF EXISTS "public"."PasswordTableEF";


-- checking out user roles
SELECT * FROM "public"."AspNetUserRoles" "ur"
join "public"."AspNetRoles" "r" on "ur"."RoleId" = "r"."Id"
join "public"."AspNetUsers" "u" on "u"."Id" = "ur"."UserId"