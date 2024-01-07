using CommunityToolkit.Maui.Views;
using Wordle.ViewModels;

namespace Wordle;

public partial class SettingsPopUp : Popup
{
    //variables
    private WordsViewModel _viewModel = new WordsViewModel();
    bool isHard;
    public SettingsPopUp()
	{
        //constructor - assigns viewmodel, find value for hard mode and assigns it to the switch.
		InitializeComponent();
        BindingContext = _viewModel;
        isHard = (bool)Application.Current.Resources["IsHardMode"];
        hardMode_switch.IsToggled = isHard;

    }//constructor

    private void darkMode_switch_Toggled(object sender, ToggledEventArgs e)
    {
        //changes dark mode value and toggles theme.
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
        //changes hard mode value and toggles switch.
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
        //changes resource backgrounds depending on theme.
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