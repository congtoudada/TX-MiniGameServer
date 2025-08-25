/****************************************************
  文件：Room.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月19日 11:12:47
  功能：
*****************************************************/

using System;
using System.Collections.Generic;

namespace MiniGameServer
{
    public enum RoomState
    {
        Ready = 0,  // 准备阶段
        Game = 1,  // 游戏中
        Destroy = 2, //待销毁
    }

    public class Room
    {
        // public class GenActionPackage
        // {
        //     public long Uid;
        //     public long LastSendTimestamp;
        //     public ReqGenMonster Req;
        // }
        
        public Dictionary<long, PlayerGameData> Players = new Dictionary<long, PlayerGameData>();  // 房间内玩家列表
        public RoomState RoomState = RoomState.Ready;
        public HashSet<int> TriggeredActions = new HashSet<int>();
        
        public Dictionary<long, MonsterData> Monsters = new Dictionary<long, MonsterData>();  // 怪物列表
        // public List<GenActionPackage> GenMonsterCache = new List<GenActionPackage>();
        public int RoomId;

        public Room(int roomId)
        {
            RoomId = roomId;
        }
        
        /// <summary>
        /// 房间初始化
        /// </summary>
        public void OnInit()
        {
            this.Log($"[Room] OnInit ");
        }
        
        /// <summary>
        /// 房间销毁
        /// </summary>
        public void OnRelease()
        {
            this.Log($"[Room] OnRelease ");
        }

        public PlayerGameData GetPlayer(long uid)
        {
            return Players.GetValueOrDefault(uid);
        }

        public void AddPlayer(long uid)
        {
            if (!Players.ContainsKey(uid))
            {
                PlayerGameData gameData = new PlayerGameData
                {
                    SysData = CacheSvc.Instance.GetPlayerTempData(uid),
                    Session = CacheSvc.Instance.GetSession(uid)
                };
                Players.Add(uid, gameData);
                Players[uid].SysData.RoomId = RoomId;
            }
        }

        public bool RemovePlayer(long uid)
        {
            if (Players.ContainsKey(uid))
            {
                Players[uid].SysData.RoomId = 0;
                Players.Remove(uid);
                // 解散~
                if (Players.Count == 0)
                {
                    RoomState = RoomState.Destroy;
                }
            }
            return true;
        }

        public int GetCount()
        {
            return Players.Count;
        }

        public bool IsFull()
        {
            return Players.Count == ServerConfig.RoomMaxCount;
        }

        public void Broadcast(Pkg pkg)
        {
            foreach (var gameData in Players.Values)
            {
                var session = gameData.Session;
                if (session != null)
                {
                    session.SendMsg(pkg);
                }
            }
        }
        
        public List<MatchInfo> GetMatchInfoList()
        {
            List<MatchInfo> infoList = new List<MatchInfo>(Players.Count);
            if (RoomState == RoomState.Ready)
            {
                foreach (var player in Players.Values)
                {
                    if (player.SysData != null)
                    {
                        infoList.Add(new MatchInfo()
                        {
                            Nickname = player.SysData.Nickname,
                            isReady = player.IsMatchReady
                        });
                    }
                }
            }
            return infoList;
        }

        public void BroadcastMatch(long uid)
        {
            if (Players.Count == 0) return;
            Pkg pkg = new Pkg()
            {
                Head = new Head
                {
                    Uid = uid,
                    Seq = 0,
                    Cmd = Cmd.Match,
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspMatch = new RspMatch()
                    {
                        roomId = RoomId,
                    }
                }
            };
            pkg.Body.rspMatch.matchInfoLists.AddRange(GetMatchInfoList());
            Broadcast(pkg);
        }
    }
}