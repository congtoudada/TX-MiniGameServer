/****************************************************
  文件：MonsterHandler.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月24日 15:32:48
  功能：
*****************************************************/


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
            KcpLog.Log($"[MsgHandler] ReqGenMonsterHandle Success Handle GenType: {req.genType} ActionId: {req.actionId}");

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
                        Damage = req.Damage,
                    }
                }
            };
            room.Broadcast(pkg);
        }
    }
}