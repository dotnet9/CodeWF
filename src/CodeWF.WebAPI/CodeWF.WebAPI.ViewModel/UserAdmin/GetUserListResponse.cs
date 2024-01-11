namespace CodeWF.WebAPI.ViewModel.UserAdmin;
public record GetUserListResponse(IEnumerable<UserDto>? Data, bool Success, int PageSize,
int Current);