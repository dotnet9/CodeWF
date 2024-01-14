namespace CodeWF.WebAPI.Domain.UserAdmin;

public class Role : IdentityRole<Guid>
{
    public Role()
    {
        Id = Guid.NewGuid();
    }
}