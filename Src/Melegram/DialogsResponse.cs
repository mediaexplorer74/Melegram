using System.Collections.Generic;

#nullable disable
namespace TelegramClient
{
  public class DialogsResponse
  {
    public List<Dialog> dialogs { get; set; }

    public Dictionary<string, User> users { get; set; }

    public Dictionary<string, Chat> chats { get; set; }

    public int? count { get; set; }
  }
}
