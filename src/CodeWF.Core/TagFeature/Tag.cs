namespace CodeWF.Core.TagFeature;

public class Tag
{
    public static readonly Expression<Func<TagEntity, Tag>> EntitySelector = t => new Tag
    {
        Id = t.Id, NormalizedName = t.NormalizedName, DisplayName = t.DisplayName
    };

    public int Id { get; set; }

    public string DisplayName { get; set; }

    public string NormalizedName { get; set; }

    public static bool ValidateName(string tagDisplayName)
    {
        if (string.IsNullOrWhiteSpace(tagDisplayName))
        {
            return false;
        }

        // Regex performance best practice
        // See https://docs.microsoft.com/en-us/dotnet/standard/base-types/best-practices

        const string pattern = @"^[a-zA-Z 0-9\.\-\+\#\s]*$";
        bool isEng = Regex.IsMatch(tagDisplayName, pattern);
        if (isEng)
        {
            return true;
        }

        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/character-classes-in-regular-expressions#supported-named-blocks
        const string chsPattern = @"\p{IsCJKUnifiedIdeographs}";
        bool isChs = Regex.IsMatch(tagDisplayName, chsPattern);

        return isChs;
    }

    public static string NormalizeName(string orgTagName, IDictionary<string, string> normalizations)
    {
        bool isEnglishName = Regex.IsMatch(orgTagName, @"^[a-zA-Z 0-9\.\-\+\#\s]*$");
        if (isEnglishName)
        {
            // special case
            if (orgTagName.Equals(".net", StringComparison.OrdinalIgnoreCase))
            {
                return "dot-net";
            }

            StringBuilder result = new StringBuilder(orgTagName);
            foreach ((string key, string value) in normalizations)
            {
                result.Replace(key, value);
            }

            return result.ToString().ToLower();
        }

        byte[] bytes = Encoding.Unicode.GetBytes(orgTagName);
        IEnumerable<string> hexArray = bytes.Select(b => $"{b:x2}");
        string hexName = string.Join('-', hexArray);

        return hexName;
    }
}