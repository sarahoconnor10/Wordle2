using CommunityToolkit.Maui.Views;
using Wordle.ViewModels;

namespace Wordle;

public partial class SettingsPopUp : Popup
{
    private WordsViewModel _viewModel = new WordsViewModel();
    bool isHard;
    bool isDark;


    public SettingsPopUp()
	{
		InitializeComponent();
        BindingContext = _viewModel;
        isHard = (bool)Application.Current.Resources["IsHardMode"];
        hardMode_switch.IsToggled = isHard;

    }//constructor

    private void darkMode_switch_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            Application.Current.Resources["IsDarkMode"] = e.Value;
            darkMode_switch.IsToggled = e.Value;
            ToggleTheme(e.Value);
            OnPropertyChanged(nameof(_viewModel.IsDarkMode));
        }
        catch (Exception ex)
        {
            Shell.Current.DisplayAlert("Error toggling dark mode", ex.Message, "OK");

        }
    }//darkMode_switch_Toggled

    private async void hardMode_switch_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            Application.Current.Resources["IsHardMode"] = e.Value;
            hardMode_switch.IsToggled = e.Value;
        }
        catch (Exception ex)
        {
            Shell.Current.DisplayAlert("Error toggling hard mode", ex.Message, "OK");
        }
    }//hardMode_switch_Toggled()
    private void ToggleTheme(bool isDarkMode)
    {
        var app = (App)Application.Current;

        if(isDarkMode)
        {
            app.Resources["BackgroundColor"] = Color.FromHex("#121212");
            app.Resources["TextColor"] = Color.FromHex("#FFFFFF");
        }//if darkmode
        else
        {
            app.Resources["BackgroundColor"] = Color.FromHex("#FFFFFF");
            app.Resources["TextColor"] = Color.FromHex("#000000");
        }//else
    }//ToggleTheme
}//class