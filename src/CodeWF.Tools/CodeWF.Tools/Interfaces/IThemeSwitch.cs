namespace CodeWF.Tools.Interfaces;

public interface IThemeSwitch
{
	ApplicationTheme Current { get; }
	void ChangeTheme(ApplicationTheme theme);
}