using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;

#nullable disable
namespace TelegramClient
{
    public sealed partial class App : Application
    {
       
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;            
        }


        /*protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                Debug.WriteLine("OnLaunched called with PrelaunchActivated: " + e.PrelaunchActivated);
                
                Frame rootFrame = Window.Current.Content as Frame;
                Debug.WriteLine("Existing rootFrame: " + (rootFrame != null));

               
                if (rootFrame == null)
                {
                    Debug.WriteLine("Creating new frame");
                    rootFrame = new Frame();
                    Debug.WriteLine("New frame created");

                    rootFrame.NavigationFailed += this.OnNavigationFailed;
                    Debug.WriteLine("NavigationFailed event handler attached");

                    Window.Current.Content = rootFrame;
                    Debug.WriteLine("RootFrame assigned to Window.Current.Content");
                    
                    // Try to set a visible background
                    Debug.WriteLine("Setting window background color");
                    try
                    {
                        ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                        Debug.WriteLine("ApplicationView bounds mode set");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed to set ApplicationView bounds mode: " + ex.Message);
                    }
                }

                if (!e.PrelaunchActivated)
                {
                    Debug.WriteLine("Not prelaunch activated");
                    if (rootFrame.Content == null)
                    {
                        Debug.WriteLine("RootFrame content is null, navigating to test page");
                        // Create a very simple test page first
                        var testPage = new Page();
                        var grid = new Grid();
                        grid.Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Blue);
                        var textBlock = new TextBlock();
                        textBlock.Text = "TEST PAGE LOADED SUCCESSFULLY!\nIf you can see this, the app is working!";
                        textBlock.FontSize = 36;
                        textBlock.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Yellow);
                        textBlock.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                        textBlock.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                        textBlock.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
                        textBlock.TextAlignment = Windows.UI.Xaml.TextAlignment.Center;
                        grid.Children.Add(textBlock);
                        testPage.Content = grid;
                        
                        rootFrame.Content = testPage;
                        Debug.WriteLine("Test page assigned directly");
                    }
                    else
                    {
                        Debug.WriteLine("RootFrame content is NOT null");
                    }

                    Debug.WriteLine("Activating window");
                    Window.Current.Activate();
                    Debug.WriteLine("Window activated");
                    
                    // Add a small delay to ensure window is properly activated
                    await Task.Delay(100);
                    
                    // Try to ensure window is visible and has a size
                    try
                    {
                        var view = ApplicationView.GetForCurrentView();
                        Debug.WriteLine("Current view bounds: " + view.VisibleBounds.ToString());
                        
                        // Try to set a preferred size
                        view.SetPreferredMinSize(new Windows.Foundation.Size(480, 768));
                        Debug.WriteLine("Preferred min size set");
                        
                        // Try to make the window visible
                        if (!view.IsFullScreen)
                        {
                            view.TryEnterFullScreenMode();
                            Debug.WriteLine("Entered fullscreen mode");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed to set view properties: " + ex.Message);
                    }
                }
                else
                {
                    Debug.WriteLine("Prelaunch activated, skipping navigation");
                }
                
                Debug.WriteLine("OnLaunched completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in OnLaunched: " + ex.Message + "\n" + ex.StackTrace);
            }
        }*/


        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            //await appStartup();
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }


            rootFrame.Navigate(typeof(MainPage), e.Arguments);

            // Ensure the current window is active
            Window.Current.Activate();

        }



        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            //Debug.WriteLine($"Navigation failed: {e.SourcePageType.FullName} - {e.Exception.Message}");
            //Debug.WriteLine("Navigation failed stack trace: " + e.Exception.StackTrace);
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName, e.Exception);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            //Debug.WriteLine("App suspending");
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}