using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI;

#nullable disable
namespace TelegramClient
{
    public class MessageViewModel
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public string Time { get; set; }

        public string SenderName { get; set; }

        public Visibility ShowSenderVisibility { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }

        public Brush BackgroundColor { get; set; }

        public Brush TextColor { get; set; }

        public Brush TimeColor { get; set; }

        public Thickness BubbleMargin { get; set; }

        public MessageViewModel(Message message, Dictionary<string, User> users, bool isGroupChat)
        {
            this.Id = message.id;
            this.Text = message.text ?? "[Media]";
            this.Time = message.date <= 0 ? "now" : new DateTime(1970, 1, 1).AddSeconds((double)message.date).ToString("HH:mm");
            
            string myUserId = IsolatedStorageHelper.GetValue<string>("my_user_id");
            bool isMe = message.from_id == "me" || message.from_id == myUserId || string.IsNullOrEmpty(message.from_id);
            
            if (isMe)
            {
                this.HorizontalAlignment = HorizontalAlignment.Right;
                this.BackgroundColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                this.TextColor = new SolidColorBrush(Colors.White);
                this.TimeColor = new SolidColorBrush(Color.FromArgb(136, 255, 255, 255));
                this.BubbleMargin = new Thickness(40.0, 0.0, 0.0, 0.0);
                this.SenderName = "You";
                this.ShowSenderVisibility = Visibility.Collapsed;
            }
            else
            {
                this.HorizontalAlignment = HorizontalAlignment.Left;
                this.BackgroundColor = new SolidColorBrush(Color.FromArgb(255, 229, 229, 229));
                this.TextColor = new SolidColorBrush(Colors.Black);
                this.TimeColor = new SolidColorBrush(Color.FromArgb(136, 0, 0, 0));
                this.BubbleMargin = new Thickness(0.0, 0.0, 40.0, 0.0);
                
                if (users != null && users.ContainsKey(message.from_id))
                {
                    User user = users[message.from_id];
                    this.SenderName = (user.fn + " " + user.ln).Trim();
                    if (string.IsNullOrEmpty(this.SenderName))
                        this.SenderName = user.name;
                    if (string.IsNullOrEmpty(this.SenderName))
                        this.SenderName = "User";
                }
                else
                    this.SenderName = "User";
                    
                this.ShowSenderVisibility = isGroupChat ? Visibility.Visible : Visibility.Collapsed;
            }
            
            Debug.WriteLine($"Message - From: {message.from_id}, MyID: {myUserId}, IsMe: {isMe}, Text: {message.text}");
        }
    }
}
