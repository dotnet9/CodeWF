using CodeWF.Models;

namespace CodeWF.Components.Pages;

using static Data.ActivityData;

public partial class Activities
{
    private int _page = 1;

    private int _year = -1;

    private ActivityType _type = ActivityType.None;

    private List<Item<int>> _yearItems = new()
    {
        new Item<int>("全部", -1),
        new Item<int>("2023", 2023),
        new Item<int>("2022", 2022)
    };

    private List<Item<ActivityType>> _activityTypeItems = new()
    {
        new Item<ActivityType>("全部", ActivityType.None),
        new Item<ActivityType>("社区例会", ActivityType.CommunityMeeting),
        new Item<ActivityType>("产品发布会", ActivityType.ProductLaunching),
        new Item<ActivityType>("培训", ActivityType.Training),
        new Item<ActivityType>("线下聚会", ActivityType.Meetup),
    };

    private IEnumerable<Activity> FilteredActivities
        => AllActivities.Where(u => (_year == -1 || u.StartAt.Year == _year) && (_type == ActivityType.None || u.Type == _type))
                        .OrderByDescending(u => u.StartAt);
}

public class Item<T>
{
    public string Label { get; set; }

    public T Value { get; set; }

    public Item(string label, T value)
    {
        Label = label;
        Value = value;
    }
}
