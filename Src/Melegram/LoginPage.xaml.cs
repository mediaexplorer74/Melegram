using System;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Windows.System.Threading;
using Windows.UI.Popups;
using System.Threading.Tasks;

#nullable disable
namespace TelegramClient
{
    public sealed partial class LoginPage : Page
    {
        private MpgramApiClient _apiClient;
        private string _phoneNumber;
        private string _phoneCodeHash;
        private string _userToken;
        private string _currentCaptchaId;
        private int _captchaAttempts = 0;
        private CoreDispatcher _dispatcher;

        public LoginPage()
        {
            Debug.WriteLine("LoginPage constructor called");
            this.InitializeComponent();
            this._dispatcher = Window.Current.Dispatcher;
            this._apiClient = new MpgramApiClient(null, _dispatcher);
            this.CodeSection.Visibility = Visibility.Collapsed;
            this.VerifyButton.Visibility = Visibility.Collapsed;
            this.StatusText.Text = "Enter your phone number to start";
            Debug.WriteLine("LoginPage InitializeComponent completed");
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("LoginPage LoginButton_Click called");
            if (this.CaptchaSection.Visibility == Visibility.Visible)
                this.SendCodeWithCaptcha();
            else
                this.LoginClicked(sender, e);
        }

        private void LoginClicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("LoginPage LoginClicked called");
            this._phoneNumber = this.PhoneTextBox.Text.Trim();
            if (string.IsNullOrEmpty(this._phoneNumber) || this._phoneNumber.Length < 5)
            {
                this.ShowStatus("Please enter a valid phone number");
            }
            else
            {
                this._captchaAttempts = 0;
                this.LoginButton.IsEnabled = false;
                this.LoginButton.Content = "Sending...";
                this.ShowStatus("Sending verification code...");
                this._apiClient.PhoneLogin(this._phoneNumber, 
                    response => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.HandleLoginResponse(response));
                    }, 
                    ex => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            this.LoginButton.IsEnabled = true;
                            this.LoginButton.Content = "Send Code";
                            this.ShowStatus("Error: " + ex.Message);
                        });
                    });
            }
        }

        private void HandleLoginResponse(LoginResponse response)
        {
            Debug.WriteLine("LoginPage HandleLoginResponse called");
            this.LoginButton.IsEnabled = true;
            this.LoginButton.Content = "Send Code";
            if (response == null)
            {
                this.ShowStatus("Login failed: No response from server");
            }
            else
            {
                Debug.WriteLine($"LOGIN RESPONSE: res={response.res}, user={response.user}, captcha_id={response.captcha_id}");
                if (response.res == "code_sent")
                {
                    this._phoneCodeHash = response.phone_code_hash;
                    this._userToken = response.user;
                    this.ShowCodeInput();
                    this.ShowStatus("✓ Verification code sent via SMS! Please enter the code you received.");
                }
                else if (response.res == "need_captcha")
                {
                    ++this._captchaAttempts;
                    if (this._captchaAttempts > 3)
                    {
                        this.ShowStatus("Too many captcha attempts. Creating new session...");
                        this.CreateNewSession();
                    }
                    else if (string.IsNullOrEmpty(response.captcha_id))
                    {
                        this.ShowStatus("Captcha required but no captcha ID received");
                        this.GetNewCaptchaId();
                    }
                    else
                    {
                        this._currentCaptchaId = response.captcha_id;
                        this.ShowCaptchaInput();
                        this.LoadCaptchaImage();
                        this.ShowStatus($"Security check required. Please solve the captcha. (Attempt {_captchaAttempts}/3)");
                    }
                }
                else if (response.res == "wrong_captcha")
                {
                    this.ShowStatus("✗ Wrong captcha! Please try again with the new image.");
                    this.GetNewCaptchaId();
                }
                else if (response.res == "captcha_expired")
                {
                    this.ShowStatus("Captcha expired! Loading new one...");
                    this.GetNewCaptchaId();
                }
                else if (response.res == "phone_number_invalid")
                    this.ShowStatus("✗ Invalid phone number. Please check and try again.");
                else if (response.res == "phone_code_invalid")
                    this.ShowStatus("✗ Invalid verification code. Please try again.");
                else if (response.res == "phone_code_expired")
                    this.ShowStatus("✗ Verification code expired. Please request a new one.");
                else if (response.res == "auth_restart")
                {
                    this.ShowStatus("Session expired. Starting new login...");
                    this.CreateNewSession();
                }
                else
                {
                    string message = "Login failed: " + response.res;
                    if (!string.IsNullOrEmpty(response.message))
                        message = message + " - " + response.message;
                    this.ShowStatus(message);
                }
            }
        }

        private void CreateNewSession()
        {
            Debug.WriteLine("LoginPage CreateNewSession called");
            this.ShowStatus("Creating new session...");
            this._apiClient.ClearSession();
            this._captchaAttempts = 0;
            this.CaptchaSection.Visibility = Visibility.Collapsed;
            this.LoginButton.Content = "Send Code";
            this.ShowStatus("New session ready. Try sending code again.");
        }

        private void ShowCaptchaInput()
        {
            Debug.WriteLine("LoginPage ShowCaptchaInput called");
            this.CaptchaSection.Visibility = Visibility.Visible;
            this.CodeSection.Visibility = Visibility.Collapsed;
            this.LoginButton.Content = "Submit Captcha";
            this.LoginButton.Visibility = Visibility.Visible;
            this.VerifyButton.Visibility = Visibility.Collapsed;
            this.CaptchaTextBox.Text = "";
            this.CaptchaLoadingText.Visibility = Visibility.Visible;
            this.CaptchaImage.Visibility = Visibility.Collapsed;
        }

        private void LoadCaptchaImage()
        {
            Debug.WriteLine("LoginPage LoadCaptchaImage called");
            if (string.IsNullOrEmpty(this._currentCaptchaId))
            {
                this.ShowStatus("No captcha ID available");
            }
            else
            {
                this._apiClient.GetCaptchaImage(this._currentCaptchaId, 
                    image => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            if (image != null)
                            {
                                this.CaptchaImage.Source = image;
                                this.CaptchaLoadingText.Visibility = Visibility.Collapsed;
                                this.CaptchaImage.Visibility = Visibility.Visible;
                            }
                            else
                                this.ShowStatus("Failed to load captcha image");
                        });
                    }, 
                    ex => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            this.ShowStatus("Failed to load captcha: " + ex.Message);
                        });
                    });
            }
        }

        private void RefreshCaptchaClicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("LoginPage RefreshCaptchaClicked called");
            this.ShowStatus("Refreshing captcha...");
            this.GetNewCaptchaId();
        }

        private void GetNewCaptchaId()
        {
            Debug.WriteLine("LoginPage GetNewCaptchaId called");
            this.ShowStatus("Getting new captcha...");
            this._apiClient.InitLogin(
                response => {
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        if (response != null && !string.IsNullOrEmpty(response.captcha_id))
                        {
                            this._currentCaptchaId = response.captcha_id;
                            this.ShowCaptchaInput();
                            this.LoadCaptchaImage();
                        }
                        else
                            this.ShowStatus("Failed to get new captcha. Please try again.");
                    });
                }, 
                ex => {
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        this.ShowStatus("Error getting captcha: " + ex.Message);
                    });
                });
        }

        private void SendCodeWithCaptcha()
        {
            Debug.WriteLine("LoginPage SendCodeWithCaptcha called");
            string captchaKey = this.CaptchaTextBox.Text.Trim();
            if (string.IsNullOrEmpty(captchaKey))
            {
                this.ShowStatus("Please enter the captcha text");
            }
            else
            {
                this.LoginButton.IsEnabled = false;
                this.LoginButton.Content = "Verifying Captcha...";
                this.ShowStatus("Verifying captcha...");
                this._apiClient.PhoneLogin(this._phoneNumber, this._currentCaptchaId, captchaKey, 
                    response => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.HandleLoginResponse(response));
                    }, 
                    ex => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            this.LoginButton.IsEnabled = true;
                            this.LoginButton.Content = "Submit Captcha";
                            this.ShowStatus("Error: " + ex.Message);
                        });
                    });
            }
        }

        private void ShowCodeInput()
        {
            Debug.WriteLine("LoginPage ShowCodeInput called");
            this.CaptchaSection.Visibility = Visibility.Collapsed;
            this.CodeSection.Visibility = Visibility.Visible;
            this.LoginButton.Visibility = Visibility.Collapsed;
            this.VerifyButton.Visibility = Visibility.Visible;
            this.CodeTextBox.Text = "";
            this.CodeTextBox.Focus(FocusState.Programmatic);
        }

        private void VerifyClicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("LoginPage VerifyClicked called");
            string code = this.CodeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(code) || code.Length < 3)
            {
                this.ShowStatus("Please enter the verification code");
            }
            else
            {
                this.VerifyButton.IsEnabled = false;
                this.VerifyButton.Content = "Verifying...";
                this.ShowStatus("Verifying code...");
                this._apiClient.SetUserToken(this._userToken);
                this._apiClient.CompletePhoneLogin(code, 
                    response => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            this.VerifyButton.IsEnabled = true;
                            this.VerifyButton.Content = "Verify Code";
                            if (response != null && response.res == "1")
                            {
                                IsolatedStorageHelper.SetValue<string>("user_token", this._userToken);
                                this.ShowStatus("✓ Login successful! Loading your chats...");
                                
                                // Use Task for delay
                                Task.Run(async () => {
                                    await Task.Delay(1500);
                                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                                        Frame.Navigate(typeof(MainPage));
                                    });
                                });
                            }
                            else if (response != null)
                            {
                                string message = "Verification failed: " + response.res;
                                if (!string.IsNullOrEmpty(response.message))
                                    message = message + " - " + response.message;
                                this.ShowStatus(message);
                            }
                            else
                                this.ShowStatus("Verification failed: No response from server");
                        });
                    }, 
                    ex => {
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            this.VerifyButton.IsEnabled = true;
                            this.VerifyButton.Content = "Verify Code";
                            this.ShowStatus("Error: " + ex.Message);
                        });
                    });
            }
        }

        private void ShowStatus(string message)
        {
            this.StatusText.Text = message;
        }

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("LoginPage DebugButton_Click called");
            this.ShowStatus("Debug: " + this._apiClient.GetCookieDebugInfo());
            Debug.WriteLine($"DEBUG: Phone={_phoneNumber}, Token={_userToken}, CaptchaID={_currentCaptchaId}");
        }

        private void PhoneTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                this.LoginButton_Click(sender, e);
        }

        private void CaptchaTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                this.SendCodeWithCaptcha();
        }

        private void CodeTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                this.VerifyClicked(sender, e);
        }
    }
}