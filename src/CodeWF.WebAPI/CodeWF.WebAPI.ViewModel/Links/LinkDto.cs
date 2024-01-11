﻿namespace CodeWF.WebAPI.ViewModel.Links;

public record LinkDto
{
    public Guid Id { get; set; }
    public int SequenceNumber { get; set; }
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string? Description { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LinkKind Kind { get; set; }
}