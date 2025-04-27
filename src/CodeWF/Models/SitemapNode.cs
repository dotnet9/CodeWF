﻿namespace CodeWF.Models;

public class SitemapNode
{
    public string Url { get; set; } = null!;
    public DateTimeOffset LastModified { get; set; }
    public SitemapFrequency Frequency { get; set; }
    public double Priority { get; set; }
}

public enum SitemapFrequency
{
    Monthly,
    Daily
}