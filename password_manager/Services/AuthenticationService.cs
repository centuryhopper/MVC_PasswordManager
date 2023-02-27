// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Security.Cryptography;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using PasswordManager.Data;
// using PasswordManager.Models;
// using PasswordManager.Utils;

// namespace PasswordManager.Services;

// public class AuthenticationService : IAuthenticationService<UserModel>
// {
//     private readonly PasswordDbContext db;
//     private readonly IConfiguration _configuration;
//     private readonly ILogger<AuthenticationService> logger;
//     // private readonly UserManager<IdentityUser> userManager;
//     // private readonly SignInManager<IdentityUser> signInManager;

//     public AuthenticationService(PasswordDbContext db, IConfiguration configuration, ILogger<AuthenticationService> logger/*,UserManager<IdentityUser> userManager,
//     SignInManager<IdentityUser> signInManager*/)
//     {
//         this.logger = logger;
//         this.db = db;
//         this._configuration = configuration;
//         // this.userManager = userManager;
//         // this.signInManager = signInManager;
//     }

//     public async Task<int> Commit()
//     {
//         return await db.SaveChangesAsync();
//     }

//     public async Task<UserModel?> Delete(string userId)
//     {
//         try
//         {
//             // should never be null if we're only calling this when the user is signed in
//             var model = await db.UserTableEF.FirstOrDefaultAsync(m => m.userId == userId);

//             db.UserTableEF.Remove(model!);

//             await Commit();

//             logger.LogWarning($"model ({model}) has been deleted");

//             return model;

//         }
//         catch (Exception e)
//         {
//             logger.LogError("Account deletion failed :(. Couldn't find model.");
//             logger.LogError(e.Message);
//             return null!;
//         }
//     }

//     public async Task<UserModel?> Login(LoginViewModel loginViewModel)
//     {
//         try
//         {
//             var model = await db.UserTableEF.FirstOrDefaultAsync(m => m.username == loginViewModel.username);

//             if (model is null)
//             {
//                 throw new Exception("incorrect username or password");
//             }

//             // decrypt password from model and compare with argModel's
//             if (loginViewModel.password != decryptPassword(model))
//             {
//                 throw new Exception("incorrect username or password");
//             }

//             // check if token has expired. If so then generate a new one. Otherwise keep it
//             DateTime tokenExpires = DateTime.Parse(model.tokenExpires!);

//             // logger.LogWarning($"{model}");
//             // logger.LogWarning($"{DateTime.Now}");
//             // logger.LogWarning($"{tokenExpires}");

//             // check for expiration and refresh if needed
//             if (DateTime.Compare(DateTime.Now, tokenExpires) > 0)
//             {
//                 var (jwt, created, expires) = TokenManager.createJwtToken(model, _configuration.GetSection("AppSettings:Token").Value!);
//                 model.currentJwtToken = jwt;
//                 model.tokenCreated = created.ToString();
//                 model.tokenExpires = expires.ToString();

//                 await Commit();
//             }

//             return model;
//         }
//         catch (Exception e)
//         {
//             logger.LogError(e.Message);
//             return null;
//         }
//     }

//     public async Task<UserModel?> Register(RegisterViewModel registerViewModel)
//     {
//         try
//         {
//             var userLst = await db.UserTableEF.ToListAsync();
//             int numUsers = userLst.Count;
//             logger.LogWarning($"# of users: {numUsers}");
//             logger.LogWarning($"{userLst.FirstOrDefault()?.aesIV}");

//             // make sure user is not in the database already
//             // userLst.ForEach(Console.WriteLine);
//             if (numUsers > 0)
//             {
//                 var existingModel = await db.UserTableEF.FirstOrDefaultAsync(user =>
//                      user.username == registerViewModel.username);

//                 if (existingModel is not null)
//                 {
//                     throw new Exception("This user already exists. Please try a different username.");
//                 }
//             }

//             var model = new UserModel
//             {
//                 userId = Guid.NewGuid().ToString(),
//                 username = registerViewModel.username,
//                 password = registerViewModel.password
//             };

//             using (Aes myAes = Aes.Create())
//             {
//                 byte[] encrypted = SymmetricEncryptionHandler.EncryptStringToBytes_Aes(model.password!, myAes.Key, myAes.IV);

//                 model.password = Convert.ToBase64String(encrypted);
//                 model.aesKey = Convert.ToBase64String(myAes.Key);
//                 model.aesIV = Convert.ToBase64String(myAes.IV);
//             }

//             var (jwt, created, expires) = TokenManager.createJwtToken(model, _configuration.GetSection("AppSettings:Token").Value!);
//             model.currentJwtToken = jwt;
//             model.tokenCreated = created.ToString();
//             model.tokenExpires = expires.ToString();

//             await db.UserTableEF.AddAsync(model);
//             await Commit();

//             return model;
//         }
//         catch (System.Exception e)
//         {
//             string msg = e.Message;
//             logger.LogError(e.Message);
//             return null;
//         }
//     }

//     private string decryptPassword(UserModel user)
//     {
//         return SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(Convert.FromBase64String(user.password!), Convert.FromBase64String(user.aesKey!), Convert.FromBase64String(user.aesIV!));
//     }


// }