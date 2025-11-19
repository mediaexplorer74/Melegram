#nullable disable
namespace TelegramClient
{
  public class Message
  {
    public string id { get; set; }

    public int date { get; set; }

    public string text { get; set; }

    public string peer_id { get; set; }

    public string from_id { get; set; }
  }
}
