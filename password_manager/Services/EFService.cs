using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PasswordManager.Data;
using PasswordManager.Models;
using PasswordManager.Utils;

namespace PasswordManager.Services;

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

    public async Task<IResult> Get()
    {
        try
        {
            var models = await (from acc in db.PasswordTableEF orderby acc.title select acc).ToListAsync();

            models.ForEach(model => model.password = SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(Convert.FromBase64String(model.password!), Convert.FromBase64String(model.aesKey!), Convert.FromBase64String(model.aesIV!)));

            logger.LogInformation("retrieved models from get request");

            return Results.Ok(JsonConvert.SerializeObject(models, Formatting.Indented));
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return Results.BadRequest(e.Message);
        }
    }

    // in the future, the userId will be obtained from the jwt that is received from the client
    /*
        client logs in,
        server sends jwt to client,
        client sends jwt back to server,
        for every CRUD action performed
    */
    public async Task<IResult> Get(string userId)
    {
        try
        {
            var samples = await db.PasswordTableEF.Select(acc => acc).ToListAsync();

            // get foreign key shadow property
            var models = samples.Where(acc => db.Entry(acc).Property<string?>("userId").CurrentValue == userId).OrderBy(acc => acc.title).ToList();

            models.ForEach(model => model.password = SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(Convert.FromBase64String(model.password!), Convert.FromBase64String(model.aesKey!), Convert.FromBase64String(model.aesIV!)));

            logger.LogInformation("retrieved models from get request");

            // return Results.Ok(JsonConvert.SerializeObject(models, Formatting.Indented));
            return Results.Ok(models);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return Results.BadRequest(e.Message);
        }
    }

    public async Task<IResult> Post(AccountModel model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.title) || model.username.IsNullOrEmpty() || model.password.IsNullOrEmpty())
            {
                logger.LogInformation("one or more fields are null or empty");
                throw new Exception("not all fields have been properly assigned for your model");
            }


            processModelPassword(model);
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


            logger.LogInformation($"{model}");
            await db.AddAsync(model);
            await Commit();
            logger.LogInformation($"model {model} was successfully added to db");
            return Results.Ok($"{model} was successfully added");
        }
        catch (System.Exception e)
        {
            logger.LogError(e.Message);
            return Results.BadRequest(e.Message);
        }
    }

    // public async Task<IResult> Get(string id)
    // {
    //     try
    //     {
    //         // var model = await db.PasswordTableEF.FindAsync(id);
    //         logger.LogInformation($"table name: \"public\".\"{nameof(db.PasswordTableEF)}\"");

    //         var model = await db.PasswordTableEF.FromSqlRaw($"select * from \"public\".\"{nameof(db.PasswordTableEF)}\" where id = '{id}'").FirstOrDefaultAsync();

    //         if (model is null)
    //         {
    //             throw new Exception("Couldn't get by id because model is null");
    //         }

    //         logger.LogInformation($"{model}");

    //         model.password = SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(Convert.FromBase64String(model.password!), Convert.FromBase64String(model.aesKey!), Convert.FromBase64String(model.aesIV!));

    //         return Results.Ok(model);
    //     }
    //     catch (System.Exception e)
    //     {
    //         logger.LogError(e.Message);
    //         return Results.BadRequest(e.Message);
    //     }
    // }

    public async Task<IResult> Delete(string id)
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

            logger.LogInformation($"model ({model}) has been deleted");

            return Results.Ok(model);

        }
        catch (System.Exception e)
        {
            logger.LogError("Deletion failed :(. Couldn't find model.");
            return Results.BadRequest(e.Message);
        }

    }

    public async Task<IResult> PostMany(List<AccountModel> models)
    {
        try
        {
            models.ForEach((model) =>
            {
                processModelPassword(model);
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

                logger.LogInformation($"shadow property: {userId}");
            });
            await db.PasswordTableEF.AddRangeAsync(models);
            await Commit();
            logger.LogInformation("your accounts have been successfully added");
            return Results.Ok(models);
        }
        catch (Exception e)
        {
            logger.LogError("your accounts couldn't be added");
            return Results.BadRequest(e.Message);
        }
    }

    private void processModelPassword(AccountModel model)
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
                processModelPassword(argModel);
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

            logger.LogInformation("Updated model");

            return Results.Ok(model);
        }
        catch (Exception e)
        {
            logger.LogError("Unable to update model :(");
            return Results.BadRequest(e.Message);
        }
    }

    // public async Task<IResult> GetByTitle(string title)
    // {
    //     try
    //     {
    //         var model = await db.PasswordTableEF.FromSqlInterpolated($"select * from {nameof(db.PasswordTableEF)} where title = '{title}'").FirstOrDefaultAsync();

    //         return Results.Ok(model);
    //     }
    //     catch (System.Exception e)
    //     {
    //         logger.LogError(e.Message);
    //         return Results.BadRequest(e.Message);
    //     }
    // }



}