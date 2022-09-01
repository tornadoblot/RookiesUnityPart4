using System;
using System.Net;
using System.Threading;
using ServerCore;

namespace Server
{

    class Packet
    {
        public ushort size;
        public ushort packetid;
    }

    // 클라가 서버에게 특정 플레이어의 정보 요청
    class PlayerInfoReq : Packet
    {
        public long playerId;
    }

    // 서버에게 받은 플레이어 정보
    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    // 컨텐츠 단에서 하는게 아니라 안에서 하게 만들기
    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            /*Packet packet = new Packet() { size = 100, packetid = 10 };


            // send는 왜 외부에서 비트를 만들어서 보낼까
            // 그 이유는 성능 이슈가 있어서 그렇다
            // 내부에다 만들면 버퍼를 만번씩 복사를 해야 한다
            // 버퍼 사이즈라도 정할 수 있을까? 8바이트 옮기는데 1024바이트는 비효율적이니까
            // 그치만 할 수 없는 이유는 보내는 정보에 가변 정보(리스트 )이 있을 수 있기 때문이다
            // 대신 버퍼 하나 큰 거를 할당하고 야금야금 잘라서 쓰게 만들면 좋다

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            byte[] buffer = BitConverter.GetBytes(packet.size);
            byte[] buffer2 = BitConverter.GetBytes(packet.packetid);
            Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);



            Send(sendBuff);*/
            Thread.Sleep(5000);
            DisConnect();

        }

        public override void OnDiscoonnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        long playerId = BitConverter.ToInt64(buffer.Array, buffer.Offset + count);
                        count += 8;
                        Console.WriteLine($"PlayerInfoReq: {playerId}");
                    }
                    break;

            }

            Console.WriteLine($"RecvPacket Id: {id}, Size: {size}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transffered bytes: {numOfBytes}");

        }
    }
}