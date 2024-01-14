namespace CodeWF.WebAPI.ViewModel.Auth;

public record ChangeMyPasswordRequest(string OldPassword, string NewPassword);