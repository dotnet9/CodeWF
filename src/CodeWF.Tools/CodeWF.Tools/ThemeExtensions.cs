namespace CodeWF.Tools;

public static class ThemeExtensions
{
	public static T LocateMaterialTheme<T>(this Application application) where T : NeumorphismTheme
	{
		var materialTheme = application.Styles.FirstOrDefault(style => style is T);
		if (materialTheme == null)
		{
			throw new InvalidOperationException(
				$"Cannot locate {nameof(T)} in Avalonia application styles. Be sure that you include MaterialTheme in your App.xaml in Application.Styles section");
		}

		return (T)materialTheme;
	}
}