/****************************************************
  文件：PlayerHandler.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月26日 15:00:06
  功能：
*****************************************************/

using MiniGameServer;

namespace MiniGameServer
{
    public partial class MsgHandler
    {
        #region PlayerTick
        [GameMessage(Cmd.PlayerTick)]
        public static void ReqPlayerTickHandle(MsgPack pack)
        {
            // 准备
            long uid = pack.GetUid();
            var req = pack.Body.reqPlayerTick;
            var data = RoomSvc.Instance.GetPlayer(uid);
            Room room = RoomSvc.Instance.GetRoomByUid(uid);
            // 记录
            data.Position = NetVtoV(req.Position);
            // data.Rotation = NetVtoV(req.Rotation);
            // 广播(写EventHandler，定时自动广播)
        }
        #endregion
        
        [GameMessage(Cmd.PlayerBullet)]
        public static void ReqPlayerBulletHandle(MsgPack pack)
        {
            // 取发包 uid
            long uid = pack.GetUid();
            var req = pack.Body.reqPlayerBullet;
            Room room = RoomSvc.Instance.GetRoomByUid(uid);

            // 广播给房间内所有人：某个玩家生成了一颗子弹
            Pkg pkg = new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,                  // 发消息的玩家 uid
                    Cmd = Cmd.PlayerBullet,     // 消息号
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspPlayerBullet = new RspPlayerBullet()
                    {
                        Uid = req.Uid,                  // 子弹的唯一ID
                        Position = req.Position,        // 初始位置
                        Dir = req.Dir,                  // 飞行方向
                        flyVelocity = req.flyVelocity,  // 飞行速度
                        lifeTime = req.lifeTime,        // 存活时间
                        bulletType = req.bulletType     // 子弹类型
                    }
                }
            };

            room.Broadcast(pkg);
        }
        
        [GameMessage(Cmd.Recoil)]
        public static void ReqPlayerRecoilHandle(MsgPack pack)
        {
            long uid = pack.GetUid();
            var req = pack.Body.reqPlayerRecoil;
            Room room = RoomSvc.Instance.GetRoomByUid(uid);

            // 广播给房间所有玩家：某个玩家触发了后坐力
            Pkg pkg = new Pkg()
            {
                Head = new Head()
                {
                    Uid = uid,
                    Cmd = Cmd.Recoil,
                    Result = Result.Success
                },
                Body = new Body()
                {
                    rspPlayerRecoil = new RspPlayerRecoil()
                    {
                        Uid = req.Uid,
                        Direction = req.Direction,
                        initialSpeed = req.initialSpeed,
                        recoilAcceleration = req.recoilAcceleration,
                        burstTime = req.burstTime,
                        slideVelocity = req.slideVelocity
                    }
                }
            };

            room.Broadcast(pkg);
        }
    }
}