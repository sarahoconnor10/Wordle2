using System.ComponentModel;

namespace Wordle
{
    public class AppSettings : INotifyPropertyChanged
    {
        private bool _isHardMode;
        private bool _isDarkMode;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }//onPropertyChanged
    }//class
}//namespace
