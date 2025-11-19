#nullable disable
namespace TelegramClient
{
  public class Dialog
  {
    public string id { get; set; }

    public int? unread { get; set; }

    public Message msg { get; set; }
  }
}
