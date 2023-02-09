using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using PasswordManager.Models;

namespace PasswordManager.Utils;

public static class TokenManager
{
    public static (string, DateTime, DateTime) createJwtToken(string userId, string tokenSecretKey)
    {
        var now = DateTime.Now;
        var expires = now.AddDays(1);
        byte[] key = Convert.FromBase64String(tokenSecretKey);
        var securityKey = new SymmetricSecurityKey(key);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new []
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                }
            ),
            Expires = expires,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
        return (handler.WriteToken(token), now, expires);


        // List<Claim> claims = new List<Claim>
        // {
        //     new Claim(ClaimTypes.NameIdentifier, pwm.username!),
        //     new Claim(ClaimTypes.Role, "Admin"),
        //     new Claim(JwtRegisteredClaimNames.Sub, pwm.userId!),
        //     new Claim(JwtRegisteredClaimNames.Exp, expires.ToString()),
        // };

        // var signingCreds = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
        //     tokenSecretKey)), SecurityAlgorithms.HmacSha512Signature);

        // var token = new JwtSecurityToken(
        //     claims: claims,
        //     expires: expires,
        //     signingCredentials: signingCreds);

        // var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        // return (jwt, now, expires);
    }

    // get username from the token
    public static string? ValidateToken(string token, string secret)
    {
        var principal = GetPrincipal(token, secret);
        if (principal is null)
        {
            return null;
        }

        ClaimsIdentity? identity = null;
        try
        {
            identity = (ClaimsIdentity?) principal.Identity;
        }
        catch
        {
            return null;
        }



        Claim userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier)!;
        return userIdClaim.Value;
    }

    private static ClaimsPrincipal? GetPrincipal(string token, string secret)
    {
        // var handler = new JwtSecurityTokenHandler();
        // var securityToken = handler.ReadToken(token) as SecurityToken;
        // var claimsPrincipal = handler.ValidateToken(token, new TokenValidationParameters(), out securityToken);

        // string userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value;

        // Console.WriteLine("User ID: " + userId);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = (JwtSecurityToken) tokenHandler.ReadToken(token);
            if (jwtToken is null)
            {
                return null;
            }

            byte[] key = Convert.FromBase64String(secret);
            var parameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
            SecurityToken securityToken;
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
            return principal;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}

