using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wordle.ViewModels
{
    public class WordsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string FilePath => System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, "words.txt");

        private HttpClient httpClient;
        private List<string> words;
        private bool isBusy;

        public WordsViewModel()
        {
            httpClient = new HttpClient();
            words = new List<string>();
            //GetWordsCommand = new Command(async () => await MakeCollection());

            if(File.Exists(FilePath))
            {
                ReadFromFile();
            }
        }//constructor

        private async void ReadFromFile()
        {
            try
            {
                words = File.ReadAllLines(FilePath).ToList();
            }//try
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error reading file", ex.Message, "OK");
            }//catch
        }//ReadFromFile()

        private async Task GetWords()
        {
            words.Clear();
            var response = await httpClient.GetAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt");
            string content = await response.Content.ReadAsStringAsync();
            string[] individualWords = content.Split(new[] { '\n' });
            words.AddRange(individualWords);

            //write to file
            SaveWordsFile(content);

        }//GetWords()

        private async void SaveWordsFile(string data)
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

            }
            catch(Exception ex)
            {

            }
            finally
            {
                IsBusy = false;
            }
        }

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
        }

        public bool IsNotBusy => !IsBusy;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
