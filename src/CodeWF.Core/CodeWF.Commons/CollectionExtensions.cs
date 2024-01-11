namespace CodeWF.Commons;

public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this ICollection<T>? source)
    {
        return source == null || !source.Any();
    }
}