namespace CodeWF.Data.Entities;

public class User
{
    public Guid Id { get; set; }

    public string UserName { get; set; }
    public string Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public EnabledKind UserStatus { get; set; }

    public GenderKind Gender { get; set; }

    public string? OpenId { get; set; }

    public string? Avatar { get; set; }

    public string? Admire { get; set; }

    public string? Subscribe { get; set; }

    public string? Introduction { get; set; }

    public UserTypeKind UserType { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime UpdateTime { get; set; }

    public string? UpdateBy { get; set; }

    public EnabledKind Deleted { get; set; }
}

public enum EnabledKind
{
    No = 0,
    Yes = 1
}

public enum GenderKind
{
    Male = 1,
    Female = 2,
    Unspecified = 0
}

public enum UserTypeKind
{
    Admin = 0,
    Manager = 1,
    Regular = 2
}