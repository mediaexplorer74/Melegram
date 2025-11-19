using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI;

#nullable disable
namespace TelegramClient
{
    public class InputDialog
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public string DefaultText { get; set; }

        public event EventHandler<InputDialogCompletedEventArgs> Completed;

        public async void Show()
        {
            // Create a ContentDialog for UWP
            ContentDialog dialog = new ContentDialog();
            dialog.Title = this.Title;
            
            // Create the content
            StackPanel panel = new StackPanel();
            panel.Children.Add(new TextBlock() { Text = this.Message, TextWrapping = TextWrapping.Wrap });
            
            TextBox inputBox = new TextBox();
            inputBox.Text = this.DefaultText;
            inputBox.Margin = new Thickness(0, 10, 0, 0);
            panel.Children.Add(inputBox);
            
            dialog.Content = panel;
            dialog.PrimaryButtonText = "OK";
            dialog.SecondaryButtonText = "Cancel";
            
            // Show the dialog and wait for result
            ContentDialogResult result = await dialog.ShowAsync();
            
            if (this.Completed != null)
            {
                InputDialogCompletedEventArgs args = new InputDialogCompletedEventArgs();
                if (result == ContentDialogResult.Primary)
                {
                    args.Result = CustomDialogResult.OK;
                    args.Text = inputBox.Text;
                }
                else
                {
                    args.Result = CustomDialogResult.Cancel;
                }
                this.Completed(this, args);
            }
        }
    }
}
