using System;
using System.Collections.Generic;

namespace Server
{
    public class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            lock(_lock)
            {
                foreach (ClientSession s in _sessions)
                    s.Send(segment);
            }
        }


        // Enter와 Leave는 멀티스레드 - 동시다발적으로 일어남
        public void Enter(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Remove(session);
            }   
        }


    }
}
