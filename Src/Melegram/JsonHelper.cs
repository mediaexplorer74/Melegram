using MiniJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable disable
namespace TelegramClient
{
  public static class JsonHelper
  {
    public static LoginResponse DeserializeLoginResponse(string json)
    {
      try
      {
        if (!(Json.Deserialize(json) is Dictionary<string, object> dict))
          return new LoginResponse()
          {
            res = "parse_error",
            message = "Failed to parse JSON response"
          };
        return new LoginResponse()
        {
          res = JsonHelper.GetString(dict, "res"),
          user = JsonHelper.GetString(dict, "user"),
          phone_code_hash = JsonHelper.GetString(dict, "phone_code_hash"),
          captcha_id = JsonHelper.GetString(dict, "captcha_id"),
          error = JsonHelper.GetString(dict, "error"),
          message = JsonHelper.GetString(dict, "message")
        };
      }
      catch (Exception ex)
      {
        return new LoginResponse()
        {
          res = "parse_error",
          message = "JSON parsing error: " + ex.Message
        };
      }
    }

    public static ApiResponse DeserializeApiResponse(string json)
    {
      try
      {
        if (!(Json.Deserialize(json) is Dictionary<string, object> dict))
          return (ApiResponse) null;
        return new ApiResponse()
        {
          res = JsonHelper.GetString(dict, "res"),
          error = JsonHelper.GetString(dict, "error"),
          message = JsonHelper.GetString(dict, "message")
        };
      }
      catch (Exception ex)
      {
        return (ApiResponse) null;
      }
    }

    public static SelfResponse DeserializeSelfResponse(string json)
    {
      try
      {
        if (!(Json.Deserialize(json) is Dictionary<string, object> dict))
          return (SelfResponse) null;
        return new SelfResponse()
        {
          id = JsonHelper.GetString(dict, "id"),
          fn = JsonHelper.GetString(dict, "fn"),
          ln = JsonHelper.GetString(dict, "ln"),
          name = JsonHelper.GetString(dict, "name")
        };
      }
      catch (Exception ex)
      {
        return (SelfResponse) null;
      }
    }

    public static DialogsResponse DeserializeDialogsResponse(string json)
    {
      try
      {
        if (!(Json.Deserialize(json) is Dictionary<string, object> dictionary1))
          return (DialogsResponse) null;
        DialogsResponse dialogsResponse = new DialogsResponse()
        {
          dialogs = new List<Dialog>(),
          users = new Dictionary<string, User>(),
          chats = new Dictionary<string, Chat>()
        };
        if (dictionary1.ContainsKey("dialogs") && dictionary1["dialogs"] is List<object> objectList)
        {
          foreach (object obj in objectList)
          {
            if (obj is Dictionary<string, object> dict1)
            {
              Dialog dialog = new Dialog()
              {
                id = JsonHelper.GetString(dict1, "id"),
                unread = dict1.ContainsKey("unread") ? new int?(Convert.ToInt32(dict1["unread"])) : new int?()
              };
              if (dict1.ContainsKey("msg") && dict1["msg"] is Dictionary<string, object> dict)
                dialog.msg = new Message()
                {
                  id = JsonHelper.GetString(dict, "id"),
                  text = JsonHelper.GetString(dict, "text"),
                  date = dict.ContainsKey("date") ? Convert.ToInt32(dict["date"]) : 0,
                  peer_id = JsonHelper.GetString(dict, "peer_id"),
                  from_id = JsonHelper.GetString(dict, "from_id")
                };
              dialogsResponse.dialogs.Add(dialog);
            }
          }
        }
        if (dictionary1.ContainsKey("users") && dictionary1["users"] is Dictionary<string, object> dictionary2)
        {
          foreach (KeyValuePair<string, object> keyValuePair in dictionary2)
          {
            if (keyValuePair.Value is Dictionary<string, object> dict)
              dialogsResponse.users[keyValuePair.Key] = new User()
              {
                id = keyValuePair.Key,
                fn = JsonHelper.GetString(dict, "fn"),
                ln = JsonHelper.GetString(dict, "ln"),
                name = JsonHelper.GetString(dict, "name"),
                photo_url = JsonHelper.GetString(dict, "photo_url")
              };
          }
        }
        if (dictionary1.ContainsKey("chats") && dictionary1["chats"] is Dictionary<string, object> dictionary3)
        {
          foreach (KeyValuePair<string, object> keyValuePair in dictionary3)
          {
            if (keyValuePair.Value is Dictionary<string, object> dict)
              dialogsResponse.chats[keyValuePair.Key] = new Chat()
              {
                id = keyValuePair.Key,
                type = JsonHelper.GetString(dict, "type"),
                t = JsonHelper.GetString(dict, "t"),
                name = JsonHelper.GetString(dict, "name"),
                photo_url = JsonHelper.GetString(dict, "photo_url")
              };
          }
        }
        if (dictionary1.ContainsKey("count"))
          dialogsResponse.count = new int?(Convert.ToInt32(dictionary1["count"]));
        return dialogsResponse;
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Dialog parse error: " + ex.Message);
        return (DialogsResponse) null;
      }
    }

    public static MessagesResponse DeserializeMessagesResponse(string json)
    {
      try
      {
        if (!(Json.Deserialize(json) is Dictionary<string, object> dictionary1))
          return (MessagesResponse) null;
        MessagesResponse messagesResponse = new MessagesResponse()
        {
          messages = new List<Message>(),
          users = new Dictionary<string, User>(),
          chats = new Dictionary<string, Chat>()
        };
        if (dictionary1.ContainsKey("messages") && dictionary1["messages"] is List<object> objectList)
        {
          foreach (object obj in objectList)
          {
            if (obj is Dictionary<string, object> dict)
              messagesResponse.messages.Add(new Message()
              {
                id = JsonHelper.GetString(dict, "id"),
                text = JsonHelper.GetString(dict, "text"),
                date = dict.ContainsKey("date") ? Convert.ToInt32(dict["date"]) : 0,
                peer_id = JsonHelper.GetString(dict, "peer_id"),
                from_id = JsonHelper.GetString(dict, "from_id")
              });
          }
        }
        if (dictionary1.ContainsKey("users") && dictionary1["users"] is Dictionary<string, object> dictionary2)
        {
          foreach (KeyValuePair<string, object> keyValuePair in dictionary2)
          {
            if (keyValuePair.Value is Dictionary<string, object> dict)
              messagesResponse.users[keyValuePair.Key] = new User()
              {
                id = keyValuePair.Key,
                fn = JsonHelper.GetString(dict, "fn"),
                ln = JsonHelper.GetString(dict, "ln"),
                name = JsonHelper.GetString(dict, "name")
              };
          }
        }
        return messagesResponse;
      }
      catch (Exception ex)
      {
        return (MessagesResponse) null;
      }
    }

    private static string GetString(Dictionary<string, object> dict, string key)
    {
      return dict.ContainsKey(key) && dict[key] != null ? dict[key].ToString() : (string) null;
    }
  }
}
