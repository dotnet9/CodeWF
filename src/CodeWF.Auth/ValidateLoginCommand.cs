using CodeWF.Auth.Options;
using CodeWF.Utils;
using MediatR;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace CodeWF.Auth;

public class ValidateLoginCommand : IRequest<bool>
{
    [Required]
    [Display(Name = "Account")]
    [MinLength(2), MaxLength(32)]
    [RegularExpression("[a-z0-9]+")]
    public string Account { get; set; }

    [Required]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    [MinLength(8), MaxLength(32)]
    [RegularExpression("^(?=.*[a-zA-Z])(?=.*[0-9])[A-Za-z0-9._~!@#$^&*]{8,}$")]
    public string Password { get; set; }
}

public class ValidateLoginCommandHandler(IOptions<EncryptionOption> encryptionOptions)
    : IRequestHandler<ValidateLoginCommand, bool>
{
    public Task<bool> Handle(ValidateLoginCommand request, CancellationToken ct)
    {
        var account = encryptionOptions.Value;

        if (account is null) return Task.FromResult(false);
        if (account.Account != request.Account) return Task.FromResult(false);

        var valid = account.PasswordHash == Helper.HashPassword(request.Password.Trim(), account.PasswordSalt);

        return Task.FromResult(valid);
    }
}