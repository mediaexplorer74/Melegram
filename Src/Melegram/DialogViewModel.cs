using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.UI.Core;

#nullable disable
namespace TelegramClient
{
    public class DialogViewModel : INotifyPropertyChanged
    {
        private BitmapImage _chatIcon;
        private CoreDispatcher _dispatcher;

        public string Title { get; set; }

        public string LastMessage { get; set; }

        public string PeerId { get; set; }

        public string Time { get; set; }

        public string IconText { get; set; }

        public Brush IconColor { get; set; }

        public int UnreadCount { get; set; }

        public Visibility UnreadVisibility { get; set; }

        public bool IsGroup { get; set; }

        public BitmapImage ChatIcon
        {
            get => this._chatIcon;
            set
            {
                this._chatIcon = value;
                this.OnPropertyChanged(nameof(ChatIcon));
                this.OnPropertyChanged("HasChatIcon");
            }
        }

        public bool HasChatIcon => this._chatIcon != null;

        public string MessageStatus { get; set; }

        public string StatusIcon { get; set; }

        public Brush StatusColor { get; set; }

        public Brush ForegroundColor { get; set; }

        public Brush SubtleTextColor { get; set; }

        public Brush AccentColor { get; set; }

        public DialogViewModel(
            Dialog dialog,
            Dictionary<string, User> users,
            Dictionary<string, Chat> chats)
        {
            this._dispatcher = Window.Current.Dispatcher;
            this.PeerId = dialog.id;
            this.UnreadCount = dialog.unread ?? 0;
            this.UnreadVisibility = this.UnreadCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            this.LoadThemeColors();
            this.SetMessageStatus(dialog);
            this.LoadChatIcon();
            
            if (users != null && users.ContainsKey(dialog.id))
            {
                User user = users[dialog.id];
                this.Title = (user.fn + " " + user.ln).Trim();
                if (string.IsNullOrEmpty(this.Title))
                    this.Title = user.name;
                if (string.IsNullOrEmpty(this.Title))
                    this.Title = "User " + dialog.id;
                this.IsGroup = false;
                this.IconText = string.IsNullOrEmpty(this.Title) ? "U" : this.Title.Substring(0, 1).ToUpper();
                this.IconColor = this.AccentColor;
                if (!string.IsNullOrEmpty(user.photo_url))
                    this.LoadUserProfileImage(user.photo_url);
            }
            else if (chats != null && chats.ContainsKey(dialog.id))
            {
                Chat chat = chats[dialog.id];
                this.Title = chat.t ?? chat.name;
                if (string.IsNullOrEmpty(this.Title))
                    this.Title = "Chat " + dialog.id;
                this.IsGroup = true;
                this.IconText = string.IsNullOrEmpty(this.Title) ? "G" : this.Title.Substring(0, 1).ToUpper();
                this.IconColor = this.AccentColor;
                this.LoadChatIcon();
            }
            else
            {
                this.Title = "Unknown " + dialog.id;
                this.IsGroup = false;
                this.IconText = "?";
                this.IconColor = new SolidColorBrush(Colors.Gray);
            }
            
            if (dialog.msg != null)
            {
                this.LastMessage = dialog.msg.text;
                if (string.IsNullOrEmpty(this.LastMessage))
                    this.LastMessage = "[Media]";
                if (dialog.msg.date <= 0)
                    return;
                this.Time = new DateTime(1970, 1, 1).AddSeconds((double)dialog.msg.date).ToString("HH:mm");
            }
            else
            {
                this.LastMessage = "No messages";
                this.Time = "";
            }
        }

        private void LoadChatIcon()
        {
            try
            {
                string photoUrl = "https://mp.nnchan.ru/ava.php?c=" + this.PeerId + "&p=r36";
                MpgramApiClient mpgramApiClient = new MpgramApiClient();
                string userToken = IsolatedStorageHelper.GetValue<string>("user_token");
                if (!string.IsNullOrEmpty(userToken))
                    mpgramApiClient.SetUserToken(userToken);
                
                mpgramApiClient.GetProfileImage(photoUrl, 
                    image => {
                        if (_dispatcher != null)
                        {
                            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                                this.ChatIcon = image;
                            });
                        }
                    }, 
                    ex => Debug.WriteLine("Failed to load chat icon: " + ex.Message));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading chat icon: " + ex.Message);
            }
        }

        private void LoadUserProfileImage(string photoUrl)
        {
            try
            {
                MpgramApiClient mpgramApiClient = new MpgramApiClient();
                string userToken = IsolatedStorageHelper.GetValue<string>("user_token");
                if (!string.IsNullOrEmpty(userToken))
                    mpgramApiClient.SetUserToken(userToken);
                
                mpgramApiClient.GetProfileImage(photoUrl, 
                    image => {
                        if (_dispatcher != null)
                        {
                            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                                this.ChatIcon = image;
                            });
                        }
                    }, 
                    ex => Debug.WriteLine("Failed to load profile image: " + ex.Message));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading profile image: " + ex.Message);
            }
        }

        private void SetMessageStatus(Dialog dialog)
        {
            this.MessageStatus = "delivered";
            switch (this.MessageStatus)
            {
                case "sent":
                    this.StatusIcon = "✓";
                    this.StatusColor = new SolidColorBrush(Colors.Gray);
                    break;
                case "delivered":
                    this.StatusIcon = "✓✓";
                    this.StatusColor = new SolidColorBrush(Colors.Gray);
                    break;
                case "read":
                    this.StatusIcon = "✓✓";
                    this.StatusColor = new SolidColorBrush(Colors.Blue);
                    break;
                case "failed":
                    this.StatusIcon = "✕";
                    this.StatusColor = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    this.StatusIcon = "◷";
                    this.StatusColor = new SolidColorBrush(Colors.LightGray);
                    break;
            }
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

        public void UpdateThemeColors(string theme)
        {
            switch (theme)
            {
                case "light":
                    this.IconColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                    break;
                case "blue":
                    this.IconColor = new SolidColorBrush(Color.FromArgb(255, 64, 196, 255));
                    break;
                default:
                    this.IconColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                    break;
            }
            this.OnPropertyChanged("IconColor");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
