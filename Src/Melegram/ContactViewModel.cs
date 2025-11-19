using System.ComponentModel;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;

#nullable disable
namespace TelegramClient
{
    public class ContactViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }

        public string PeerId { get; set; }

        public string Status { get; set; }

        public string IconText { get; set; }

        public Brush ForegroundColor { get; set; }

        public Brush SubtleTextColor { get; set; }

        public Brush AccentColor { get; set; }

        public ContactViewModel(User user)
        {
            this.PeerId = user.id;
            this.Title = (user.fn + " " + user.ln).Trim();
            if (string.IsNullOrEmpty(this.Title))
                this.Title = user.name;
            if (string.IsNullOrEmpty(this.Title))
                this.Title = "User " + user.id;
            this.Status = "last seen recently";
            this.IconText = string.IsNullOrEmpty(this.Title) ? "U" : this.Title.Substring(0, 1).ToUpper();
            this.LoadThemeColors();
        }

        private void LoadThemeColors()
        {
            string theme = IsolatedStorageHelper.GetValue<string>("theme");
            if (string.IsNullOrEmpty(theme))
                theme = "dark";
                
            switch (theme)
            {
                case "light":
                    this.ForegroundColor = new SolidColorBrush(Colors.Black);
                    this.SubtleTextColor = new SolidColorBrush(Color.FromArgb(255, 102, 102, 102));
                    this.AccentColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                    break;
                case "blue":
                    this.ForegroundColor = new SolidColorBrush(Colors.White);
                    this.SubtleTextColor = new SolidColorBrush(Color.FromArgb(255, 136, 187, 232));
                    this.AccentColor = new SolidColorBrush(Color.FromArgb(255, 64, 196, 255));
                    break;
                default:
                    this.ForegroundColor = new SolidColorBrush(Colors.White);
                    this.SubtleTextColor = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
                    this.AccentColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
