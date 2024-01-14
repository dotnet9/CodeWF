namespace CodeWF.Web.Service.Categories;

public interface ICategoryService
{
    Task<List<CategoryBrief>> CategoriesAsync();
    Task<List<CategoryBriefForMenu>?> CategoriesForMenuAsync();
}