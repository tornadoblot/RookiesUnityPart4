﻿using System;
using System.Collections.Generic;
using ServerCore;

namespace Server
{
    public class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
               
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            _pendingList.Add(segment);
            
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }


        // Enter와 Leave는 멀티스레드 - 동시다발적으로 일어남
        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
            
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
            
        }
    }
}
