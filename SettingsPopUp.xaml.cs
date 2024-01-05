using CommunityToolkit.Maui.Views;

namespace Wordle;

public partial class SettingsPopUp : Popup
{
    private AppSettings _viewModel = new AppSettings();
	public SettingsPopUp()
	{
		InitializeComponent();
        BindingContext = _viewModel;
	}

    private void hardMode_switch_Toggled(object sender, ToggledEventArgs e)
    {
        _viewModel.IsHardMode = e.Value;
    }

    private void darkMode_switch_Toggled(object sender, ToggledEventArgs e)
    {
        _viewModel.IsDarkMode = e.Value;
        ToggleTheme(e.Value);
    }
    private void ToggleTheme(bool isDarkMode)
    {
        var app = (App)Application.Current;

        if(isDarkMode)
        {
            app.Resources["BackgroundColor"] = Color.FromHex("#000000");
            app.Resources["TextColor"] = Color.FromHex("#FFFFFF");
        }
        else
        {
            app.Resources["BackgroundColor"] = Color.FromHex("#FFFFFF");
            app.Resources["TextColor"] = Color.FromHex("#000000");
        }
    }

    
}