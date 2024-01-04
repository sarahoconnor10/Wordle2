using System.Diagnostics;

namespace Wordle;

public partial class App : Application
{
    public App()
	{
		InitializeComponent();
		
		MainPage = new AppShell();
	}//constructor

}//class
