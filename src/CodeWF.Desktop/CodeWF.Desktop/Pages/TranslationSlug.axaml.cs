namespace CodeWF.Desktop.Pages;

public partial class TranslationSlug : UserControl
{
	public TranslationSlug()
	{
		InitializeComponent();
		DataContext = new TranslationSlugViewModel();
	}
}