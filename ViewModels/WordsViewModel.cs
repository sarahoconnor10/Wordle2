using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wordle.ViewModels
{
    public class WordsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string FilePath => System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "words.txt");

        private HttpClient httpClient;
        private List<string> ListofWords;
        public List<string> Words => ListofWords;
        private bool isBusy;

        public WordsViewModel()
        {
            httpClient = new HttpClient();
            ListofWords = new List<string>();

            if(File.Exists(FilePath))
            {
                ReadFromFile();
            }//if file exists
        }//constructor

        private async Task ReadFromFile()
        {
            try
            {
                ListofWords = File.ReadAllLines(FilePath).ToList();
            }//try
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error reading file", ex.Message, "OK");
            }//catch
        }//ReadFromFile()

        private async Task GetWords()
        {
            ListofWords.Clear();
            var response = await httpClient.GetAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");
            string content = await response.Content.ReadAsStringAsync();
            string[] individualWords = content.Split(new[] { '\n' });
            ListofWords.AddRange(individualWords);

            //write to file
            SaveWordsFile(content);

        }//GetWords()

        private async Task SaveWordsFile(string data)
        {
            try
            {
                File.WriteAllText(FilePath, data);
            }//try
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error writing file", ex.Message, "OK");

            }//catch
        }//SaveWordsFile()

        public async Task MakeList()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;

                if (File.Exists(FilePath))
                     ReadFromFile(); 
                else
                    await GetWords();
            }//try
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error making list", ex.Message, "OK");
            }//catch
            finally
            {
                IsBusy = false;
            }//finally
        }//MakeList()

        public async Task GetWordsFromVM()
        {
            await MakeList();
        }//GetWordsFromVM

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy == value)
                    return;
                isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }//IsBusy

        public bool IsNotBusy => !IsBusy;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }//OnPropertyChanged()


        private bool _isHardMode;
        public bool IsHardMode
        {
            get { return _isHardMode; }
            set
            {
                if (_isHardMode != value)
                {
                    _isHardMode = value;
                    OnPropertyChanged(nameof(IsHardMode));
                }
            }//set
        }//isHardMode

        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get { return _isDarkMode; }
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged(nameof(IsDarkMode));
                }
            }//set
        }//isDarkMode

    }//class
}//namespace


