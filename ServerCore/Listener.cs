using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;


        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            // 문지기 교육 시켜주기(주소 연동)-Bind
            _listenSocket.Bind(endPoint);

            // 영업 시작
            _listenSocket.Listen(backlog);
            // backlog: 최대 대기수(몇 명 대기가 가능한가)

            for (int i = 0; i < register; i++)
            {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            // 콜백되면 할 이벤트 핸들러 추가하고 초기화 해주
            RegisterAccept(args);
            }
            // 반복문을 통해 문지기를 여러명 둬서 부하를 줄일 수 있/
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            // 두번째 반복 부터는 args에 이미 소켓이 들어가 있기 때문에 초기화 해줘야함

            bool pending = _listenSocket.AcceptAsync(args);
            // 에이싱크가 붙었으면 비동기(논블로킹) 라는 뜻
            // 일이 성공하든 아니든 리턴을 하고봄
            // 접속을 완료하면 알려주는 형식 - 콜백 방식으로 연락
            // accept 요청 / 실제 완료 두개를 구현해야함

            // 실행하는 동시에 클라이언트가 들어온 경우
            if (pending == false)
            {
                OnAcceptCompleted(null, args);
                // if문에서 계속 걸린다면 재귀적 호출로 스택 오버플로우가 일어날 수 있지 않을까?
                // 이론상으로는 가능하지만 누가 악의적으로 공격하지 않는 이상 일어날 수 없는 일임
            }
        }

        // -- 레드존
        void OnAcceptCompleted(Object sender, SocketAsyncEventArgs args)
        {
            // 에러 없이 Success 해도 에러의  범주에 포함되기 때문에
            if (args.SocketError == SocketError.Success)
            {
                // TODO
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);

            }
            else
                Console.WriteLine(args.SocketError.ToString());

            RegisterAccept(args);
            // 다음에 들어올 클라이언트를 위해 다시 등록
        }
    }
}