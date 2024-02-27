namespace CodeWF.Web.Configuration;

public static class Encoder
{
    public static HtmlEncoder CodeWfHtmlEncoder => HtmlEncoder.Create(
        UnicodeRanges.BasicLatin,
        UnicodeRanges.CjkCompatibility,
        UnicodeRanges.CjkCompatibilityForms,
        UnicodeRanges.CjkCompatibilityIdeographs,
        UnicodeRanges.CjkRadicalsSupplement,
        UnicodeRanges.CjkStrokes,
        UnicodeRanges.CjkUnifiedIdeographs,
        UnicodeRanges.CjkUnifiedIdeographsExtensionA,
        UnicodeRanges.CjkSymbolsandPunctuation,
        UnicodeRanges.EnclosedCjkLettersandMonths,
        UnicodeRanges.MiscellaneousSymbols,
        UnicodeRanges.HalfwidthandFullwidthForms
    );
}