namespace Masa.Blazor.Docs.Examples.converter.date_converter;

public partial class DateConverterTool
{
    private string? _inputDateStr;
    private string? _inputDateType;
    private string? _jsLocaleDateString;
    private string? _iso8601;
    private string? _iso9075;
    private string? _rfc3339;
    private string? _rfc7231;
    private string? _unixTimestamp;
    private string? _timestamp;
    private string? _utcFormat;
    private string? _mongoObjectID;
    private string? _excelDateAndTime;

    public class Item(string label, string value)
    {
        public string Label { get; set; } = label;
        public string Value { get; set; } = value;
    }

    readonly List<Item> _items =
    [
        new Item("JS locale date string", "1"),
        new Item("ISO 8601", "2"),
        new Item("ISO 9075", "3"),
        new Item("RFC 3339", "4"),
        new Item("RFC 7231", "5"),
        new Item("Unix timestamp", "6"),
        new Item("Timestamp", "7"),
        new Item("UTC format", "8"),
        new Item("Mongo ObjectID", "9"),
        new Item("Excel date/time", "10")
    ];
}
