using System;

#nullable disable
namespace TelegramClient
{
  public class InputDialogCompletedEventArgs : EventArgs
  {
    public CustomDialogResult Result { get; set; }

    public string Text { get; set; }
  }
}
