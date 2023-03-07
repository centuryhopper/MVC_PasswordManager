using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using password_manager.Data;
using password_manager.Models;
using password_manager.Utils;

namespace password_manager.Services;

public class EFService : IDataAccess<AccountModel>
{
    private readonly PasswordDbContext db;
    private readonly ILogger<EFService> logger;

    public EFService(PasswordDbContext db, ILogger<EFService> logger)
    {
        this.logger = logger;
        this.db = db;
    }

    public async Task<int> Commit()
    {
        return await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<AccountModel>> Get()
    {
        try
        {
            var models = await (from acc in db.PasswordTableEF orderby acc.title select acc).ToListAsync();

            models.ForEach(model => model.password = SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(Convert.FromBase64String(model.password!), Convert.FromBase64String(model.aesKey!), Convert.FromBase64String(model.aesIV!)));

            logger.LogWarning("retrieved models from get request");

            return models.AsEnumerable();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return null!;
        }
    }

    // in the future, the userId will be obtained from the jwt that is received from the client
    /*
        client logs in,
        server sends jwt to client,
        client sends jwt back to server,
        for every CRUD action performed
    */
    public async Task<IEnumerable<AccountModel>> Get(string userId)
    {
        try
        {
            var samples = await db.PasswordTableEF.Select(acc => acc).ToListAsync();

            // get foreign key shadow property
            var models = samples.Where(acc => db.Entry(acc).Property<string?>("userId").CurrentValue == userId).OrderBy(acc => acc.title).ToList();

            models.ForEach(model => model.password = SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(Convert.FromBase64String(model.password!), Convert.FromBase64String(model.aesKey!), Convert.FromBase64String(model.aesIV!)));

            logger.LogWarning($"retrieved models from get request by id: {userId}");

            // logger.LogWarning();
            // db.ChangeTracker.Entries().ToList().ForEach((EntityEntry e) => {
            //     logger.LogWarning($"{e.State}");
            //     foreach (var prop in e.CurrentValues.Properties)
            //     {
            //         logger.LogWarning($"{prop.Name}, {e.CurrentValues[prop]}");
            //     }
            // });


            return models;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return null!;
        }
    }

    public async Task<IEnumerable<AccountModel>> FilterBy(string userId, string title)
    {
        try
        {
            List<AccountModel> models;
            if (string.IsNullOrEmpty(title))
            {
                models = await db.PasswordTableEF.Where(acc => acc.userId == userId).OrderBy(acc => acc.title).ToListAsync();
            }
            else
            {
                models = await db.PasswordTableEF.Where(acc => acc.userId == userId).Where(acc => acc.title!.ToLower() == title.ToLower()).OrderBy(acc => acc.title).ToListAsync();
            }

            // List<AccountModel> modelClones = new List<AccountModel>();

            models.ForEach(model => model.password = SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(Convert.FromBase64String(model.password!), Convert.FromBase64String(model.aesKey!), Convert.FromBase64String(model.aesIV!)));

            return models;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return null!;
        }
    }

    /// <summary>
    /// post an account into the db
    /// </summary>
    /// <param name="model">model to be entered</param>
    /// <param name="userId">the user id associated with this account</param>
    /// <returns></returns>
    public async Task<IResult> Post(AccountModel model, string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(model.title) || model.username.IsNullOrEmpty() || model.password.IsNullOrEmpty())
            {
                logger.LogWarning("one or more fields are null or empty");
                throw new Exception("not all fields have been properly assigned for your model");
            }

            encodeModelPassword(model);
            model.accountId = Guid.NewGuid().ToString();
            DateTime myDate = DateTime.ParseExact(
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                "yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture
            );
            model.insertedDateTime = model.lastModifiedDateTime = myDate.ToString();

            logger.LogWarning(userId);
            // db.Entry(model).Property<string?>("userId").CurrentValue = userId;
            model.userId = userId;


            logger.LogWarning($"{model}");
            await db.AddAsync(model);
            await Commit();
            logger.LogWarning($"model {model} was successfully added to db");
            return Results.Ok($"{model} was successfully added");
        }
        catch (System.Exception e)
        {
            logger.LogError(e.Message);
            return Results.BadRequest(e.Message);
        }
    }

    public async Task<AccountModel> Delete(string id)
    {
        try
        {
            var model = await db.PasswordTableEF.FindAsync(id);

            if (model is null)
            {
                throw new Exception();
            }

            db.PasswordTableEF.Remove(model);

            await Commit();

            logger.LogWarning($"model ({model}) has been deleted");

            return model;

        }
        catch (System.Exception e)
        {
            logger.LogError("Deletion failed :(. Couldn't find model.");
            logger.LogError(e.Message);
            return null!;
        }
    }

    public async Task<IResult> PostMany(List<AccountModel> models)
    {
        try
        {
            models.ForEach((model) =>
            {
                encodeModelPassword(model);
                model.accountId = Guid.NewGuid().ToString();
                DateTime myDate = DateTime.ParseExact(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    "yyyy-MM-dd HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture
                );
                model.insertedDateTime = model.lastModifiedDateTime = myDate.ToString();

                // shadow property
                var userId = db.Entry(model).Property<string?>("userId").CurrentValue;
                string userIdValueFromJWTThatWasSentBackFromClient = "";
                db.Entry(model).Property<string?>("userId").CurrentValue = userIdValueFromJWTThatWasSentBackFromClient;

                logger.LogWarning($"shadow property: {userId}");
            });
            await db.PasswordTableEF.AddRangeAsync(models);
            await Commit();
            logger.LogWarning("your accounts have been successfully added");
            return Results.Ok(models);
        }
        catch (Exception e)
        {
            logger.LogError("your accounts couldn't be added");
            return Results.BadRequest(e.Message);
        }
    }

    private void encodeModelPassword(AccountModel model)
    {
        if (model is null || model.title is null || model.username is null || model.password is null)
        {
            return;
        }

        using (Aes myAes = Aes.Create())
        {
            byte[] encrypted = SymmetricEncryptionHandler.EncryptStringToBytes_Aes(model.password, myAes.Key, myAes.IV);

            model.password = Convert.ToBase64String(encrypted);
            model.aesKey = Convert.ToBase64String(myAes.Key);
            model.aesIV = Convert.ToBase64String(myAes.IV);

            // System.Console.WriteLine($"encrypted password: {accountModel.password}");
            // System.Console.WriteLine($"encrypted key: {accountModel.aesKey}");
            // System.Console.WriteLine($"encrypted iv: {accountModel.aesIV}");

            // System.Console.WriteLine($"decrypted: {SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV)}");
        }
    }

    public async Task<IResult> Put(AccountModel argModel)
    {
        try
        {
            var model = await (from m in db.PasswordTableEF where m.accountId == argModel.accountId select m).FirstOrDefaultAsync();

            if (model is null)
            {
                throw new Exception("model was not found, and therefore cannot be updated :(");
            }

            if (!String.IsNullOrEmpty(model.password))
            {
                encodeModelPassword(argModel);
                model.password = argModel.password;
                model.aesIV = argModel.aesIV;
                model.aesKey = argModel.aesKey;
            }

            model.title = String.IsNullOrEmpty(argModel.title) ? model.title : argModel.title;
            model.username = String.IsNullOrEmpty(argModel.username) ? model.username : argModel.username;
            DateTime myDate = DateTime.ParseExact(
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                "yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture
            );
            model.lastModifiedDateTime = myDate.ToString();

            await Commit();

            logger.LogWarning("Updated model");

            return Results.Ok(model);
        }
        catch (Exception e)
        {
            logger.LogError("Unable to update model :(");
            return Results.BadRequest(e.Message);
        }
    }

    public async Task<AccountModel?> GetOne(string accountId)
    {
        try
        {
            var model = await db.PasswordTableEF.FirstOrDefaultAsync(m => m.accountId == accountId);

            byte[] passwordBytes = Convert.FromBase64String(model.password!);
            byte[] keyBytes = Convert.FromBase64String(model.aesKey!);
            byte[] ivBytes = Convert.FromBase64String(model.aesIV!);

            model!.password = SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(passwordBytes, keyBytes, ivBytes);

            return model;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return null!;
        }
    }

}
