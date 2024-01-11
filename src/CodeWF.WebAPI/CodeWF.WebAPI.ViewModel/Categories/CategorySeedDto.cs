namespace CodeWF.WebAPI.ViewModel.Categories;

public record CategorySeedDto(int SequenceNumber, string Name, string Slug, string Cover, CategorySeedDto[]? Children);