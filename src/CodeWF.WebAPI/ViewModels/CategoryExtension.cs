namespace CodeWF.WebAPI.ViewModels;

public static class CategoryExtension
{
    public static async Task<Dictionary<Guid, string>?> GetCategoryIdAndNames(this CodeWFDbContext dbContext,
        IMemoryCacheHelper cacheHelper)
    {
        async Task<Dictionary<Guid, string>?> GetIdAndNamesFromDb()
        {
            return await dbContext.Categories!.ToDictionaryAsync(category => category.Id, category => category.Name);
        }

        return await cacheHelper.GetOrCreateAsync("CategoryIDAndNames", async e => await GetIdAndNamesFromDb());
    }
}