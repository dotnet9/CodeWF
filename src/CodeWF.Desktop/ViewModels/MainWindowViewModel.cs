using Avalonia.Themes.Neumorphism.Controls;
using CodeWF.Core;
using ReactiveUI;
using Splat;
using System;

namespace CodeWF.Desktop.ViewModels;

/// <summary>
/// 中文转英文，英文转别名
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
	private readonly ITranslationService? _translationService = Locator.Current.GetService<ITranslationService>();
	private string? _chinese;

	/// <summary>
	/// 中文标题
	/// </summary>
	public string? Chinese
	{
		get => _chinese;
		set
		{
			if (value != _chinese)
			{
				this.RaiseAndSetIfChanged(ref _chinese, value);
			}
		}
	}

	private string? _english;

	/// <summary>
	/// 英文标题
	/// </summary>
	public string? English
	{
		get => _english;
		set
		{
			if (value != _english)
			{
				this.RaiseAndSetIfChanged(ref _english, value);
			}
		}
	}

	private string? _slug;

	/// <summary>
	/// 别名
	/// </summary>
	public string? Slug
	{
		get => _slug;
		set
		{
			if (value != _slug)
			{
				this.RaiseAndSetIfChanged(ref _slug, value);
			}
		}
	}

	public async void HandleChineseToEnglishAsync()
	{
		try
		{
			English = await _translationService!.ChineseToEnglishAsync(Chinese);
		}
		catch (Exception ex)
		{
			English = ex.Message;
			SnackbarHost.Post($"中译英异常，请联系作者：{ex.Message}");
		}
	}

	public async void HandleEnglishToChineseAsync()
	{
		try
		{
			Chinese = await _translationService!.EnglishToChineseAsync(English);
		}
		catch (Exception ex)
		{
			Chinese = ex.Message;
			SnackbarHost.Post($"英译中异常，请联系作者：{ex.Message}");
		}
	}

	public void HandleEnglishToUrlSlug()
	{
		try
		{
			Slug = _translationService!.EnglishToUrlSlug(English);
		}
		catch (Exception ex)
		{
			Slug = ex.Message;
			SnackbarHost.Post($"英转URL别名异常，请联系作者：{ex.Message}");
		}
	}

	public void HandleChineseToUrlSlug()
	{
		HandleChineseToEnglishAsync();
		HandleEnglishToUrlSlug();
	}
}