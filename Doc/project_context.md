# Overview
UWP-порт отреверсенного Windows Phone 7 приложения Melegram (Telegram-клиент), минимальная версия платформы 10.0.10240.0, Target 10.0.19041.0.

# Goals
- Перенести все страницы и навигацию с WP7/Silverlight (PhoneApplicationPage, Microsoft.Phone.*) на UWP (Page, Frame.Navigate).
- Сохранить UX с Pivot на главной странице.
- Удалить или заменить все стили/ресурсы со словом "Phone" на нормальные UWP-аналоги.
- Обеспечить сборку через MSBuild.exe для x64 и совместимость с Windows 10 Mobile 10240.

# Progress
- Проект уже UWP (TargetPlatform UAP, MinVersion=10.0.10240.0, Target=10.0.19041.0).
- App.xaml очищен от WP7-пространств имён и PhoneApplicationService.
- App.xaml.cs переписан на стандартный UWP App (sealed partial App : Application, OnLaunched, Frame в Window.Current.Content, стартовая страница welcome).

# Ready
- Переписать все PhoneApplicationPage/XAML на UWP Page + Windows.UI.Xaml.*.
- Удалить Microsoft.Phone.* API (SystemTray, ShellToast, ShellTile и т.п.) или заменить их простыми UWP-эквивалентами/заглушками.
- Перенести Phone-стили (PhoneFont*, PhoneText*Style, PhoneAccentBrush и т.д.) в UWP-ресурсы или заменить прямыми значениями.
- Осовременить навигацию (Frame.Navigate, BackStack) и уведомления с учётом ограничений UWP/Win10 Mobile.
