using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace chat
{
    partial class Program
    {
        class Message
        {
            static string text;
            static string user;

            public Message (byte[] json)
            {
                string jsonStr = Encoding.Unicode.GetString(json);
                Dictionary<String, Object> values = JsonConvert.DeserializeObject<Dictionary<String, Object>>(jsonStr);
                text = (string)values["text"];
                user = (string)values["user"];
            }

            public Message(string usr, string txt)
            {
                text = txt;
                user = usr;
            }

            public byte[] bytesToSend()
            {
                Dictionary<String, Object> values = new Dictionary<String, Object>();
                values["text"] = text;
                values["user"] = user;
                return Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(values, Formatting.Indented));
            }

            public string Display()
            {
                return user + ": " + text;
            }
        }
    }
}