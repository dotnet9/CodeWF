namespace CodeWF.Core.PostFeature;

public struct PostSlug(int year, int month, int day, string slug)
{
    public int Year { get; set; } = year;
    public int Month { get; set; } = month;
    public int Day { get; set; } = day;
    public string Slug { get; set; } = slug;

    public override string ToString()
    {
        return $"{Year}/{Month}/{Day}/{Slug}";
    }
}