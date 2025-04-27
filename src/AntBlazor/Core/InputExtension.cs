namespace AntBlazor.Core;

static class InputExtension
{
    public static void AddItem(this Dictionary<string, object> attributes, string key, bool value)
    {
        if (!value)
            return;

        attributes[key] = value;
    }

    public static void AddItem(this Dictionary<string, object> attributes, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        attributes[key] = value;
    }
}