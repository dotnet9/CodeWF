namespace CodeWF.JWT;

public interface ITokenService
{
    string BuildToken(IEnumerable<Claim> claims, JWTOptions options);
}