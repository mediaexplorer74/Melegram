using System.Collections.Generic;

#nullable disable
namespace TelegramClient
{
  public class MessagesResponse
  {
    public List<Message> messages { get; set; }

    public Dictionary<string, User> users { get; set; }

    public Dictionary<string, Chat> chats { get; set; }

    public int? count { get; set; }
  }
}
