using System;
using System.Net;
using ServerCore;

namespace Server
{

    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            PacketManager.Instance.Register();

            string host = "127.0.0.1";
            IPHostEntry iPHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = iPHost.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


            _listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("Listening...");

            while (true)
            {
                ;

            }

        }
    }
}