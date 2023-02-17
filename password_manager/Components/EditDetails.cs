using Microsoft.AspNetCore.Mvc;
using PasswordManager.Models;
using PasswordManager.Services;

public class EditDetails : ViewComponent
{
    private readonly ILogger<EditDetails> logger;
    private readonly IDataAccess<AccountModel> dataAccess;

    public EditDetails(ILogger<EditDetails> logger, IDataAccess<AccountModel> dataAccess)
    {
        this.logger = logger;
        this.dataAccess = dataAccess;
    }

    public async Task<IViewComponentResult> InvokeAsync(string accountId, int idx)
    {
        logger.LogWarning($"accountId: {accountId}, idx:{idx}");
        var model = await dataAccess.GetOne(accountId);
        return View(new EditViewModel {accountModel = model!, editIdx = idx});
    }
}
