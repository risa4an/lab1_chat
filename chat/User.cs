using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace chat
{
    public class User
    {
        public string userName;
        public string ip;
        public int port;

        public User(byte[] json)
        {
            string jsonStr = Encoding.Unicode.GetString(json);
            Dictionary<String, Object> values = JsonConvert.DeserializeObject<Dictionary<String, Object>>(jsonStr);
            userName = (string)values["userName"];
            ip = (string)values["ip"];
            port = (int)(long)values["port"];
        }

        public User(string _userName, string _ip, int _port)
        {
            userName = _userName;
            ip = _ip;
            port = _port;
        }

        public byte[] bytesToSend(int action)
        {
            Dictionary<String, Object> values = new Dictionary<String, Object>();
            values["action"] = action;
            values["user"] = this;
            return Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(values, Formatting.Indented));
        }

        public byte[] bytesToSend()
        {
            Dictionary<String, Object> values = new Dictionary<String, Object>();
            values["userName"] = userName;
            values["ip"] = ip;
            values["port"] = port;
            return Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(values, Formatting.Indented));
        }
    }
}
