#nullable disable
namespace TelegramClient
{
  public class LoginResponse
  {
    public string res { get; set; }

    public string user { get; set; }

    public string phone_code_hash { get; set; }

    public string captcha_id { get; set; }

    public string error { get; set; }

    public string message { get; set; }
  }
}
