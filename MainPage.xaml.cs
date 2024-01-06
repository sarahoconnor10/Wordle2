﻿
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using Wordle.ViewModels;
using System;
using System.IO;

namespace Wordle;

/*
 * 04/01/24 current tasks:
 *  add in references
 *  add github link (explain issues with previous repositories?)
 *  dynamic sizing depending on device
 *  ISSUE - 
 *  on welcome page -> clicking "settings" multiple times causing app to freeze
 */

public partial class MainPage : ContentPage
{
    private AppSettings _settingsViewModel;
    private WordsViewModel _wordsViewModel;
    SettingsPopUp settingsPage = new SettingsPopUp();
    StatsPopUp statsPage = new StatsPopUp();


    public List<Button> keys;
    private List<Label> addedLabels = new List<Label>();
    private List<Frame> addedFrames = new List<Frame>();
    private List<string> guesses = new List<string>();
    private List<string> words = new List<string>();
    private List<char> correctLettersGuessed = new List<char>();
    private List<char> wrongLettersGuessed = new List<char>();
    
    private Random random = new Random();
    private HttpClient httpClient = new HttpClient();

    public string SaveFilePath => System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "savefile.txt");
    private string guess;
    private int letterCounter = 0, guessCounter = 0;
    private int numWins, gamesPlayed, percentWon, streak;
    public bool gameRunning = false;
    bool isHardMode;
    bool validInput;
    bool gridDrawn = false;
    
    private string chosenWord { get; set; }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = _settingsViewModel;

        keys = new List<Button>
        {
            a_key, b_key, c_key, d_key, e_key, f_key, g_key, h_key, i_key, j_key,
            k_key, l_key, m_key, n_key, o_key, p_key, q_key, r_key, s_key, t_key,
            u_key, v_key, w_key, x_key, y_key, z_key, back_btn
        };


        _settingsViewModel = new AppSettings();
        _wordsViewModel = new WordsViewModel();
        isHardMode = _settingsViewModel.IsHardMode;
        _wordsViewModel.GetWordsFromVM();

        PlayGame();
    }//MainPage constructor

    public async void PlayGame()
    {

        GetDetails();
        RestartGame();
        GetWord();
    }//PlayGame()

    private void playAgain_btn_Clicked(object sender, EventArgs e)
    {
        PlayGame();
    }//playAgain

    private void DrawGrid()
    {
        if (!gridDrawn)
        {
            for (int i = 0; i < 6; i++)
                GuessGrid.AddRowDefinition(new RowDefinition());

            for (int i = 0; i < 5; i++)
                GuessGrid.AddColumnDefinition(new ColumnDefinition());
        }

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                Frame styledFrame = new Frame
                {
                    BackgroundColor = (Color)Application.Current.Resources["BackgroundColor"],
                    CornerRadius = 0,
                    HasShadow = false,
                    Padding = new Thickness(5),
                    BorderColor = (Color)Application.Current.Resources["TextColor"]
                };
                GuessGrid.Add(styledFrame, row, col);

                addedFrames.Add(styledFrame);
            }//inner loop - col
        }//outer loop - row
        gridDrawn = true;
    }//drawGrid()

    private async void GetWord()
    {
        words = _wordsViewModel.Words;
        chosenWord = PickWord();
        Debug.WriteLine(chosenWord);
    }//getWords

    public string PickWord()
    {
        if (words.Count > 0)
        {
            int randomNumber = random.Next(0, words.Count);
            return words[randomNumber];
        }//if list is populated / not empty
        else
        {
            return null;
        }//else list is empty
    }//pickword()

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (guessCounter < 6 && letterCounter < 5)
        {
            if (sender is Button button)
            {
                string text = button.Text;

                var label = new Label
                {
                    Text = text,
                    FontSize = 30,
                    TextColor = (Color)Application.Current.Resources["TextColor"],
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center

                };

                GuessGrid.Add(label, letterCounter, guessCounter);
                addedLabels.Add(label);
                letterCounter++;

                //animation - increase size of letter briefly on input
                await label.ScaleTo(1.2, 200);
                await Task.Delay(100);
                await label.ScaleTo(1, 200);

                if (letterCounter == 5)
                {
                    enter_btn.IsEnabled = true;
                    //enter_btn.BackgroundColor = (Color)Application.Current.Resources["EnabledButtonColor"];
                }
                else
                { 
                    enter_btn.IsEnabled = false;
                    //enter_btn.BackgroundColor = (Color)Application.Current.Resources["DisabledButtonColor"];
                }
            }
        }//if num guesses is less than 6
    }//keyboard button clicked

    private void Backspace_Clicked(object sender, EventArgs e)
    {
        if(letterCounter > 0)
        {
           var lastLabel = addedLabels[addedLabels.Count - 1];
            GuessGrid.Children.Remove(lastLabel);
            addedLabels.Remove(lastLabel);
            letterCounter--;
            if (letterCounter < 5)
            {
                enter_btn.IsEnabled = false;
                //enter_btn.BackgroundColor = (Color)Application.Current.Resources["EnabledButtonColor"];
            }
        }
        if(letterCounter < 0)
        {
            letterCounter = GuessGrid.ColumnDefinitions.Count - 1;
        }
    }//backspace_clicked

    private void Enter_Clicked(object sender, EventArgs e)
    {
            if (guessCounter < 6)
            {
                guess = "";

                int rowIndex = guessCounter;
                bool isRowFull = true;

                foreach (var child in GuessGrid.Children)
                {
                    if (GuessGrid.GetRow(child) == rowIndex && child is Label label)
                    {
                        if (string.IsNullOrEmpty(label.Text))
                        {
                            isRowFull = false;
                            break;
                        }
                        else
                        {
                            guess += label.Text;
                        }
                    }
                }
                guess = guess.ToLower();
                ValidWord();
                if(!validInput)
                {
                    DisplayAlert("Invalid Input", "Word not in word list", "Ok");
                }
                else
                {
                    if (isRowFull)
                    {
                        guesses.Add(guess);
                        checkWord();
                        guessCounter++;
                        letterCounter = 0;
                        Debug.WriteLine(guess);
                    }
                }
            }
    }//enter

    private async void checkWord()
    {
        enter_btn.IsEnabled = false;
  
        int row = guessCounter;


        List<char> chosenLetters = chosenWord.ToList();
        List<int> greenLetters = new List<int>();
        List<int> yellowLetters = new List<int>();

        
        for (int col = 0; col < 5; col++)
        {
            //first go through word looking for correct spots
            if (guess[col] == chosenWord[col])
            {
                //record green indexes
                greenLetters.Add(col);
                correctLettersGuessed.Add(guess[col]);
                chosenLetters.Remove(guess[col]);
            }//if letter == letter
        }//for - find green spaces
        for (int col = 0; col < 5; col++)
        {
            if (chosenLetters.Contains(guess[col]))
            {
                yellowLetters.Add(col);
                chosenLetters.Remove(guess[col]);
            }
        }//for yellow spaces
       
        for (int column = 0; column < 5; column++)
        {
            Frame currentFrame = null;

            foreach (var child in GuessGrid.Children)
            {
                if (child is Frame && GuessGrid.GetRow(child) == row && GuessGrid.GetColumn(child) == column)
                {
                    currentFrame = (Frame)child;
                    break;
                }
            }
            if (currentFrame != null)
            {
                
                await currentFrame.ScaleTo(1.2, 100);
                await Task.Delay(100);
                await currentFrame.ScaleTo(1, 100);

                if (greenLetters.Contains(column))
                {
                    //turn square green
                    currentFrame.BackgroundColor = Color.FromHex("#019a01");
                    ChangeKeyGreen(guess[column]);
                }
                else if (yellowLetters.Contains(column))
                {
                    //else turn square yellow
                    currentFrame.BackgroundColor = Color.FromHex("#ffc425");
                    if (!correctLettersGuessed.Contains(guess[column]))
                    {
                        ChangeKeyYellow(guess[column]);
                    }
                }
                else
                {
                    //turn square darker grey
                    currentFrame.BackgroundColor = Color.FromHex("#878686");
                    if (!correctLettersGuessed.Contains(guess[column]))
                    {
                        wrongLettersGuessed.Add(guess[column]);
                        ChangeKeyGrey(guess[column]);
                    }
                }
            }
                
        }//for 
        if(greenLetters.Count == 5) 
        {
            Win();
        }
        chosenLetters.Clear();
        greenLetters.Clear();
        yellowLetters.Clear();
        if (guessCounter == 6)
           Lose();
    }//check word

    private void ValidWord()
    {
        for (int i = 0; i < words.Count; i++)
        {
            if (guess.Equals(words[i]))
            {
                validInput = true;
                return;
            }
            else
                validInput = false;
        }
    }//ValidWord()

    private async void Win()
    {
        gamesPlayed++;
        playAgain_btn.IsVisible = true;
        numWins++;
        streak++;
        await DisplayAlert("Win!", "You Won!", "Ok");
        DisableKeyboard();
        gameRunning = false;
        SaveDetails();
        statsPage.UpdateStatistics();
        await this.ShowPopupAsync(new StatsPopUp());
    }//Win()
    private async void Lose()
    {
        gamesPlayed++;
        playAgain_btn.IsVisible = true;
        streak = 0;
        DisplayAnswer(chosenWord);
        DisableKeyboard();
        gameRunning = false;
        SaveDetails();
        statsPage.UpdateStatistics();
        await this.ShowPopupAsync(new StatsPopUp());

    }//Lose()

    private async Task SaveDetails()
    {
        try
        {
            //write details to a file
            using (StreamWriter sw = new StreamWriter(SaveFilePath))
            {
                //number of wins
                sw.WriteLine(numWins);
                //streak
                sw.WriteLine(streak);
                //games played
                sw.WriteLine(gamesPlayed);
            }//streamWriter
        }//try
        catch(Exception ex) 
        {
            await Shell.Current.DisplayAlert("Error saving details", ex.Message, "OK");
        }//catch
    }//SaveDetails

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
                    numWins = int.Parse(sr.ReadLine());
                    //streak
                    streak = int.Parse(sr.ReadLine());
                    //games played
                    gamesPlayed = int.Parse(sr.ReadLine());

                    //win percentage
                    if (gamesPlayed != 0)
                        percentWon = (int)(((double)numWins / gamesPlayed) * 100);
                    else
                        percentWon = 0;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error reading details from file", ex.Message, "OK");
            }
        }
        else
        {
            numWins = 0;
            streak = 0;
            gamesPlayed = 0;
        }//else no file
    }//
    
    public void RestartGame()
    {
        //clear grid
        ClearGrid();

        //redraw grid
        DrawGrid();

        //enable keyboard
        EnableKeyboard();
        ResetKeyColor();
        gameRunning = true;

        playAgain_btn.IsVisible = false;

    }//RestartGame
    private void ClearGrid()
    {
        foreach (var label in addedLabels)
        {
            GuessGrid.Children.Remove(label);
        }//remove labels

        foreach (var frame in addedFrames)
        {
            GuessGrid.Children.Remove(frame);
        }//removes frames

        //reset variables / clear lists
        addedLabels.Clear();
        addedFrames.Clear();
        guesses.Clear();
        correctLettersGuessed.Clear();
        wrongLettersGuessed.Clear();
        guess = "";
        letterCounter = 0;
        guessCounter = 0;
    }//ClearGrid();


    private async void DisplayAnswer(string answer)
    {
        await DisplayAlert("Answer", $"The correct word was: {answer}", "OK");
    }//display answer
    
    private async void GoToSettings(object sender, EventArgs e)
    {
        if(!gameRunning)
            await this.ShowPopupAsync(settingsPage);

    }//GoToSettings
    private async void GoToStats(object sender, EventArgs e)
    {
        statsPage.UpdateStatistics();
        await this.ShowPopupAsync(statsPage);
    }//GoToStats

    private void DisableKeyboard()
    {
        foreach (var button in keys)
        {
            button.IsEnabled = false;
        }//for each key
    }//DisableKeyboard()

    private void EnableKeyboard()
    {
        foreach(var button in keys)
        {
            button.IsEnabled = true;
        }//for each key
    }//EnableKeyboard

    private void ChangeKeyGreen(char key)
    {
        char lowerKey = char.ToLower(key);

        foreach (var button in keys)
        {
            if (button.Text != null && char.ToLower(button.Text[0]) == lowerKey)
            {
                button.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break; //break after finding button
            }//if correct key
        }//for each key
    }//ChangeKeyGreen()

    private void ChangeKeyYellow(char key)
    {
        char lowerKey = char.ToLower(key);

        foreach (var button in keys)
        {
            if (button.Text != null && char.ToLower(button.Text[0]) == lowerKey)
            {
                button.BackgroundColor = Color.FromHex("#ffc425");
                break; 
            }//if correct key
        }//for each key
    }//ChangeKeyYellow()
    private void ChangeKeyGrey(char key)
    {
        char lowerKey = char.ToLower(key);

        foreach (var button in keys)
        {
            if (button.Text != null && char.ToLower(button.Text[0]) == lowerKey)
            {
                button.BackgroundColor = Color.FromHex("#878686");
                break;
            }//if correct key
        }//for each key
        
        if (isHardMode)
        {
            foreach (var button in keys)
            {
                if (button.Text != null && char.ToLower(button.Text[0]) == lowerKey)
                {
                    button.IsEnabled = false;
                    break;
                }//if correct button
            }//for each key
        }//if hard mode is enabled
    }//ChangeKeyGrey()

    private void ResetKeyColor() 
    {
        foreach (var button in keys) 
        {
            button.BackgroundColor = Color.FromHex("#d6d4d4");
        }//for each key
    }//ResetKeyColor()

}//Class

