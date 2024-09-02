using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CodeWF.Utils;

public static class Helper
{
    public static string? AppVersion
    {
        get
        {
            var asm = Assembly.GetEntryAssembly();
            if (null == asm) return "N/A";

            // e.g. 11.2.0.0
            var fileVersion = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

            // e.g. 11.2-preview+e57ab0321ae44bd778c117646273a77123b6983f
            var version = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (string.IsNullOrWhiteSpace(version) || version.IndexOf('+') <= 0)
            {
                return version ?? fileVersion;
            }

            var gitHash = version[(version.IndexOf('+') + 1)..]; // e57ab0321ae44bd778c117646273a77123b6983f
            var prefix = version[..version.IndexOf('+')]; // 11.2-preview

            if (gitHash.Length <= 6) return version;

            // consider valid hash
            var gitHashShort = gitHash[..6];
            return !string.IsNullOrWhiteSpace(gitHashShort) ? $"{prefix} ({gitHashShort})" : fileVersion;
        }
    }

    public static string TryGetFullOSVersion()
    {
        var osVer = Environment.OSVersion;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return osVer.VersionString;

        try
        {
            var currentVersion = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            if (currentVersion != null)
            {
                var name = currentVersion.GetValue("ProductName", "Microsoft Windows NT");
                var ubr = currentVersion.GetValue("UBR", string.Empty).ToString();
                if (!string.IsNullOrWhiteSpace(ubr))
                {
                    return $"{name} {osVer.Version.Major}.{osVer.Version.Minor}.{osVer.Version.Build}.{ubr}";
                }
            }
        }
        catch
        {
            return osVer.VersionString;
        }

        return osVer.VersionString;
    }

    // https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-6.0
    // This is not secure, but better than nothing.
    public static string HashPassword(string clearPassword, string saltBase64)
    {
        var salt = Convert.FromBase64String(saltBase64);

        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: clearPassword!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return hashed;
    }
}