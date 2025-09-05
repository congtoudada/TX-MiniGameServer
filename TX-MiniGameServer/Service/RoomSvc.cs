/****************************************************
  文件：RoomSvc.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月19日 11:07:07
  功能：房间类
*****************************************************/

using System;
using System.Collections.Generic;
using PENet;

namespace MiniGameServer
{
    public class RoomSvc : Singleton<RoomSvc>
    {
        public static int RoomNum = Int32.MaxValue; // 房间号
        public Dictionary<int, Room> Rooms = new Dictionary<int, Room>();

        // 玩家进入房间
        public int EnterRoom(long uid, int roomId = 0)
        {
            // 是否在其他房间，如果在则踢出
            var playerData = CacheSvc.Instance.GetPlayerTempData(uid);
            if (playerData != null)
            {
                if (Rooms.ContainsKey(playerData.RoomId))
                {
                    Rooms[playerData.RoomId].RemovePlayer(uid);
                }
            }
            
            if (roomId < 0)  // 创建房间
            {
                // 创建房间
                RoomNum = CreateRoom(uid);
                this.Log($"[RoomSvc] EnterRoom Success - 手动 创建房间:{RoomNum}");
                return RoomNum;
            }
            else if (roomId == 0)  // 由系统随机分配
            {
                // 找到第一个Ready的房间
                foreach (var pair in Rooms)
                {
                    Room room = pair.Value;
                    if (room.RoomState == RoomState.Ready && !room.IsFull())
                    {
                        this.Log($"[RoomSvc] EnterRoom Success - 系统分配 加入房间:{RoomNum}");
                        room.AddPlayer(uid);
                        return pair.Key;
                    }
                }
                RoomNum = CreateRoom(uid);
                this.Log($"[RoomSvc] EnterRoom Success - 系统分配 创建房间:{RoomNum}");
                return RoomNum;
            }
            else  // 加入房间
            {
                if (Rooms.ContainsKey(roomId))
                {
                    if (Rooms[roomId].RoomState != RoomState.Ready)
                    {
                        this.Warn($"[RoomSvc] EnterRoom Failed - 房间正在游戏中:{roomId}");
                        return -1;  // 房间已存在且非就绪态
                    }
                    if (Rooms[roomId].IsFull())
                    {
                        this.Warn($"[RoomSvc] EnterRoom Failed - 房间人数已满:{roomId}");
                        return -2;  // 房间满人
                    }
                    // 正常进入房间
                    this.Log($"[RoomSvc] EnterRoom Success - 手动加入房间成功:{roomId}");
                    Rooms[roomId].AddPlayer(uid);
                    return roomId;
                }
                return -3;  // 房间不存在
            }
        }

        private int CreateRoom(long uid)
        {
            // 没有Ready的房间就新建
            RoomNum = (RoomNum - 1) % Int32.MaxValue;
            if (RoomNum == 0) RoomNum = Int32.MaxValue;
            Room newRoom = new Room(RoomNum);
            newRoom.OnInit();
            Rooms.Add(RoomNum, newRoom);
            newRoom.AddPlayer(uid);
            return RoomNum;
        }
        
        // 玩家退出房间
        public bool ExitRoom(long uid, int roomId)
        {
            if (roomId <= 0) return false;
            if (Rooms.ContainsKey(roomId) && Rooms[roomId].RoomState == RoomState.Ready)
            {
                bool ret = Rooms[roomId].RemovePlayer(uid);
                if (ret)
                {
                    this.Log($"[RoomSvc] ExitRoom - 玩家:{uid} 退出房间:{roomId}");
                    return true;
                }
            }
            return false;
        }
        
        // 得到房间
        public Room GetRoom(int roomId)
        {
            if (Rooms.ContainsKey(roomId) && Rooms[roomId].RoomState != RoomState.Destroy)
            {
                return Rooms[roomId];
            }
            return null;
        }
        
        // 得到房间By Uid
        public Room GetRoomByUid(long uid)
        {
            var data = CacheSvc.Instance.GetPlayerTempData(uid);
            var roomId = data.RoomId;
            return GetRoom(roomId);
        }
        
        // 销毁房间(由EventHandler统一调用)
        public void DestroyRoom(int roomId)
        {
            if (Rooms.ContainsKey(roomId))
            {
                Rooms[roomId].OnRelease();
                Rooms.Remove(roomId);
            }
        }
        
        // GetPlayer
        public PlayerGameData GetPlayer(long uid)
        {
            var room = GetRoomByUid(uid);
            if (room != null)
            {
                return room.GetPlayer(uid);
            }

            return null;
        }
        
        public override void Update() {
            base.Update();
        }
    }
}