namespace CodeWF.Admin.Services;

public interface ICommonService : IService
{
    //User
    Task<PagingResult<CmUser>> QueryUsersAsync(PagingCriteria criteria);

    //Category
    Task<List<CmCategory>> GetCategoriesAsync(string type);
    Task<Result> DeleteCategoriesAsync(List<CmCategory> models);
    Task<Result> SaveCategoryAsync(CmCategory model);
}

class CommonService(Context context) : ServiceBase(context), ICommonService
{
    //User
    public Task<PagingResult<CmUser>> QueryUsersAsync(PagingCriteria criteria)
    {
        return Database.QueryPageAsync<CmUser>(criteria);
    }

    //Category
    public Task<List<CmCategory>> GetCategoriesAsync(string type)
    {
        return Database.QueryListAsync<CmCategory>(d => d.CompNo == CurrentUser.CompNo && d.Type == type);
    }

    public async Task<Result> DeleteCategoriesAsync(List<CmCategory> models)
    {
        if (models == null || models.Count == 0)
            return Result.Error(Language.SelectOneAtLeast);

        foreach (var item in models)
        {
            if (item.Children != null && item.Children.Count > 0)
                return Result.Error($"{item.Name}存在子节点数据，不能删除！");
        }

        return await Database.TransactionAsync(Language.Delete, async db =>
        {
            foreach (var item in models)
            {
                await db.DeleteAsync(item);
            }
        });
    }

    public async Task<Result> SaveCategoryAsync(CmCategory model)
    {
        var vr = model.Validate(Context);
        if (!vr.IsValid)
            return vr;

        return await Database.TransactionAsync(Language.Save, async db =>
        {
            if (model.Name.Contains('，'))
            {
                var names = model.Name.Split('，');
                foreach (var item in names)
                {
                    await db.SaveAsync(new CmCategory
                    {
                        Type = model.Type,
                        ParentId = model.ParentId,
                        Code = item,
                        Name = item,
                        Enabled = true
                    });
                }
            }
            else
            {
                await db.SaveAsync(model);
            }
        }, model);
    }
}