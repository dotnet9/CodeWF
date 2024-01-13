using HashidsNet;

namespace CodeWF.Commons;

public static class HashHelper
{
    private static string ToHashString(byte[] bytes)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString();
    }

    public static string ComputeSha256Hash(Stream stream)
    {
        using (var sha256Hash = SHA256.Create())
        {
            var bytes = sha256Hash.ComputeHash(stream);
            return ToHashString(bytes);
        }
    }

    public static string ComputeSha256Hash(string input)
    {
        using (var sha256Hash = SHA256.Create())
        {
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            return ToHashString(bytes);
        }
    }

    public static string ComputeMd5Hash(string input)
    {
        using (var md5Hash = MD5.Create())
        {
            var bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            return ToHashString(bytes);
        }
    }

    public static string ComputeMd5Hash(Stream input)
    {
        using (var md5Hash = MD5.Create())
        {
            var bytes = md5Hash.ComputeHash(input);
            return ToHashString(bytes);
        }
    }

    /// <summary>
    /// TODO 用于计算短ID是有Bug的，应该以ID计算，这种方式会重复
    /// </summary>
    /// <param name="sourceStr"></param>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string GetHashids(this string sourceStr, int number = 9)
    {
        var hashids = new Hashids(sourceStr);
        return hashids.EncodeLong(number);
    }
}