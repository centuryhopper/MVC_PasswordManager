namespace password_manager.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string ErrorMessage { get; set; } = null!;
    public string ErrorTitle { get; set; } = null!;

}
