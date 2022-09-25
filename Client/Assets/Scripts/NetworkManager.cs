using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using DummyClient;
using ServerCore;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();
    public void Send(ArraySegment<byte> sendBuff)
    {
        _session.Send(sendBuff);
    }

    // Start is called before the first frame update
    void Start()
    {
        string host = "127.0.0.1";
        IPHostEntry iPHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = iPHost.AddressList[1];
        // 트래픽이 많이 몰리는 사이트는 주소를 여러개 둘 수 있기 때문에 배열로 사용
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();

        connector.Connect(endPoint, () => { return _session; }, 1);
    }

    // Update is called once per frame
    void Update()
    {
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach(IPacket packet in list)
            PacketManager.Instance.HandlePacket(_session, packet);
        
    }
}
