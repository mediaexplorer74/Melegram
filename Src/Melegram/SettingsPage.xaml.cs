using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

#nullable disable
namespace TelegramClient
{
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private SolidColorBrush _regularGroupColor;
        private SolidColorBrush _supergroupColor;
        private SolidColorBrush _channelColor;
        private DispatcherTimer _notificationTimer;
        private CoreDispatcher _dispatcher;

        public SettingsPage()
        {
            this.InitializeComponent();
            this._dispatcher = Window.Current.Dispatcher;
            this.Loaded += this.SettingsPage_Loaded;
            this.DataContext = this;
        }

        public string AppVersion { get; set; }

        public SolidColorBrush RegularGroupColor
        {
            get => this._regularGroupColor;
            set
            {
                this._regularGroupColor = value;
                this.OnPropertyChanged(nameof(RegularGroupColor));
            }
        }

        public SolidColorBrush SupergroupColor
        {
            get => this._supergroupColor;
            set
            {
                this._supergroupColor = value;
                this.OnPropertyChanged(nameof(SupergroupColor));
            }
        }

        public SolidColorBrush ChannelColor
        {
            get => this._channelColor;
            set
            {
                this._channelColor = value;
                this.OnPropertyChanged(nameof(ChannelColor));
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // ------------
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility
                = AppViewBackButtonVisibility.Visible;

            SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                // If we don't have proper parameters, go back
                if (Frame.CanGoBack)
                    Frame.GoBack();
                a.Handled = true;
            };

            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s, a) =>
                {
                    // If we don't have proper parameters, go back
                    if (Frame.CanGoBack)
                        Frame.GoBack();
                    a.Handled = true;
                };
            }
            // ------------           
           
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadSettings();
        }

        private void LoadSettings()
        {
            string theme = IsolatedStorageHelper.GetValue<string>("theme");
            if (string.IsNullOrEmpty(theme))
                theme = "dark";
                
            this.ThemeListBox.SelectionChanged -= this.ThemeSelected;
            switch (theme)
            {
                case "light":
                    this.ThemeListBox.SelectedItem = this.ThemeLight;
                    break;
                case "dark":
                    this.ThemeListBox.SelectedItem = this.ThemeDark;
                    break;
                case "blue":
                    this.ThemeListBox.SelectedItem = this.ThemeBlue;
                    break;
            }
            this.ThemeListBox.SelectionChanged += this.ThemeSelected;
            
            string colorString1 = IsolatedStorageHelper.GetValue<string>("regular_group_color");
            if (string.IsNullOrEmpty(colorString1))
                colorString1 = "#0078D7";
            this.RegularGroupColor = new SolidColorBrush(this.ParseColor(colorString1));
            
            string colorString2 = IsolatedStorageHelper.GetValue<string>("supergroup_color");
            if (string.IsNullOrEmpty(colorString2))
                colorString2 = "#4CAF50";
            this.SupergroupColor = new SolidColorBrush(this.ParseColor(colorString2));
            
            string colorString3 = IsolatedStorageHelper.GetValue<string>("channel_color");
            if (string.IsNullOrEmpty(colorString3))
                colorString3 = "#FF9800";
            this.ChannelColor = new SolidColorBrush(this.ParseColor(colorString3));
            
            bool? notificationsEnabled = IsolatedStorageHelper.GetValue<bool?>("notifications_enabled");
            this.EnableNotifications.IsChecked = !notificationsEnabled.HasValue || notificationsEnabled.Value;
            
            bool? soundEnabled = IsolatedStorageHelper.GetValue<bool?>("sound_enabled");
            this.SoundEnabled.IsChecked = !soundEnabled.HasValue || soundEnabled.Value;
            
            bool? vibrationEnabled = IsolatedStorageHelper.GetValue<bool?>("vibration_enabled");
            this.VibrationEnabled.IsChecked = !vibrationEnabled.HasValue || vibrationEnabled.Value;
            
            bool? enterToSend = IsolatedStorageHelper.GetValue<bool?>("enter_to_send");
            this.EnterToSend.IsChecked = !enterToSend.HasValue || enterToSend.Value;
            
            bool? autoDownload = IsolatedStorageHelper.GetValue<bool?>("auto_download");
            this.AutoDownload.IsChecked = !autoDownload.HasValue || autoDownload.Value;
            
            int? fontSize = IsolatedStorageHelper.GetValue<int?>("font_size");
            int fontSizeIndex = fontSize.HasValue ? fontSize.Value : 1;
            
            this.FontSizeListBox.SelectionChanged -= this.FontSizeSelected;
            if (fontSizeIndex < this.FontSizeListBox.Items.Count)
                this.FontSizeListBox.SelectedIndex = fontSizeIndex;
            this.FontSizeListBox.SelectionChanged += this.FontSizeSelected;
            
            this.AppVersion = "Telegram Client v1.0";
        }

        private Color ParseColor(string colorString)
        {
            try
            {
                if (colorString.StartsWith("#"))
                {
                    string hex = colorString.TrimStart('#');
                    byte a = 255;
                    byte r, g, b;
                    
                    if (hex.Length == 6)
                    {
                        r = Convert.ToByte(hex.Substring(0, 2), 16);
                        g = Convert.ToByte(hex.Substring(2, 2), 16);
                        b = Convert.ToByte(hex.Substring(4, 2), 16);
                    }
                    else
                    {
                        if (hex.Length != 8)
                            return Colors.Blue;
                        a = Convert.ToByte(hex.Substring(0, 2), 16);
                        r = Convert.ToByte(hex.Substring(2, 2), 16);
                        g = Convert.ToByte(hex.Substring(4, 2), 16);
                        b = Convert.ToByte(hex.Substring(6, 2), 16);
                    }
                    return Color.FromArgb(a, r, g, b);
                }
                
                switch (colorString.ToLower())
                {
                    case "blue":
                        return Colors.Blue;
                    case "red":
                        return Colors.Red;
                    case "green":
                        return Colors.Green;
                    case "purple":
                        return Colors.Purple;
                    case "orange":
                        return Colors.Orange;
                    case "yellow":
                        return Colors.Yellow;
                    default:
                        return Colors.Blue;
                }
            }
            catch
            {
                return Colors.Blue;
            }
        }

        private async void ThemeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0 || !(e.AddedItems[0] is ListBoxItem))
                return;
                
            ListBoxItem addedItem = e.AddedItems[0] as ListBoxItem;
            string theme = "dark";
            
            if (addedItem == this.ThemeLight)
            {
                theme = "light";
            }
            else if (addedItem == this.ThemeDark)
            {
                theme = "dark";
            }
            else if (addedItem == this.ThemeBlue)
            {
                theme = "blue";
            }
            
            IsolatedStorageHelper.SetValue<string>("theme", theme);
            await this.ApplyTheme(theme);
        }

        private async System.Threading.Tasks.Task ApplyTheme(string theme)
        {
            var dialog = new MessageDialog($"Theme changed to {theme}. Changes will apply when you restart the app or go back to main page.", "Theme");
            await dialog.ShowAsync();
            this.ApplyThemeToCurrentPage(theme);
        }

        private void ApplyThemeToCurrentPage(string theme)
        {
            try
            {
                switch (theme)
                {
                    case "light":
                        this.Background = new SolidColorBrush(Colors.White);
                        break;
                    case "dark":
                        this.Background = new SolidColorBrush(Color.FromArgb(255, 27, 27, 27));
                        break;
                    case "blue":
                        this.Background = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                        break;
                }
            }
            catch
            {
            }
        }

        private async void ChangeGroupColors_Click(object sender, RoutedEventArgs e)
        {
            string currentColors = this.GetColorString(this.RegularGroupColor) + "," 
                + this.GetColorString(this.SupergroupColor) + "," + this.GetColorString(this.ChannelColor);
            
            // For now, we'll show a message since InputDialog is not implemented
            var dialog = new MessageDialog(
                $"Current group colors: {currentColors}\n\nTo change colors, please edit the settings file directly.", "Group Colors");
            await dialog.ShowAsync();
        }

        private string GetColorString(SolidColorBrush brush)
        {
            byte r = brush.Color.R;
            string rHex = r.ToString("X2");
            byte g = brush.Color.G;
            string gHex = g.ToString("X2");
            byte b = brush.Color.B;
            string bHex = b.ToString("X2");
            return "#" + rHex + gHex + bHex;
        }

        private void NotificationsToggle_Changed(object sender, RoutedEventArgs e)
        {
            bool isEnabled = this.EnableNotifications.IsChecked.HasValue && this.EnableNotifications.IsChecked.Value;
            IsolatedStorageHelper.SetValue<bool>("notifications_enabled", isEnabled);
            
            if (isEnabled)
                this.StartLiveNotifications();
            else
                this.StopLiveNotifications();
        }

        private void SoundToggle_Changed(object sender, RoutedEventArgs e)
        {
            IsolatedStorageHelper.SetValue<bool>("sound_enabled", this.SoundEnabled.IsChecked.HasValue && this.SoundEnabled.IsChecked.Value);
        }

        private void VibrationToggle_Changed(object sender, RoutedEventArgs e)
        {
            IsolatedStorageHelper.SetValue<bool>("vibration_enabled", this.VibrationEnabled.IsChecked.HasValue && this.VibrationEnabled.IsChecked.Value);
        }

        private void EnterToSend_Changed(object sender, RoutedEventArgs e)
        {
            IsolatedStorageHelper.SetValue<bool>("enter_to_send", this.EnterToSend.IsChecked.HasValue && this.EnterToSend.IsChecked.Value);
        }

        private void AutoDownload_Changed(object sender, RoutedEventArgs e)
        {
            IsolatedStorageHelper.SetValue<bool>("auto_download", this.AutoDownload.IsChecked.HasValue && this.AutoDownload.IsChecked.Value);
        }

        private async void FontSizeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (this.FontSizeListBox.SelectedIndex < 0)
                return;
                
            IsolatedStorageHelper.SetValue<int>("font_size", this.FontSizeListBox.SelectedIndex);
            
            var dialog = new MessageDialog("Font size changed. Restart app to see changes.", "Font Size");
            await dialog.ShowAsync();
        }

        private async void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            var confirmDialog = new MessageDialog("Clear all cached data?", "Clear Cache");
            var confirmCommand = new UICommand("Yes");
            var cancelCommand = new UICommand("Cancel");
            confirmDialog.Commands.Add(confirmCommand);
            confirmDialog.Commands.Add(cancelCommand);
            
            var result = await confirmDialog.ShowAsync();
            if (result == confirmCommand)
            {
                IsolatedStorageHelper.RemoveValue("user_token");
                IsolatedStorageHelper.RemoveValue("my_user_id");
                IsolatedStorageHelper.RemoveValue("last_dialogs");
                
                var successDialog = new MessageDialog("Cache cleared!", "Success");
                await successDialog.ShowAsync();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void StartLiveNotifications()
        {
            if (this._notificationTimer != null)
                return;
                
            this._notificationTimer = new DispatcherTimer();
            this._notificationTimer.Interval = TimeSpan.FromSeconds(30);
            this._notificationTimer.Tick += (s, e) => this.CheckForNewMessages();
            this._notificationTimer.Start();
        }

        private void StopLiveNotifications()
        {
            if (this._notificationTimer == null)
                return;
                
            this._notificationTimer.Stop();
            this._notificationTimer = null;
        }

        private void CheckForNewMessages()
        {
            bool? notificationsEnabled = IsolatedStorageHelper.GetValue<bool?>("notifications_enabled");
            if (!notificationsEnabled.HasValue || !notificationsEnabled.Value
                || !string.IsNullOrEmpty(IsolatedStorageHelper.GetValue<string>("user_token")))
            {
                // TODO: Simulate checking for new messages
            }
        }

        private async void ShowNotification(int unreadCount)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                this.UpdateAppTile(unreadCount);
                this.ShowToastNotification(unreadCount);
            });
        }

        private void UpdateAppTile(int unreadCount)
        {
            // In UWP, we would use SecondaryTile or Notifications instead of ShellTile
            // This is a placeholder for now
            Debug.WriteLine($"Tile update: {unreadCount} unread messages");
        }

        private void ShowToastNotification(int unreadCount)
        {
            Debug.WriteLine($"Notification: {(unreadCount == 1 ? "1 new message" : $"{unreadCount} new messages")}");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
    }
}
