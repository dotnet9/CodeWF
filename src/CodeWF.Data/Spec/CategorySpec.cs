namespace CodeWF.Data.Spec;

public class CategorySpec : BaseSpecification<CategoryEntity>
{
    public CategorySpec(string routeName) : base(c => c.RouteName == routeName)
    {
    }

    public CategorySpec(Guid id) : base(c => c.Id == id)
    {
    }
}