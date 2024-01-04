using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Wordle;

public partial class SettingsPopUp : Popup
{
	public SettingsPopUp()
	{
		InitializeComponent();
	}

    private void hardMode_switch_Toggled(object sender, ToggledEventArgs e)
    {

    }

    private void darkMode_switch_Toggled(object sender, ToggledEventArgs e)
    {
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