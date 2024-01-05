using CommunityToolkit.Maui.Views;
using System.Data.Common;
using System.Diagnostics;

namespace Wordle;

/*
 * 04/01/24 current tasks:
 *  save details to file(settings and stats)
 *  add in references
 *  add github link (explain issues with previous repositories?)
 *  dynamic sizing depending on device
 *  changing keyboard colours
 */

public partial class MainPage : ContentPage
{
    private List<Label> addedLabels = new List<Label>();
    
    private List<string> guesses = new List<string>();
    private Random random = new Random();
    private List<string> words = new List<string>();
    private HttpClient httpClient = new HttpClient();
    private int letterCounter = 0, guessCounter = 0;
    private string guess;
    private int numWins;
    public bool gameRunning = false;
    private List<char> correctLettersGuessed = new List<char>();
    private List<char> wrongLettersGuessed = new List<char>();



    private string chosenWord { get; set; }
    public MainPage()
    {
        InitializeComponent();
       
        DrawGrid();

        GetWords();

        gameRunning = true;

    }//MainPage constructor

    private void DrawGrid()
    {
        for (int i = 0; i < 6; i++)
            GuessGrid.AddRowDefinition(new RowDefinition());

        for (int i = 0; i < 5; i++)
            GuessGrid.AddColumnDefinition(new ColumnDefinition());

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
            }//inner loop - col
        }//outer loop - row
    }//drawGrid()


    private async void GetWords()
    {
        var response = await httpClient.GetStringAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");
        string[] individualWords = response.Split(new[] { '\n' });
        words.AddRange(individualWords);
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
            if (isRowFull)
            {
                guess = guess.ToLower();
                guesses.Add(guess);
                checkWord();
                guessCounter++;
                letterCounter = 0;
                Debug.WriteLine(guess);
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

    private async void Win()
    {
        /*
        int row = guessCounter-1;
        int column = 0;

        Frame currentFrame = null;
        for (column = 0; column < 5; column++)
        {
            foreach (var child in GuessGrid.Children)
            {
                if (child is Frame && GuessGrid.GetRow(child) == row && GuessGrid.GetColumn(child) == column)
                {
                    currentFrame = (Frame)child;
                    await currentFrame.ScaleTo(1.5, 300);
                    // await Task.Delay(100);

                    break;
                }
            }//for each
        }*/ //ANIMATIONS ??? problems - letters weren't moving, only frame, each frame moving too many times etc etc

        numWins++;
        await DisplayAlert("Win!", "You Won!", "Ok");
        DisableKeyboard();
        gameRunning = false;
        await this.ShowPopupAsync(new StatsPopUp());


    }//Win()
    private async void Lose()
    {
        DisplayAnswer(chosenWord);
        DisableKeyboard();
        gameRunning = false;
        await this.ShowPopupAsync(new StatsPopUp());

    }//Lose()

    private void SaveDetails()
    {
        //write details to a file
            //number of wins
            //past words -
                // win or lose
                // num of guesses taken
                // correct word
    }//SaveDetails
    
    private void RestartGame()
    {
        //clear grid
        //redraw grid
        //reset variables
        //enable keyboard
        EnableKeyboard();
        gameRunning = true;
    }//RestartGame

    private async void DisplayAnswer(string answer)
    {
        await DisplayAlert("Answer", $"The correct word was: {answer}", "OK");
    }//display answer
    
    private async void GoToSettings(object sender, EventArgs e)
    {
        if(!gameRunning)
            await this.ShowPopupAsync(new SettingsPopUp());

    }//GoToSettings
    private async void GoToStats(object sender, EventArgs e)
    {
       await this.ShowPopupAsync(new StatsPopUp());
    }//GoToStats

    private void DisableKeyboard()
    {
        a_key.IsEnabled = false;
        b_key.IsEnabled = false;
        c_key.IsEnabled = false;
        d_key.IsEnabled = false;
        e_key.IsEnabled = false;
        f_key.IsEnabled = false;
        g_key.IsEnabled = false;
        h_key.IsEnabled = false;
        i_key.IsEnabled = false;
        j_key.IsEnabled = false;
        k_key.IsEnabled = false;
        l_key.IsEnabled = false;
        m_key.IsEnabled = false;
        n_key.IsEnabled = false;
        o_key.IsEnabled = false;
        p_key.IsEnabled = false;
        q_key.IsEnabled = false;
        r_key.IsEnabled = false;
        s_key.IsEnabled = false;
        t_key.IsEnabled = false;
        u_key.IsEnabled = false;
        v_key.IsEnabled = false;
        w_key.IsEnabled = false;
        x_key.IsEnabled = false;
        y_key.IsEnabled = false;
        z_key.IsEnabled = false;
        back_btn.IsEnabled = false;
        enter_btn.IsEnabled = false;

    }//DisableKeyboard()

    private void EnableKeyboard()
    {
        a_key.IsEnabled = true;
        b_key.IsEnabled = true;
        c_key.IsEnabled = true;
        d_key.IsEnabled = true;
        e_key.IsEnabled = true;
        f_key.IsEnabled = true;
        g_key.IsEnabled = true;
        h_key.IsEnabled = true;
        i_key.IsEnabled = true;
        j_key.IsEnabled = true;
        k_key.IsEnabled = true;
        l_key.IsEnabled = true;
        m_key.IsEnabled = true;
        n_key.IsEnabled = true;
        o_key.IsEnabled = true;
        p_key.IsEnabled = true;
        q_key.IsEnabled = true;
        r_key.IsEnabled = true;
        s_key.IsEnabled = true;
        t_key.IsEnabled = true;
        u_key.IsEnabled = true;
        v_key.IsEnabled = true;
        w_key.IsEnabled = true;
        x_key.IsEnabled = true;
        y_key.IsEnabled = true;
        z_key.IsEnabled = true;
        back_btn.IsEnabled = true;
    }//EnableKeyboard

    private void ChangeKeyGreen(char key)
    {
        switch (key)
        {
            case 'a':
                a_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'b':
                b_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'c':
                c_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'd':
                d_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'e':
                e_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'f':
                f_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'g':
                g_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'h':
                h_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'i':
                i_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'j':
                j_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'k':
                k_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'l':
                l_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'm':
                m_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'n':
                n_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'o':
                o_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'p':
                p_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'q':
                q_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'r':
                r_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 's':
                s_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 't':
                t_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'u':
                u_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'v':
                v_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'w':
                w_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'x':
                x_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'y':
                y_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
            case 'z':
                z_key.BackgroundColor = (Color)Application.Current.Resources["WordleGreen"];
                break;
        }//switch
    }//turn key green

    private void ChangeKeyYellow(char key)
    {
        switch (key)
        {
            case 'a':
                a_key.BackgroundColor = Color.FromHex("#ffc425");
                break;
            case 'b':
                b_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'c':
                c_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'd':
                d_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'e':
                e_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'f':
                f_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'g':
                g_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'h':
                h_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'i':
                i_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'j':
                j_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'k':
                k_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'l':
                l_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'm':
                m_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'n':
                n_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'o':
                o_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'p':
                p_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'q':
                q_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'r':
                r_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 's':
                s_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 't':
                t_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'u':
                u_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'v':
                v_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'w':
                w_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'x':
                x_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'y':
                y_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
            case 'z':
                z_key.BackgroundColor = Color.FromHex("#ffc425"); ;
                break;
        }//switch
    }//turn key yellow
    private void ChangeKeyGrey(char key)
    {
        switch (key)
        {
            case 'a':
                a_key.BackgroundColor = Color.FromHex("#878686");
                break;
            case 'b':
                b_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'c':
                c_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'd':
                d_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'e':
                e_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'f':
                f_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'g':
                g_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'h':
                h_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'i':
                i_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'j':
                j_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'k':
                k_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'l':
                l_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'm':
                m_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'n':
                n_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'o':
                o_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'p':
                p_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'q':
                q_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'r':
                r_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 's':
                s_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 't':
                t_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'u':
                u_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'v':
                v_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'w':
                w_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'x':
                x_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'y':
                y_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
            case 'z':
                z_key.BackgroundColor = Color.FromHex("#878686"); ;
                break;
        }//switch
    }//turn key grey



}//class end

