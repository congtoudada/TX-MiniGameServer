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
            // 广播(写EventHandler，定时自动广播)
        }
        #endregion
    }
}