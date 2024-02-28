namespace CodeWF.Data;

public partial class Seed
{
    private static IEnumerable<CategoryEntity> GetCategories(string assetDir)
    {
        var filePath = Path.Combine(assetDir, "site", "category.json");
        var dataList = ReadFromFile<CategoryEntity>(filePath).ToList();
        dataList.ForEach(item =>
        {
            item.Id = Guid.NewGuid();
            item.Note = item.DisplayName;
        });

        return dataList!;
    }

    private static IEnumerable<FriendLinkEntity> GetFriendLinks(string assetDir)
    {
        var filePath = Path.Combine(assetDir, "site", "FriendLink.json");
        var dataList = ReadFromFile<FriendLinkEntity>(filePath).ToList();
        dataList.ForEach(item => item.Id = Guid.NewGuid());
        return dataList;
    }

    private static IEnumerable<T> ReadFromFile<T>(string filePath) where T : class
    {
        if (!File.Exists(filePath))
        {
            throw new Exception($"Please config {filePath}");
        }

        var fileContent = File.ReadAllText(filePath);
        var dataList = JsonSerializer.Deserialize<IEnumerable<T>>(fileContent)?.ToList();
        if (dataList?.Any() != true)
        {
            throw new Exception($"Please config {filePath}");
        }

        return dataList;
    }
}