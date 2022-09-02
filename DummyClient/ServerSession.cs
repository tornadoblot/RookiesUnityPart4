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

    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string name;
        // 가변적 크기를 갖는 스트링 주고받기

        public PlayerInfoReq()
        {
            this.packetid = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {

            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            count += sizeof(ushort);


            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            // string
            // nameLen 도 공격감지해야됨
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            // trywrite 할때마다 뉴 스판 하면 불편하니까 슬라이스로 변경
            // 슬라이스하면 리턴으로 결과값 뱉어줘서 가능

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetid);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);

            // string utf16을 써보자 C#은 utf16을 기본으로 사용하기도 함
            // C++ 은 문자열 끝이 0x 00 00 으로 끝나지만 C#은 그러지 않음
            // string len[2] -> byte[] 이렇게 ushort로 사이즈를 먼저 보내주고 스트링 쓰기
            // 그치만.. 문자열의 길이를 구하는데 string.length를 쓰면 byte배열의 크기랑 안맞게 돼버림(ABCD는 문자열로 4바이트 byte배열로는 8바이트)
            /// ushort nameLen = (ushort) Encoding.Unicode.GetByteCount(this.name);
            // 이러면 되지롱~
            /// success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            /// count += sizeof(ushort);
            // Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
            // 싸늘하니까 츄라이문으로 바꾸기
            // 먼저 꺼는 길이 파악 -> 복사 방식인데 이번엔 냅따 복사 후에 사이즈 적어주기
            ushort nameLen = (ushort) Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(s, count);


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


    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name="ABCD"};


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
