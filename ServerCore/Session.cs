using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // sealed는 자식이 오버라이드 하려는 거 막아주는 거
        // [size(2)][packetId(2)][...][size(2)][packetId(2)][...]
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLen = 0;

            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // 여기까지 왔응면 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                // 이제 패킷 아이디 따라 스위치 케이스 문으로 쭉 분기해서 사용 가능

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }


            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDiscoonnected(EndPoint endPoint);

        void Clear()
        {
            lock(_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }


        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);


            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        // 유저 한개한개의 패킷을 따로 따로 보내느냐
        // 유저 한개한개의 패킷을 쫙 모아서 크게 보내느냐
        // 컨텐츠 단에서 할지 서버단에서 할지 정해야함
        public void Send(ArraySegment<byte> sendBuff)
        {

            lock (_lock)
            {
                // send 할때마다 이벤트를 계속 만드는 건 비효율적
                // 멀티스레드에서 잘 돌아가기 위해 큐에다 버퍼를 저장하고 OnSendCompleted가 완료되면 큐에 남은 걸 다시 실행
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0) // 내가 1등이면 레지스터 센드 호출 
                {
                    RegisterSend();
                }
            }
        }

        public void DisConnect()
        {
            // 멀티 스레드 환경에서 세션 디스커넥트를 두번 하면 문제가 발생
            // 인터락을 실제로 활용해 변수가 하나씩만 증가하게 해주고 두번 하지 못하게 변경
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDiscoonnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        #region 네트워크통신

        void RegisterSend()
        {

            if (_disconnected == 1)
                return;


            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
                // ArraySegment -> 어레이의 일부
                // struct라 힙영역이 아닌 스택 영역
                // Addbuff말고 BufferList로 한번에 추가해주기
            }

            _sendArgs.BufferList = _pendingList;
            // 이렇게 리스트를 따로 대입해줘야함

            try
            {

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
            }
            catch(Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                        {
                            // 큐에 값이 있으면 다시한번 보내주기
                            RegisterSend();
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    DisConnect();
                }
            }

        }

        void RegisterRecv()
        {
            if (_disconnected == 1)
                return;
                

            // 받을 수 있는 버퍼 크기 계산
            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);


            try
            {

                bool pending = _socket.ReceiveAsync(_recvArgs);

                if (pending == false)
                {
                    OnRecvCompleted(null, _recvArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Register Recv Failed {e}");
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 소켓이 닫혔을때 데이터 크기가 0인 버퍼를 보낼 수 있
            // 받는 데이터가 0이 아니고 소켓 에러가 발생하지 않을때
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                // TODO

                try
                {
                    // write 커서 이동 오류날일은 없지만 일단 디스커넥트 때림
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        DisConnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받
                    int processLen = OnRecv(_recvBuffer.ReadSegment);

                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        DisConnect();
                        return;
                    }

                    // read cursor move
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        DisConnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                // TODO Disconnect
                DisConnect();
                // 이벤트가 하나이기 때문에 그냥 디스커넥트 때려도 됨
            }
        }
        #endregion

    }
}