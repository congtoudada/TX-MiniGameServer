/****************************************************
  文件：BossHandler.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月30日 21:12:05
  功能：
*****************************************************/

namespace MiniGameServer
{
    public partial class MsgHandler
    {
        #region BossAction
        [GameMessage(Cmd.BossAction)]
        public static void ReqBossActionHandle(MsgPack pack)
        {
            // 准备
            long uid = pack.GetUid();
            var req = pack.Body.reqBossAction;
            var data = RoomSvc.Instance.GetPlayer(uid);
            Room room = RoomSvc.Instance.GetRoomByUid(uid);
            if (room.GetOwnerUid() != uid)
                return;
            // 记录
            room.Broadcast(new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.BossAction,
                    Result = Result.Success,
                },
                Body = new Body()
                {
                    rspBossAction = new RspBossAction()
                    {
                        stateId = req.stateId,
                        Target = req.Target,
                        settingIdx = req.settingIdx
                    }
                }
            });
        }
        #endregion
        
        #region BossAction
        [GameMessage(Cmd.AttackBoss)]
        public static void ReqAttackBossHandle(MsgPack pack)
        {
            // 准备
            long uid = pack.GetUid();
            var req = pack.Body.reqAttackBoss;
            var data = RoomSvc.Instance.GetPlayer(uid);
            Room room = RoomSvc.Instance.GetRoomByUid(uid);
            if (room.GetOwnerUid() != uid)
                return;
            // 记录
            room.Broadcast(new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.AttackBoss,
                    Result = Result.Success,
                },
                Body = new Body()
                {
                    rspAttackBoss = new RspAttackBoss()
                    {
                        Hp = req.Hp
                    }
                }
            });
        }
        #endregion
    }
}