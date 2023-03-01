
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PasswordManager.Utils;

public static class Helpers
{
    public static string GetErrors<T>(ModelStateDictionary ModelState, ILogger<T>? logger=null)
    {
        // Retrieve the list of errors
        var errors = ModelState.Values.SelectMany(v => v.Errors);
        if (logger is not null)
        {
            errors.ToList().ForEach(e => logger.LogWarning(e.ErrorMessage));
        }
        return string.Join("\n", errors.Select(e => e.ErrorMessage).ToList());
    }
}