﻿using System.Net;
using System.Threading;
using ServerCore;

namespace DummyClient
{
    class Program
    {

        static void Main(string[] args)
        {

            string host = "127.0.0.1";
            IPHostEntry iPHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = iPHost.AddressList[1];
            // 트래픽이 많이 몰리는 사이트는 주소를 여러개 둘 수 있기 때문에 배열로 사용
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Thread.Sleep(1000);
            Connector connector = new Connector();

            connector.Connect(endPoint, () => { return new ServerSession(); });


            while (true)
            {
            }


        }
    }
}