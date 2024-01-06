namespace CodeWF.Desktop.ViewModels;

internal sealed class TranslationSlugViewModel : ViewModelBase
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

	public async Task HandleChineseToEnglishAsync()
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

	public async void HandleChineseToUrlSlug()
	{
		await HandleChineseToEnglishAsync();
		HandleEnglishToUrlSlug();
	}
}