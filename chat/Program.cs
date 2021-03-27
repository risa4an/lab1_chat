using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace chat
{
    partial class Program
    {
        static string lobbyAdress = "224.1.1.1"; // адресс лобби
        static int lobbyPort = 11080;
        static int userPort = 11002; 
        static int groupPort = 11003;
        static string userName;
        static string groupIp = null;
        static User myUser;
        static List<User> users;

        static void Main(string[] args)
        {
            try
            {
                Console.Write("Введите имя: ");
                userName = Console.ReadLine();
                myUser = new User(userName, LocalIPAddress(), userPort);

                Thread mesThread = new Thread(new ThreadStart(ReceiveMessage));
                mesThread.Start();


                Thread lobbyThread = new Thread(new ThreadStart(ReceiveNewConnections));
                lobbyThread.Start();

                Console.WriteLine("1 - создать новую группу\n2 - отправить запрос на вступление");
                int action = Int32.Parse(Console.ReadLine());
                users = new List<User>();

                if (action == 1)
                {
                    Random rnd = new Random();
                    groupIp = "224.0.0." + (int)rnd.Next(0, 255);

                    Thread groupThread = new Thread(new ThreadStart(ReceiveGroupMessages));
                    groupThread.Start();

                    SendMessage();
                }
                else
                {

                    Console.WriteLine("Выберите, к кому присоединиться");

                    UdpClient sender = new UdpClient(); // создаем UdpClient для отправки
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(lobbyAdress), lobbyPort);
                    sender.Send(myUser.bytesToSend(), myUser.bytesToSend().Length, endPoint);
                    sender.Close();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SendMessage()
        {
            UdpClient sender = new UdpClient(); 
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(groupIp), groupPort);
            try
            {
                while (true)
                {
                    string message = Console.ReadLine(); 
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Message mes = new Message(userName, message);
                    byte[] data = mes.bytesToSend();
                    sender.Send(data, data.Length, endPoint); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }


        private static void ReceiveNewConnections()
        {
            UdpClient receiver = new UdpClient(lobbyPort); 
            receiver.JoinMulticastGroup(IPAddress.Parse(lobbyAdress));
            IPEndPoint remoteIp = null;
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); 
                    User user = new User(data);
                    if (groupIp != null)
                    {
                        UdpClient sender = new UdpClient(); 
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(user.ip), user.port);
                        sender.Send(myUser.bytesToSend(0), myUser.bytesToSend(0).Length, endPoint);
                        sender.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        private static void ReceiveMessage()
        {
            UdpClient receiver = new UdpClient(userPort); // UdpClient для получения данных
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); 
                    string jsonStr = Encoding.Unicode.GetString(data);
                    Dictionary<String, Object> values = JsonConvert.DeserializeObject<Dictionary<String, Object>>(jsonStr);
                    int action = (int)(long)values["action"];

                    if (action == 0)
                    {
                        User usr = JsonConvert.DeserializeObject<User>(values["user"].ToString());
                        users.Add(usr);
                        Console.WriteLine(usr.userName);

                        string user = Console.ReadLine();
                        User groupHost = users.Find(x => x.userName == user);

                        UdpClient sender = new UdpClient(); 
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(groupHost.ip), groupHost.port);
                        sender.Send(myUser.bytesToSend(1), myUser.bytesToSend(1).Length, endPoint);
                        sender.Close();
                    }
                    else if (action == 1)
                    {
                        User usr = JsonConvert.DeserializeObject<User>(values["user"].ToString());
                        Console.WriteLine("Это я, {0}, можно к вам ? ", usr.userName);
                        string reaction = Console.ReadLine();
                        if (reaction == "Y")
                        {
                            UdpClient sender = new UdpClient();
                            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(usr.ip), usr.port);
                            Dictionary<String, Object> responce = new Dictionary<String, Object>();
                            responce["action"] = 2;
                            responce["groupIp"] = groupIp;
                            byte[] bytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(responce, Formatting.Indented));
                            sender.Send(bytes, bytes.Length, endPoint);
                            sender.Close();
                        }
                    }
                    else if (action == 2)
                    {
                        groupIp = (String)values["groupIp"];
                        Thread groupThread = new Thread(new ThreadStart(ReceiveGroupMessages));
                        groupThread.Start();

                        SendMessage();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        private static void ReceiveGroupMessages()
        {
            UdpClient receiver = new UdpClient(groupPort); 
            receiver.JoinMulticastGroup(IPAddress.Parse(groupIp));
            IPEndPoint remoteIp = null;
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp);
                    Message mes = new Message(data);
                    Console.WriteLine(mes.Display());

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        private static string LocalIPAddress()
        {
            string localIP = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}