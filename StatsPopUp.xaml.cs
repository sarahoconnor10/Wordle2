using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Wordle;

public partial class StatsPopUp : Popup, INotifyPropertyChanged
{
    private int _percentWon;
    private int _numWins;
    private int _streak;
    private int _gamesPlayed;


    public StatsPopUp()
	{
		InitializeComponent();
        BindingContext = this;
        UpdateStatistics();


    }
    public void UpdateStatistics()
    {
        GetDetails();
    }
    public int PercentWon
    {
        get => _percentWon;

        set
        {
            if(_percentWon != value)
            {
                _percentWon = value;
                OnPropertyChanged(nameof(PercentWon));
            }//if not equal to value
        }//set
    }//PercentWon

    public int NumWins
    {
        get => _numWins;
        set
        {
            if (_numWins != value)
            {
                _numWins = value;
                OnPropertyChanged(nameof(NumWins));
            }
        }
    }

    public int Streak
    {
        get => _streak;
        set
        {
            if (_streak != value)
            {
                _streak = value;
                OnPropertyChanged(nameof(Streak));
            }
        }
    }
    public int GamesPlayed
    {
        get => _gamesPlayed;
        set
        {
            if (_gamesPlayed != value)
            {
                _gamesPlayed = value;
                OnPropertyChanged(nameof(GamesPlayed));
            }
        }
    }
    public string SaveFilePath => System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "savefile.txt");

    public async Task GetDetails()
    {
        if (File.Exists(SaveFilePath))
        {
            try
            {
                //read in variables from save file
                using (StreamReader sr = new StreamReader(SaveFilePath))
                {
                    //number of wins
                    NumWins = int.Parse(sr.ReadLine());
                    //streak
                    Streak = int.Parse(sr.ReadLine());
                    //games played
                    GamesPlayed = int.Parse(sr.ReadLine());

                    //win percentage
                    if (GamesPlayed != 0)
                        PercentWon = (int)(((double)NumWins / GamesPlayed) * 100);
                    else
                        PercentWon = 0;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error reading details from file", ex.Message, "OK");

            }
        }
        else
        {
            NumWins = 0;
            PercentWon = 0;
            Streak = 0;
            GamesPlayed = 0;
        }//else no file


        Debug.WriteLine($"Percent won: {PercentWon}");
        Debug.WriteLine($"Num wins: {NumWins}");
        Debug.WriteLine($"Streak: {Streak}");
        Debug.WriteLine($"Games Played: {GamesPlayed}");



    }//


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}//class