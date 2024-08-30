using CodeWF.Data.Entities;
using CodeWF.EventBus;

namespace CodeWF.Core.Categories;

public class GetCategoriesQuery : Query<List<Category>>
{
    public override List<Category> Result { get; set; }
}