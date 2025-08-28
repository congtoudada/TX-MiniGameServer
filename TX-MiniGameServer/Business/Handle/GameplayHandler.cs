/****************************************************
  文件：MsgHandler_Gp.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月20日 15:51:59
  功能：
*****************************************************/

using System.Collections.Generic;
using PENet;
using Xamarin.Forms.Internals;

namespace MiniGameServer
{
    public partial class MsgHandler
    {
        public static void BroadcastRoom(long uid, Pkg pkg)
        {
            var room = RoomSvc.Instance.GetRoomByUid(uid);
            if (room != null)
            {
                room.Broadcast(pkg);
            }
        }
        
        #region Equip
        [GameMessage(Cmd.Equip)]
        public static void ReqEquipHandle(MsgPack pack)
        {
            // 准备
            long uid = pack.GetUid();
            ReqEquip req = pack.Body.reqEquip;
            var data = RoomSvc.Instance.GetPlayer(uid);
            Room room = RoomSvc.Instance.GetRoomByUid(uid);
            // 记录
            if (req.isSelect)
            {
                // 判断武器是否已经被选择了
                foreach (var item in room.Players.Values)
                {
                    if (item.WeaponTypes.Contains(req.Weapon))
                    {
                        return;
                    }
                }
                data.WeaponTypes.Add(req.Weapon);
            }
            else
            {
                data.WeaponTypes.Remove(req.Weapon);
            }
            // 广播
            Pkg pkg = new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.Equip,
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspEquip = new RspEquip()
                    {
                        Nickname = data.SysData.Nickname,
                        Weapon = req.Weapon,
                        isSelect = req.isSelect
                    }
                }
            };
            BroadcastRoom(uid, pkg);
        }
        #endregion

        #region StartGame
        [GameMessage(Cmd.StartGame)]
        public static void ReqStartGameHandle(MsgPack pack)
        {
            long uid = pack.GetUid();
            ReqStartGame req = pack.Body.reqStartGame;
            var data = RoomSvc.Instance.GetPlayer(uid);
            // 记录
            data.IsGameReady = req.isReady;
            // 广播
            Room room = RoomSvc.Instance.GetRoomByUid(uid);
            List<long> rspList = new List<long>();
            foreach (var item in room.Players.Values)
            {
                if (item.IsGameReady)
                {
                    rspList.Add(item.SysData.Uid);
                }
            }
            
            Pkg pkg = new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.Equip,
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspStartGame = new RspStartGame()
                    {
                        readyLists = rspList.ToArray()
                    }
                }
            };
            BroadcastRoom(uid, pkg);
        }
        #endregion
        
        #region GetProp
        [GameMessage(Cmd.GetProp)]
        public static void ReqGetPropHandle(MsgPack pack)
        {
            // 准备
            long uid = pack.GetUid();
            var req = pack.Body.reqGetProp;
            var data = RoomSvc.Instance.GetPlayer(uid);
            Room room = RoomSvc.Instance.GetRoomByUid(uid);
            KcpLog.Log($"[ReqGetPropHandle] PlayerUid: {uid} Get PropId: {req.propId} PropType: {req.propType}");
            // 记录
            room.Broadcast(new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.GetProp,
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspGetProp = new RspGetProp()
                    {
                        propId = req.propId,
                    }
                }
            });
        }
        #endregion
    }
}