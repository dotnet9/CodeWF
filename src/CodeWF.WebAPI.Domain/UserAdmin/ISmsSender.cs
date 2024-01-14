namespace CodeWF.WebAPI.Domain.UserAdmin;

public interface ISmsSender
{
    public Task SendAsync(string phoneNum, params string[] args);
}