using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable disable
namespace TelegramClient
{
    public sealed partial class WelcomePage : Page
    {
       
        public WelcomePage() 
        {
            //Debug.WriteLine("Welcome constructor called");
            this.InitializeComponent();
            //Debug.WriteLine("Welcome InitializeComponent completed");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                //Debug.WriteLine("Welcome OnNavigatedTo called");
                base.OnNavigatedTo(e);

                // Очищаем backstack, чтобы с приветственного экрана нельзя было вернуться назад
                Frame frame = Window.Current.Content as Frame;
                if (frame != null)
                {
                    //Debug.WriteLine("Frame found, clearing backstack. Backstack count before: " + frame.BackStack.Count);
                    frame.BackStack.Clear();
                    //Debug.WriteLine("Backstack cleared. Backstack count after: " + frame.BackStack.Count);
                }
                else
                {
                    Debug.WriteLine("Frame not found");
                }
                //Debug.WriteLine("Welcome OnNavigatedTo completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in Welcome OnNavigatedTo: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Debug.WriteLine("Welcome button clicked");
                if (this.Frame != null)
                {
                    //Debug.WriteLine("Frame is not null, navigating to LoginPage");
                    bool navigationResult = this.Frame.Navigate(typeof(LoginPage));
                    //Debug.WriteLine("Navigation result: " + navigationResult);
                }
                else
                {
                    Debug.WriteLine("Frame is null, cannot navigate");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in Welcome button click: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}