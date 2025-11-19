using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http.Headers;
using System.Linq;
namespace TelegramClient
{
  public class MpgramApiClient : IDisposable
  {
    private readonly string _baseUrl = "http://mp.nnchan.ru/api.php";
    private string _userToken;
    private readonly HttpClientHandler _httpClientHandler;
    private readonly HttpClient _httpClient;
    private readonly CoreDispatcher _dispatcher;

    public MpgramApiClient(string userToken, CoreDispatcher dispatcher = null)
    {
      _userToken = userToken;
      _httpClientHandler = new HttpClientHandler
      {
          UseCookies = true,
          CookieContainer = new CookieContainer()
      };
      
      _httpClient = new HttpClient(_httpClientHandler);
      _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
      
      _dispatcher = dispatcher ?? Window.Current.Dispatcher;
    }

    public MpgramApiClient() : this(null)
    {
    }

    public void SetUserToken(string userToken) => _userToken = userToken;

    private async Task MakeApiRequestAsync(
        string method,
        Dictionary<string, string> parameters,
        Action<string> onSuccess,
        Action<Exception> onError)
    {
        try
        {
            var queryString = $"{_baseUrl}?method={method}";
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    queryString += $"&{parameter.Key}={Uri.EscapeDataString(parameter.Value)}";
                }
            }
            queryString += "&v=10";

            var request = new HttpRequestMessage(HttpMethod.Get, queryString);
            if (!string.IsNullOrEmpty(_userToken))
            {
                request.Headers.Add("X-Mpgram-User", _userToken);
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                onSuccess?.Invoke(json);
            });
        }
        catch (Exception ex)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                onError?.Invoke(ex);
            });
        }
    }

    private async Task MakeApiPostRequestAsync(
        string method,
        Dictionary<string, string> parameters,
        Action<string> onSuccess,
        Action<Exception> onError)
    {
        try
        {
            var url = $"{_baseUrl}?method={method}";
            var content = new FormUrlEncodedContent(parameters ?? new Dictionary<string, string>());
            
            if (!string.IsNullOrEmpty(_userToken))
            {
                _httpClient.DefaultRequestHeaders.Remove("X-Mpgram-User");
                _httpClient.DefaultRequestHeaders.Add("X-Mpgram-User", _userToken);
            }

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                onSuccess?.Invoke(json);
            });
        }
        catch (Exception ex)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                onError?.Invoke(ex);
            });
        }
    }

    public async void GetCaptchaImage(
        string captchaId,
        Action<BitmapImage> onSuccess,
        Action<Exception> onError)
    {
        try
        {
            var url = $"{_baseUrl}?method=getCaptchaImg&captcha_id={Uri.EscapeDataString(captchaId)}&v=10";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            if (!string.IsNullOrEmpty(_userToken))
            {
                request.Headers.Add("X-Mpgram-User", _userToken);
            }

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    onSuccess?.Invoke(bitmapImage);
                });
            }
        }
        catch (Exception ex)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                onError?.Invoke(ex);
            });
        }
    }

    public void PhoneLogin(
        string phone,
        string captchaId,
        string captchaKey,
        Action<LoginResponse> onSuccess,
        Action<Exception> onError)
    {
        var parameters = new Dictionary<string, string>
        {
            ["phone"] = phone
        };

        if (!string.IsNullOrEmpty(captchaId))
            parameters["captcha_id"] = captchaId;
            
        if (!string.IsNullOrEmpty(captchaKey))
            parameters["captcha_key"] = captchaKey;

        _ = MakeApiRequestAsync("phoneLogin", parameters, json =>
        {
            var loginResponse = JsonHelper.DeserializeLoginResponse(json);
            if (!string.IsNullOrEmpty(loginResponse.user))
                _userToken = loginResponse.user;
            onSuccess(loginResponse);
        }, onError);
    }

    public void PhoneLogin(
        string phone,
        Action<LoginResponse> onSuccess,
        Action<Exception> onError)
    {
        PhoneLogin(phone, null, null, onSuccess, onError);
    }

    public void CompletePhoneLogin(
        string code,
        Action<ApiResponse> onSuccess,
        Action<Exception> onError)
    {
        var parameters = new Dictionary<string, string> { ["code"] = code };
        _ = MakeApiRequestAsync("completePhoneLogin", parameters, 
            json => onSuccess(JsonHelper.DeserializeApiResponse(json)), 
            onError);
    }

    public void CheckAuth(Action<ApiResponse> onSuccess, Action<Exception> onError)
    {
        _ = MakeApiRequestAsync("checkAuth", null, 
            json => onSuccess(JsonHelper.DeserializeApiResponse(json)), 
            onError);
    }

    public void GetDialogs(
        Action<DialogsResponse> onSuccess,
        Action<Exception> onError,
        int limit,
        string folder)
    {
        var parameters = new Dictionary<string, string>
        {
            ["limit"] = limit.ToString()
        };

        if (!string.IsNullOrEmpty(folder))
            parameters["f"] = folder;

        _ = MakeApiRequestAsync("getDialogs", parameters, 
            json => onSuccess(JsonHelper.DeserializeDialogsResponse(json)), 
            onError);
    }

    public void GetDialogs(Action<DialogsResponse> onSuccess, Action<Exception> onError)
    {
        GetDialogs(onSuccess, onError, 100, null);
    }

    public void GetHistory(
        string peer,
        Action<MessagesResponse> onSuccess,
        Action<Exception> onError,
        int limit,
        int offsetId)
    {
        var parameters = new Dictionary<string, string>
        {
            ["peer"] = peer,
            ["limit"] = limit.ToString(),
            ["offset_id"] = offsetId.ToString()
        };

        _ = MakeApiRequestAsync("getHistory", parameters, 
            json => onSuccess(JsonHelper.DeserializeMessagesResponse(json)), 
            onError);
    }

    public void GetHistory(
        string peer,
        Action<MessagesResponse> onSuccess,
        Action<Exception> onError)
    {
        GetHistory(peer, onSuccess, onError, 100, 0);
    }

    public void SendMessage(
        string peer,
        string text,
        Action<ApiResponse> onSuccess,
        Action<Exception> onError,
        int replyTo)
    {
        var parameters = new Dictionary<string, string>
        {
            ["peer"] = peer,
            ["text"] = text
        };

        if (replyTo > 0)
            parameters["reply"] = replyTo.ToString();

        _ = MakeApiPostRequestAsync("sendMessage", parameters, 
            json => onSuccess(JsonHelper.DeserializeApiResponse(json)), 
            onError);
    }

    public void SendMessage(
        string peer,
        string text,
        Action<ApiResponse> onSuccess,
        Action<Exception> onError)
    {
        SendMessage(peer, text, onSuccess, onError, 0);
    }

    public void GetSelf(Action<SelfResponse> onSuccess, Action<Exception> onError)
    {
        _ = MakeApiRequestAsync("getSelf", null, 
            json => onSuccess(JsonHelper.DeserializeSelfResponse(json)), 
            onError);
    }

    public void InitLogin(Action<LoginResponse> onSuccess, Action<Exception> onError)
    {
        _ = MakeApiRequestAsync("initLogin", null, 
            json => onSuccess(JsonHelper.DeserializeLoginResponse(json)), 
            onError);
    }

    public async void GetProfileImage(
        string photoUrl,
        Action<BitmapImage> onSuccess,
        Action<Exception> onError)
    {
        try
        {
            Debug.WriteLine($"GetProfileImage called with URL: {photoUrl}");
            
            var fullUrl = photoUrl;
            if (!photoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var baseUrl = _baseUrl.Replace("/api.php", "");
                fullUrl = photoUrl.StartsWith("/") 
                    ? baseUrl + photoUrl 
                    : baseUrl + "/" + photoUrl;
            }
            
            Debug.WriteLine($"Full image URL: {fullUrl}");

            var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            
            if (!string.IsNullOrEmpty(_userToken))
            {
                request.Headers.Add("X-Mpgram-User", _userToken);
            }

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            Debug.WriteLine($"Image response status: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                Debug.WriteLine($"Downloaded {memoryStream.Length} bytes for image");

                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                
                //Debug.WriteLine("BitmapImage created successfully");
                
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    onSuccess?.Invoke(bitmapImage);
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetProfileImage exception: {ex.Message}");
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                onError?.Invoke(ex);
            });
        }
    }

    public string GetCaptchaUrl(string captchaId)
    {
        return $"{_baseUrl}?method=getCaptchaImg&captcha_id={Uri.EscapeDataString(captchaId)}&v=10";
    }

    public void ClearSession()
    {
        _httpClientHandler.CookieContainer = new CookieContainer();
        _userToken = null;
    }

    public string GetCookieDebugInfo()
    {
        try
        {
            var cookies = _httpClientHandler.CookieContainer.GetCookies(new Uri(_baseUrl));
            var cookieDebugInfo = "Cookies: ";
            
            foreach (Cookie cookie in cookies)
            {
                cookieDebugInfo += $"{cookie.Name}={cookie.Value}; ";
            }
            
            return cookieDebugInfo;
        }
        catch (Exception ex)
        {
            return $"Cookie error: {ex.Message}";
        }
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
        _httpClientHandler?.Dispose();
    }
  }
}
