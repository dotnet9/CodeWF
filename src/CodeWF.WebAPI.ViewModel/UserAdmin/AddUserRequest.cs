namespace CodeWF.WebAPI.ViewModel.UserAdmin;

public record AddUserRequest(string UserName, string RoleNames, string PhoneNumber);