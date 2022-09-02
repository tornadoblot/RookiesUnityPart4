using System;
using System.Net;
using System.Text;
using ServerCore;

namespace DummyClient
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetid;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    // 클라가 서버에게 특정 플레이어의 정보 요청
    class PlayerInfoReq : Packet
    {
        public long playerId;

        public PlayerInfoReq()
        {
            this.packetid = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> s)
        {

            ushort count = 0;

            // 요까지 왔다는 건 이미 패킷이 PlayerInfoReq인걸 알고 있다는 거
            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
            count += 2;
            //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += 2;


            this.playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(s.Array, s.Offset + count, s.Count - count));
            // 서버는 항상 클라가 거짓말 하고 있다고 생각해야함
            // 패킷 헤더에 있는 정보는 참고만 하자
            // 그래서 데이터를 읽으려고 할때도 유효 범위만 읽어야 하니까 리드온리 스판으로 슈슈슉 하는거임
            // 근데 패킷에서 사이즈 정보는 왜 보내냐? 그래도 반은 믿어주는거임
            count += 8;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> s = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.packetid);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.playerId);
            count += 8;

            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);
            if (success == false)
                return null;


            return SendBufferHelper.Close(count);
        }
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

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001 };


            ArraySegment<byte> s = packet.Write();

            if (s != null)
                Send(s);

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
