using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

#nullable disable
namespace TelegramClient
{
    public sealed partial class ChatPage : Page
    {
        private MpgramApiClient _apiClient;
        private string _peerId;
        private string _chatTitle;
        private bool _isGroup;
        private ObservableCollection<MessageViewModel> _messages;
        private DispatcherTimer _refreshTimer;
        private bool _isLoading = false;
        private CoreDispatcher _dispatcher;

        public ChatPage()
        {
            this.InitializeComponent();
            this._dispatcher = Window.Current.Dispatcher;
            this._apiClient = new MpgramApiClient(null, _dispatcher);
            this._messages = new ObservableCollection<MessageViewModel>();
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

            // Get navigation parameters
            if (e.Parameter is Dictionary<string, string> parameters)
            {
                if (parameters.ContainsKey("peer"))
                {
                    this._peerId = parameters["peer"];
                    this._chatTitle = parameters.ContainsKey("title") ? parameters["title"] : "Chat";
                    this._isGroup = parameters.ContainsKey("isGroup") && bool.Parse(parameters["isGroup"]);
                    this._apiClient.SetUserToken(IsolatedStorageHelper.GetValue<string>("user_token"));
                    Debug.WriteLine($"ChatPage opened: {_chatTitle} (ID: {_peerId}, Group: {_isGroup})");
                    this.UpdateTitle();
                    this.ShowLoadingState();
                    this.LoadMessages();
                    return;
                }
            }
            
            // If we don't have proper parameters, go back
            Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.StopAutoRefresh();
        }

        private void UpdateTitle()
        {
            this.PageTitle.Text = this._chatTitle;
            this.PageSubtitle.Text = "Loading...";
        }

        private void ShowLoadingState()
        {
            this.LoadingPanel.Visibility = Visibility.Visible;
            this.MessagesListBox.Visibility = Visibility.Collapsed;
            this.InputPanel.Visibility = Visibility.Collapsed;
            this.ErrorPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowContentState()
        {
            this.LoadingPanel.Visibility = Visibility.Collapsed;
            this.MessagesListBox.Visibility = Visibility.Visible;
            this.InputPanel.Visibility = Visibility.Visible;
            this.ErrorPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowErrorState(string errorMessage)
        {
            this.LoadingPanel.Visibility = Visibility.Collapsed;
            this.MessagesListBox.Visibility = Visibility.Collapsed;
            this.InputPanel.Visibility = Visibility.Collapsed;
            this.ErrorPanel.Visibility = Visibility.Visible;
            this.ErrorText.Text = errorMessage;
        }

        private async void LoadMessages()
        {
            if (this._isLoading)
                return;
            this._isLoading = true;
            Debug.WriteLine("Loading messages for: " + this._peerId);
            
            try
            {
                this._apiClient.GetHistory(this._peerId, 
                    response => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                            this._isLoading = false;
                            if (response != null)
                            {
                                this._messages.Clear();
                                if (response.messages != null && response.messages.Count > 0)
                                {
                                    this.DetermineMyUserId(response.messages);
                                    foreach (Message message in response.messages.OrderBy(m => m.date).ToList())
                                        this._messages.Add(new MessageViewModel(message, response.users, this._isGroup));
                                    this.RefreshMessagesList();
                                    this.ScrollToBottom();
                                    this.UpdateSubtitle($"{this._messages.Count} messages");
                                    this.ShowContentState();
                                    Debug.WriteLine($"Loaded {this._messages.Count} messages for {_peerId}");
                                    this.StartAutoRefresh();
                                }
                                else
                                {
                                    this.UpdateSubtitle("No messages yet");
                                    this.ShowContentState();
                                    Debug.WriteLine($"No messages found for {_peerId}");
                                    this.StartAutoRefresh();
                                }
                            }
                            else
                            {
                                this.UpdateSubtitle("Failed to load");
                                this.ShowErrorState("Failed to load messages from server");
                                Debug.WriteLine($"Failed to load messages for {_peerId}");
                            }
                        });
                    }, 
                    ex => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            this._isLoading = false;
                            this.UpdateSubtitle("Error loading");
                            this.ShowErrorState("Error loading messages: " + ex.Message);
                            Debug.WriteLine($"Error loading messages for {_peerId}: {ex.Message}");
                        });
                    });
            }
            catch (Exception ex)
            {
                this._isLoading = false;
                this.UpdateSubtitle("Error loading");
                this.ShowErrorState("Error loading messages: " + ex.Message);
                Debug.WriteLine($"Error loading messages for {_peerId}: {ex.Message}");
            }
        }

        private void DetermineMyUserId(List<Message> messages)
        {
            if (!string.IsNullOrEmpty(IsolatedStorageHelper.GetValue<string>("my_user_id")))
                return;
                
            this._apiClient.GetSelf(
                response => {
                    if (response != null && !string.IsNullOrEmpty(response.id))
                    {
                        IsolatedStorageHelper.SetValue<string>("my_user_id", response.id);
                        Debug.WriteLine("Stored my user ID: " + response.id);
                    }
                    else
                    {
                        Message message = messages.FirstOrDefault(m => !string.IsNullOrEmpty(m.from_id) && m.from_id != "0");
                        if (message != null && !string.IsNullOrEmpty(message.from_id))
                        {
                            IsolatedStorageHelper.SetValue<string>("my_user_id", message.from_id);
                            Debug.WriteLine("Guessed my user ID: " + message.from_id);
                        }
                    }
                }, 
                ex => Debug.WriteLine("Failed to get self info: " + ex.Message));
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e) => this.SendMessage();

        private void MessageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                this.SendMessage();
                e.Handled = true;
            }
        }

        private async void SendMessage()
        {
            string text = this.MessageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
                return;
            this.MessageTextBox.Text = "";
            string myUserId = IsolatedStorageHelper.GetValue<string>("my_user_id") ?? "me";
            
            MessageViewModel tempMessage = new MessageViewModel(new Message()
            {
                id = "temp_" + DateTime.Now.Ticks,
                text = text,
                date = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds,
                from_id = myUserId,
                peer_id = this._peerId
            }, null, this._isGroup);
            
            this._messages.Add(tempMessage);
            this.RefreshMessagesList();
            this.ScrollToBottom();
            Debug.WriteLine($"Sending message to {_peerId}: {text}");
            
            this._apiClient.SendMessage(this._peerId, text, 
                response => {
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                        if (response == null || response.res != "1")
                        {
                            this._messages.Remove(tempMessage);
                            this.RefreshMessagesList();
                            string errorMessage = response != null ? response.message : "Unknown error";
                            Debug.WriteLine("Failed to send message: " + errorMessage);
                            var dialog = new MessageDialog($"Failed to send message: {errorMessage}", "Error");
                            await dialog.ShowAsync();
                        }
                        else
                        {
                            Debug.WriteLine("Message sent successfully, reloading...");
                            this.LoadMessages();
                        }
                    });
                }, 
                ex => {
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                        this._messages.Remove(tempMessage);
                        this.RefreshMessagesList();
                        Debug.WriteLine("Error sending message: " + ex.Message);
                        var dialog = new MessageDialog($"Error sending message: {ex.Message}", "Error");
                        await dialog.ShowAsync();
                    });
                });
        }

        private void RefreshMessagesList()
        {
            this.MessagesListBox.ItemsSource = null;
            this.MessagesListBox.ItemsSource = this._messages;
        }

        private async void ScrollToBottom()
        {
            if (this._messages.Count <= 0)
                return;
                
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                try
                {
                    var scrollViewer = FindVisualChild<ScrollViewer>(this.MessagesListBox);
                    if (scrollViewer != null)
                    {
                        scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Scroll error: " + ex.Message);
                }
            });
        }

        // Helper method to find visual children
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void StartAutoRefresh()
        {
            if (this._refreshTimer != null)
                return;
                
            this._refreshTimer = new DispatcherTimer();
            this._refreshTimer.Interval = TimeSpan.FromSeconds(10);
            this._refreshTimer.Tick += (s, e) => {
                if (!this._isLoading)
                {
                    this.LoadMessages();
                }
            };
            this._refreshTimer.Start();
        }

        private void StopAutoRefresh()
        {
            if (this._refreshTimer == null)
                return;
            this._refreshTimer.Stop();
            this._refreshTimer = null;
        }

        private void RefreshClicked(object sender, RoutedEventArgs e) => this.LoadMessages();

        private void RetryLoading_Click(object sender, RoutedEventArgs e)
        {
            this.ShowLoadingState();
            this.LoadMessages();
        }

        private async void AttachClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Coming Soon!", "Attach");
            await dialog.ShowAsync();
        }

        private async void ClearHistoryClicked(object sender, RoutedEventArgs e)
        {
            var confirmDialog = new MessageDialog("Clear all messages in this chat?", "Clear History");
            var confirmCommand = new UICommand("Yes");
            var cancelCommand = new UICommand("Cancel");
            confirmDialog.Commands.Add(confirmCommand);
            confirmDialog.Commands.Add(cancelCommand);
            
            var result = await confirmDialog.ShowAsync();
            if (result == confirmCommand)
            {
                var infoDialog = new MessageDialog("Clear history feature would be implemented here", "Clear History");
                await infoDialog.ShowAsync();
            }
        }

        private async void ChatInfoClicked(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog(
                $"Chat Info:\nPeer: {this._peerId}\nTitle: {this._chatTitle}\nType: {(this._isGroup ? "Group" : "User")}\nMessages: {this._messages.Count}", "Chat Info");
            await dialog.ShowAsync();
        }

        private void UpdateSubtitle(string text) => this.PageSubtitle.Text = text;
    }
}
