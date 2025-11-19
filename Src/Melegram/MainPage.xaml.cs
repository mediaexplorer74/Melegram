using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.UI.Interop;

#nullable disable
namespace TelegramClient
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private MpgramApiClient _apiClient;
        private List<DialogViewModel> _chats;
        private List<ContactViewModel> _contacts;
        private DispatcherTimer _notificationTimer;
        private bool _notificationsEnabled = false;
        private int _lastTotalUnread = 0;
        private string _currentSearchText = "";
        private CoreDispatcher _dispatcher;

        public SolidColorBrush BackgroundColor { get; set; }
        public SolidColorBrush ForegroundColor { get; set; }
        public SolidColorBrush SubtleTextColor { get; set; }
        public SolidColorBrush AccentColor { get; set; }
        public SolidColorBrush ListBackgroundColor { get; set; }
        public SolidColorBrush ItemBackgroundColor { get; set; }
        public Color AppBarBackgroundColor { get; set; }
        public Color AppBarForegroundColor { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            this._dispatcher = Window.Current.Dispatcher;
            this._apiClient = new MpgramApiClient(null, _dispatcher);
            this._chats = new List<DialogViewModel>();
            this._contacts = new List<ContactViewModel>();
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // ------------
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility
                = AppViewBackButtonVisibility.Visible;

            SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                //Debug.WriteLine("Special Back button Requested");
                //if (WebViewControl.CanGoBack)
                //{
                //    WebViewControl.GoBack();
                    a.Handled = true;
                //}
            };

            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s, a) =>
                {
                    //Debug.WriteLine("Hardware Back button Requested");
                    //if (WebViewControl.CanGoBack)
                    //{
                    //    WebViewControl.GoBack();
                    //}
                    a.Handled = true;
                };
            }
            // ------------

            ApplyTheme();
            LoadData();
            InitializeLiveNotifications();
        }

        private async void LoadData()
        {
            string userToken = IsolatedStorageHelper.GetValue<string>("user_token");
            if (string.IsNullOrEmpty(userToken))
            {
                Frame.Navigate(typeof(WelcomePage));
            }
            else
            {
                this._apiClient.SetUserToken(userToken);
                await LoadChats();
                await LoadContacts();
            }
        }

        private async System.Threading.Tasks.Task LoadChats()
        {
            ShowChatsLoading();
            await System.Threading.Tasks.Task.Delay(100); // Small delay to show loading indicator
            
            try
            {
                _apiClient.GetDialogs(
                    response => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            HideChatsLoading();
                            _chats.Clear();
                            if (response != null && response.dialogs != null)
                            {
                                foreach (Dialog dialog in response.dialogs)
                                {
                                    _chats.Add(new DialogViewModel(dialog, response.users, response.chats));
                                }
                                if (_chats.Count > 0)
                                {
                                    ChatsListBox.ItemsSource = _chats;
                                    ChatsListBox.Visibility = Visibility.Visible;
                                    ChatsEmptyText.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    ChatsListBox.Visibility = Visibility.Collapsed;
                                    ChatsEmptyText.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                ChatsListBox.Visibility = Visibility.Collapsed;
                                ChatsEmptyText.Visibility = Visibility.Visible;
                                ChatsEmptyText.Text = "Failed to load chats";
                            }
                        });
                    },
                    ex => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            HideChatsLoading();
                            ChatsListBox.Visibility = Visibility.Collapsed;
                            ChatsEmptyText.Visibility = Visibility.Visible;
                            ChatsEmptyText.Text = "Failed to load chats: " + ex.Message;
                        });
                    }
                );
            }
            catch (Exception ex)
            {
                HideChatsLoading();
                ChatsEmptyText.Text = "Error loading chats: " + ex.Message;
                ChatsEmptyText.Visibility = Visibility.Visible;
            }
        }

        private async System.Threading.Tasks.Task LoadContacts()
        {
            ShowContactsLoading();
            await System.Threading.Tasks.Task.Delay(100); // Small delay to show loading indicator
            
            try
            {
                _apiClient.GetDialogs(
                    response => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            HideContactsLoading();
                            _contacts.Clear();
                            if (response != null && response.users != null)
                            {
                                foreach (KeyValuePair<string, User> userEntry in response.users)
                                {
                                    User user = userEntry.Value;
                                    if (!string.IsNullOrEmpty(user.fn) || !string.IsNullOrEmpty(user.name))
                                        _contacts.Add(new ContactViewModel(user));
                                }
                                if (_contacts.Count > 0)
                                {
                                    ContactsListBox.ItemsSource = _contacts;
                                    ContactsListBox.Visibility = Visibility.Visible;
                                    ContactsEmptyText.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    ContactsListBox.Visibility = Visibility.Collapsed;
                                    ContactsEmptyText.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                ContactsListBox.Visibility = Visibility.Collapsed;
                                ContactsEmptyText.Visibility = Visibility.Visible;
                                ContactsEmptyText.Text = "Failed to load contacts";
                            }
                        });
                    },
                    ex => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            HideContactsLoading();
                            ContactsListBox.Visibility = Visibility.Collapsed;
                            ContactsEmptyText.Visibility = Visibility.Visible;
                            ContactsEmptyText.Text = "Failed to load contacts: " + ex.Message;
                        });
                    }
                );
            }
            catch (Exception ex)
            {
                HideContactsLoading();
                ContactsEmptyText.Text = "Error loading contacts: " + ex.Message;
                ContactsEmptyText.Visibility = Visibility.Visible;
            }
        }

        private void ShowChatsLoading()
        {
            ChatsLoadingPanel.Visibility = Visibility.Visible;
            ChatsListBox.Visibility = Visibility.Collapsed;
            ChatsEmptyText.Visibility = Visibility.Collapsed;
        }

        private void HideChatsLoading()
        {
            ChatsLoadingPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowContactsLoading()
        {
            ContactsLoadingPanel.Visibility = Visibility.Visible;
            ContactsListBox.Visibility = Visibility.Collapsed;
            ContactsEmptyText.Visibility = Visibility.Collapsed;
        }

        private void HideContactsLoading()
        {
            ContactsLoadingPanel.Visibility = Visibility.Collapsed;
        }

        private void ChatSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ChatsListBox.SelectedItem != null && ChatsListBox.SelectedItem is DialogViewModel selectedItem)
            {
                // Navigate to chat page with parameters
                Frame.Navigate(typeof(ChatPage), new Dictionary<string, string>
                {
                    { "peer", selectedItem.PeerId },
                    { "title", selectedItem.Title },
                    { "isGroup", selectedItem.IsGroup.ToString() }
                });
            }
            ChatsListBox.SelectedItem = null;
        }

        private void ContactSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsListBox.SelectedItem != null && ContactsListBox.SelectedItem is ContactViewModel selectedItem)
            {
                // Navigate to chat page with parameters
                Frame.Navigate(typeof(ChatPage), new Dictionary<string, string>
                {
                    { "peer", selectedItem.PeerId },
                    { "title", selectedItem.Title },
                    { "isGroup", "false" }
                });
            }
            ContactsListBox.SelectedItem = null;
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 0)
            {
                LoadChats();
            }
            else if (MainPivot.SelectedIndex == 1)
            {
                LoadContacts();
            }
        }

        private async void NewChatClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("New chat feature would be implemented here", "New Chat");
            await dialog.ShowAsync();
        }

        private void SettingsClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private async void LogoutClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Are you sure you want to logout?", "Logout");
            var confirmCommand = new UICommand("Yes");
            var cancelCommand = new UICommand("Cancel");
            dialog.Commands.Add(confirmCommand);
            dialog.Commands.Add(cancelCommand);
            
            var result = await dialog.ShowAsync();
            if (result == confirmCommand)
            {
                _apiClient.ClearSession();
                IsolatedStorageHelper.RemoveValue("user_token");
                Frame.Navigate(typeof(WelcomePage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitializeLiveNotifications()
        {
            bool? nullable = IsolatedStorageHelper.GetValue<bool?>("notifications_enabled");
            _notificationsEnabled = !nullable.HasValue || nullable.Value;
            if (!_notificationsEnabled)
                return;
            StartLiveNotifications();
        }

        private void StartLiveNotifications()
        {
            if (_notificationTimer != null)
                return;
            CheckForNewMessages();
            _notificationTimer = new DispatcherTimer();
            _notificationTimer.Interval = TimeSpan.FromMinutes(15);
            _notificationTimer.Tick += (s, e) => ShowPeriodicNotification();
            _notificationTimer.Start();
        }

        private void ShowPeriodicNotification()
        {
            if (!_notificationsEnabled)
                return;
            string userToken = IsolatedStorageHelper.GetValue<string>("user_token");
            if (string.IsNullOrEmpty(userToken))
                return;
                
            new MpgramApiClient(userToken, _dispatcher).GetDialogs(
                response => {
                    if (response == null || response.dialogs == null)
                        return;
                    int totalUnread = CalculateTotalUnread(response.dialogs);
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        // In UWP, we would use Toast Notifications instead of ShellToast
                        // This is a placeholder for now
                        Debug.WriteLine($"Periodic notification: {totalUnread} unread messages");
                    });
                }, 
                ex => Debug.WriteLine("Periodic notification check failed: " + ex.Message)
            );
        }

        private int CalculateTotalUnread(List<Dialog> dialogs)
        {
            int totalUnread = 0;
            foreach (Dialog dialog in dialogs)
            {
                int? unread = dialog.unread;
                if (unread.HasValue)
                {
                    totalUnread += unread.Value;
                }
            }
            return totalUnread;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearchText = SearchBox.Text.ToLower();
            FilterChats();
        }

        private void FilterChats()
        {
            if (string.IsNullOrWhiteSpace(_currentSearchText))
            {
                ChatsListBox.ItemsSource = _chats;
                SearchResultsText.Visibility = Visibility.Collapsed;
            }
            else
            {
                var filteredChats = _chats.Where(chat => 
                    chat.Title.ToLower().Contains(_currentSearchText) || 
                    (chat.LastMessage != null && chat.LastMessage.ToLower().Contains(_currentSearchText))
                ).ToList();
                
                ChatsListBox.ItemsSource = filteredChats;
                SearchResultsText.Text = $"Found {filteredChats.Count} chats";
                SearchResultsText.Visibility = Visibility.Visible;
            }
        }

        private void ApplyTheme()
        {
            string theme = IsolatedStorageHelper.GetValue<string>("theme");
            if (string.IsNullOrEmpty(theme))
                theme = "dark";
                
            switch (theme)
            {
                case "light":
                    this.BackgroundColor = new SolidColorBrush(Colors.White);
                    this.ForegroundColor = new SolidColorBrush(Colors.Black);
                    this.SubtleTextColor = new SolidColorBrush(Color.FromArgb(255, 102, 102, 102));
                    this.AccentColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                    this.ListBackgroundColor = new SolidColorBrush(Colors.White);
                    this.ItemBackgroundColor = new SolidColorBrush(Colors.White);
                    this.AppBarBackgroundColor = Colors.White;
                    this.AppBarForegroundColor = Colors.Black;
                    break;
                case "blue":
                    this.BackgroundColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                    this.ForegroundColor = new SolidColorBrush(Colors.White);
                    this.SubtleTextColor = new SolidColorBrush(Color.FromArgb(255, 136, 187, 232));
                    this.AccentColor = new SolidColorBrush(Color.FromArgb(255, 64, 196, 255));
                    this.ListBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                    this.ItemBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 0, 100, 195));
                    this.AppBarBackgroundColor = Color.FromArgb(255, 0, 120, 215);
                    this.AppBarForegroundColor = Colors.White;
                    break;
                default: // dark theme
                    this.BackgroundColor = new SolidColorBrush(Color.FromArgb(255, 27, 27, 27));
                    this.ForegroundColor = new SolidColorBrush(Colors.White);
                    this.SubtleTextColor = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
                    this.AccentColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                    this.ListBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 27, 27, 27));
                    this.ItemBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 37, 37, 37));
                    this.AppBarBackgroundColor = Color.FromArgb(255, 27, 27, 27);
                    this.AppBarForegroundColor = Colors.White;
                    break;
            }
            
            // Apply colors to page elements
            this.Background = this.BackgroundColor;
        }

        private void CheckForNewMessages()
        {
            if (!_notificationsEnabled)
                return;
                
            string userToken = IsolatedStorageHelper.GetValue<string>("user_token");
            if (string.IsNullOrEmpty(userToken))
                return;
                
            new MpgramApiClient(userToken, _dispatcher).GetDialogs(
                response => {
                    if (response == null || response.dialogs == null)
                        return;
                        
                    int totalUnread = CalculateTotalUnread(response.dialogs);
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        // Only show notification if unread count increased
                        if (totalUnread > _lastTotalUnread && _lastTotalUnread > 0)
                        {
                            // In UWP, we would use Toast Notifications instead of ShellToast
                            // This is a placeholder for now
                            Debug.WriteLine($"New messages notification: {totalUnread - _lastTotalUnread} new messages");
                        }
                        _lastTotalUnread = totalUnread;
                    });
                }, 
                ex => Debug.WriteLine("Notification check failed: " + ex.Message)
            );
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(SettingsPage));
            }
            catch { }
        }
    }
}