using System.Security.Cryptography;
using password_manager.Utils;

namespace password_manager.tests;

// added password_manager.csproj reference to this csproj
// run "dotnet test" to see results
public class PasswordConversionTests
{

    [Theory]
    [InlineData("HelloThere1!")]
    [InlineData("Whatsupman")]
    [InlineData("ThisIsMyPassword")]
    [InlineData("PasswordIsThis")]
    [InlineData("ThereHello!1")]
    public void TestPasswords(string testPassword)
    {
        using Aes myAes = Aes.Create();
        byte[] encryptedPassword = SymmetricEncryptionHandler.EncryptStringToBytes_Aes(testPassword, myAes.Key, myAes.IV);

        var decryptedPassword = SymmetricEncryptionHandler.DecryptStringFromBytes_Aes(encryptedPassword, myAes.Key, myAes.IV);

        // Console.WriteLine(decryptedPassword);

        Assert.Equal(testPassword, decryptedPassword);
    }
}
