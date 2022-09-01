using System;
using System.Net;
using System.Text;
using ServerCore;

namespace DummyClient
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
    class ServerSession : Session
    {
        /*
        // unsafe를 사용하면 c#에선 건들지 못했던 포인터를 건들 수 있게됨
        static unsafe void ToBytes(byte[] array, int offset, ulong value)
        {
            fixed (byte* ptr = &array[offset])
                *(ulong*)ptr = value;
        }*/


        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { packetid = (ushort)PacketID.PlayerInfoReq, playerId = 1001 };


            // 보내기

            //for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = SendBufferHelper.Open(4096);

                // 바이트 배열을 만들면 내부적으로는 동적 배열을 만들게 되니까 불-편
                // 그럼 BitConverter.TryWriteBytes(new Span<byte>(byte[] array, int start, int length) 써보자

                ushort count = 0;
                bool success = true;


                // 놀라운 사실 ㄴㅇㄱ 사실 패킷의 사이즈는 패킷 포장이 전부 끝날 때 까지 알 수 없었던 거임 ㄷ
                //success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), packet.size);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), packet.packetid);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), packet.playerId);
                count += 8;

                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);


                ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

                if (success)
                    Send(sendBuff);

            }

        }

        public override void OnDiscoonnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }



        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transffered bytes: {numOfBytes}");

        }
    }
}
