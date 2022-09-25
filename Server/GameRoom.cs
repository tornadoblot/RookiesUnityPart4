using System;
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

        public void Broadcast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
            
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            //Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }


        // Enter와 Leave는 멀티스레드 - 동시다발적으로 일어남
        public void Enter(ClientSession session)
        {
            // 플레이어 추가 
            _sessions.Add(session);
            session.Room = this;

            // 다른 모든 플레이어 목록 전송
            S_PlayerList players = new S_PlayerList();

            foreach(ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX, 
                    posY = s.PosY, 
                    posZ = s.PosZ,
                });               
            }

            session.Send(players.Write());

            // 플레이어 입장 알림
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());

            
        }

        public void Leave(ClientSession session)
        {
            // 플레이어 제거
            _sessions.Remove(session);

            // 다른 플레이어에게 알림
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
            
        }

        public void Move(ClientSession session, C_Move packet)
        {
            // 플레이어 좌표 알려주기
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            // 모두에게 알리기
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;
            Broadcast(move.Write());
        }
    }
}
