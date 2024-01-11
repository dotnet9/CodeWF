namespace CodeWF.WebAPI.ViewModel.UserAdmin;

public record EditUserRequest(Guid Id, string RoleNames, string PhoneNumber);