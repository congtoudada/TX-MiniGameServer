/****************************************************
  文件：EventHandler.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月07日 12:09:44
  功能：服务器定时回调
*****************************************************/

using System.Collections.Generic;
using MiniGameServer;
using PENet;

namespace MiniGameServer
{
    public partial class EventHandler
    {
        [EventMessage(ServerConfig.PingQueryFrequency)]
        public static void PingEvent()
        {
            long now = NetSvc.GetTimeStamp();
            // 遍历每个Session更新心跳包
            foreach (var pair in NetSvc.Instance.ServerDriver.SessionDic)
            {
                var session = pair.Value;
                if (session.IsBeginPing)
                {
                    if (now - session.LastPingTime > ServerConfig.PingDestroyInterval)
                    {
                        KcpLog.Log("[EventHandler] Check Heart Ping Offline!");
                        // 已登录就走登录，否则走Session
                        if (CacheSvc.Instance.IsAcctOnLine(pair.Value))
                        {
                            CacheSvc.Instance.AcctOffline(pair.Value);
                        }
                        else
                        {
                            pair.Value.CloseSession();    
                        }
                    }
                }
            }
        }

        [EventMessage(ServerConfig.DestroyRoomFrequency)]
        public static void DestroyRoomEvent()
        {
            List<int> killIds = new List<int>();
            foreach (var pair in RoomSvc.Instance.Rooms)
            {
                if (pair.Value.RoomState == RoomState.Destroy)
                {
                    killIds.Add(pair.Key);
                }
            }
            for (int i = 0; i < killIds.Count; i++)
            {
                KcpLog.Log($"[EventHandler] Destroy Room: {killIds[i]}");
                RoomSvc.Instance.DestroyRoom(killIds[i]);
            }
        }

        [EventMessage(ServerConfig.EventHandleFps)]
        public static void BroadcastPlayerStatus()
        {
            foreach (var room in RoomSvc.Instance.Rooms.Values)
            {
                if (room.RoomState != RoomState.Game)
                    continue;
                Pkg pkg = new Pkg()
                {
                    Head = new Head()
                    {
                        Uid = room.GetOwnerUid(),
                        Cmd = Cmd.PlayerTick,
                        Result = Result.Success
                    },
                    Body = new Body()
                    {
                        rspPlayerTick = new RspPlayerTick()
                    }
                };
                List<RspPlayerTickItem> itemList = new List<RspPlayerTickItem>();
                foreach (var player in room.Players.Values)
                {
                    itemList.Add(new RspPlayerTickItem()
                    {
                        Uid = player.SysData.Uid,
                        Position = MsgHandler.VtoNetV(player.Position),
                        Rotation = MsgHandler.VtoNetV(player.Rotation),
                        Raftposition = MsgHandler.VtoNetV(player.RaftPosition),
                    });
                }
                pkg.Body.rspPlayerTick.itemLists.AddRange(itemList);
                room.Broadcast(pkg);
            }
        }
    }
}