using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;

namespace Server
{


	// 컨텐츠 단에서 하는게 아니라 안에서 하게 만들기
	class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");
            Thread.Sleep(5000);
            DisConnect();

        }

        public override void OnDiscoonnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);


        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transffered bytes: {numOfBytes}");

        }
    }
}