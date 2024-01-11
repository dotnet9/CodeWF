namespace CodeWF.WebAPI.ViewModel.Categories;

public record GetCategoryListResponse(IEnumerable<CategoryDto>? Data, long Total, bool Success, int PageSize,
    int Current);