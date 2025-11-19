using System.IO.IsolatedStorage;
using Windows.Storage;
using System.Threading.Tasks;

#nullable disable
namespace TelegramClient
{
    public static class IsolatedStorageHelper
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static T GetValue<T>(string key)
        {
            if (localSettings.Values.ContainsKey(key))
            {
                var value = localSettings.Values[key];
                if (value is T)
                {
                    return (T)value;
                }
            }
            return default(T);
        }

        public static void SetValue<T>(string key, T value)
        {
            localSettings.Values[key] = value;
        }

        public static void RemoveValue(string key)
        {
            if (localSettings.Values.ContainsKey(key))
            {
                localSettings.Values.Remove(key);
            }
        }
    }
}
