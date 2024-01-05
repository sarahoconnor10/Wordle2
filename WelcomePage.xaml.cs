using CommunityToolkit.Maui.Views;

namespace Wordle;

public partial class WelcomePage : ContentPage
{
    SettingsPopUp settingsPage = new SettingsPopUp();
    StatsPopUp statsPage = new StatsPopUp();
    public WelcomePage()
    {
        InitializeComponent();
    }//WelcomePage constructor

    private async void PlayButton_Clicked(object sender, EventArgs e)
    {
        //redirect to main game page upon clicking
        await Shell.Current.GoToAsync("/MainPage");
    }//PlayButton_Clicked

    private async void SettingsButton_Clicked(object sender, EventArgs e)
    {
        //redirect to settings page upon clicking
        await this.ShowPopupAsync(settingsPage);
    }//SettingsButton_Clicked

    private async void StatsButton_Clicked(object sender, EventArgs e)
    {
        //redirect to statistics page upon clicking
        await this.ShowPopupAsync(statsPage);

    }//StatsButton_Clicked
}//WelcomePage Class