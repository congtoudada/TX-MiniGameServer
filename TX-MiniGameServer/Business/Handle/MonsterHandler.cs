/****************************************************
  文件：MonsterHandler.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月24日 15:32:48
  功能：
*****************************************************/


using System.Collections.Generic;
using System.Numerics;
using System.Security.Principal;
using PENet;

namespace MiniGameServer
{
    public partial class MsgHandler
    {
        public static long mid = 1;
        
        #region GenMonster
        [GameMessage(Cmd.GenMonster)]
        public static void ReqGenMonsterHandle(MsgPack pack)
        {
            // 准备
            long uid = pack.GetUid();
            var req = pack.Body.reqGenMonster;
            var data = RoomSvc.Instance.GetPlayer(uid);
            Room room = RoomSvc.Instance.GetRoomByUid(uid);
            // 记录
            if (room.TriggeredActions.Contains(req.actionId))
            {
                return;  // 已经触发过的动作不再生效
            }
            KcpLog.Log($"[MsgHandler] ReqGenMonsterHandle Success Handle GenType: {req.genType} ActionId: {req.actionId} monsterIdx: {req.monsterIdx}");

            mid++; // 

            MonsterData monsData = new MonsterData(100, NetVtoV(req.genPos), NetVtoV(req.genRotate));
            
            room.Monsters.Add(mid, monsData);

            room.TriggeredActions.Add(req.actionId);
            // 广播
            Pkg pkg = new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.GenMonster,
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspGenMonster = new RspGenMonster()
                    {
                        actionId = req.actionId,
                        monsterIdx = req.monsterIdx,
                        
                        genCount = 1,//req.genCount,
                        genPos = req.genPos,
                        genRotate = req.genRotate,
                        mId = mid,
                    }
                }
            };
            room.Broadcast(pkg);
        }
        #endregion
        
        #region AttackMonster
        [GameMessage(Cmd.AttackMonster)]
        public static void ReqAttackMonsterHandle(MsgPack pack)
        {
            // 准备
            long uid = pack.GetUid();
            var req = pack.Body.reqAttackMonster;
            Room room = RoomSvc.Instance.GetRoomByUid(uid);
            
            // 广播
            Pkg pkg = new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.AttackMonster,
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspAttackMonster = new RspAttackMonster()
                    {
                        mId = req.mId,
                        Dir = req.Dir,
                        Dis = req.Dis,
                        Hp = req.Hp,
                        Sp = req.Sp,
                        Duration = req.Duration
                    }
                }
            };
            room.Broadcast(pkg);
        }
        #endregion
        
        #region SyncPosMonster

        [GameMessage(Cmd.SyncPosMonster)]
        public static void ReqSyncPosMonsterHandle(MsgPack pack)  // 仅限房主发送
        {
            // 准备
            long uid = pack.GetUid();
            var req = pack.Body.reqSyncPosMonster;
            var data = RoomSvc.Instance.GetPlayer(uid);
            Room room = RoomSvc.Instance.GetRoomByUid(uid);

            if (uid != room.GetOwnerUid())
            {
                KcpLog.Warn($"ReqSyncPosMonsterHandle仅限房主发送!: {uid} ownerUid: {room.GetOwnerUid()}");
                return;
            }

            Pkg pkg = new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.SyncPosMonster,
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspSyncPosMonster = new RspSyncPosMonster()
                    {
                        Timestamp = req.Timestamp,
                    }
                }
            };
            List<RspSyncPosMonsterItem> rspList = new List<RspSyncPosMonsterItem>();
            foreach (var item in req.reqLists)
            {
                if (item.uId == 0) continue;  // 没有索敌的怪不考虑
                PlayerGameData pData = room.GetPlayer(item.uId);
                Vector3 pos = MsgHandler.NetVtoV(item.Pos);
                Vector3 target = pData.Position;
                Vector3 dir = target - pos;
                dir.Y = 0;
                dir = Vector3.Normalize(dir);
                // Vector3 move = dir * item.Speed * ServerConfig.Tick / 1000.0f;
                Vector3 move = dir * item.Speed * ServerConfig.Tick / 1000.0f;
                Vector3 targetPos = pos + move;

                RspSyncPosMonsterItem rspItem = new RspSyncPosMonsterItem()
                {
                    targetPos = VtoNetV(targetPos),
                    mId = item.mId,
                    uId = item.uId
                };
                rspList.Add(rspItem);
            }
            pkg.Body.rspSyncPosMonster.rspLists.AddRange(rspList);
            room.Broadcast(pkg);
        }
        #endregion
        
        #region FindTarget

        [GameMessage(Cmd.MonsterFindTarget)]
        public static void ReqMonsterFindTargetHandle(MsgPack pack)
        {
            // 测试:只锁房间第一个玩家
            // 准备
            long uid = pack.GetUid();
            var req = pack.Body.reqFindTarget;
            var data = RoomSvc.Instance.GetPlayer(uid);
            Room room = RoomSvc.Instance.GetRoomByUid(uid);
            
            Pkg pkg = new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.MonsterFindTarget,
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspFindTarget = new RspFindTarget()
                    {
                        mId = req.mId,
                        Uid = room.GetOwnerUid()
                    }
                }
            };
            room.Broadcast(pkg);
        }
        #endregion
        
        #region Dash
        [GameMessage(Cmd.MonsterDash)]
        public static void ReqMonsterDashHandle(MsgPack pack)
        {
            // 准备
            long uid = pack.GetUid();
            var req = pack.Body.reqMonsterDash;
            var data = RoomSvc.Instance.GetPlayer(uid);
            Room room = RoomSvc.Instance.GetRoomByUid(uid);

            var player = room.GetPlayer(req.uId);
            if (player != null)
            {
                var playerPos = player.Position;
                var start = NetVtoV(req.Pos);
                var dir = playerPos - start;
                var target = start + Vector3.Normalize(dir) * req.Dis;
                target.Y = 0;
                Pkg pkg = new Pkg()
                {
                    Head = new Head()
                    {
                        Uid = uid,
                        Cmd = Cmd.MonsterDash,
                        Result = Result.Success
                    },
                    Body = new Body()
                    {
                        rspMonsterDash = new RspMonsterDash()
                        {
                            mId = req.mId,
                            Target = VtoNetV(target)
                        }
                    }
                };
                room.Broadcast(pkg);
            }
        }
        
        #endregion
    }
}