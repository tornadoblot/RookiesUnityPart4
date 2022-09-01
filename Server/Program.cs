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

            // DNS(Domain Name System) 사용 예정
            // www.google.com -> 123.123.132.12

            string host = "127.0.0.1";
            IPHostEntry iPHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = iPHost.AddressList[1];
            // 트래픽이 많이 몰리는 사이트는 주소를 여러개 둘 수 있기 때문에 배열로 사용
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 문지기가 들고 있는 핸드폰 만들어주기

            _listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine("Listening...");

            while (true)
            {


            }
            // 주 스레드와 작업자 스레드가 있어서 콜백함수는 별도의 스레드에서 실행 가능
            // 메인에선 반복문만 돌고 있지만 멀티 스레드로 돌고 있다는 뜻 - 같은 데이터를 건들인다면 경합 조건이 일어날 가능성 존재

        }
    }
}