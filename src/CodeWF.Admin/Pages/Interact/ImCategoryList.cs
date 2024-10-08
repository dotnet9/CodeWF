namespace CodeWF.Admin.Pages.Interact;

[Route("/ims/categories")]
public class ImCategoryList : CategoryList
{
    protected override ContentType Type => ContentType.Interact;
}