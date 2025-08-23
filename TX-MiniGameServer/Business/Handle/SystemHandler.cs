/****************************************************
  文件：SystemHandler.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月20日 15:53:26
  功能：
*****************************************************/

 using PENet;

namespace MiniGameServer
{
    public partial class MsgHandler
    {
        #region Login
        [GameMessage(Cmd.Login)]
        public static void ReqLoginHandle(MsgPack pack)
        {
            var req = pack.Body.reqLogin;
            Result code = Result.Success;
            if (req == null)
            {
                KcpLog.Error("[MsgHandler] Protocol Error! reqLogin is null!");
                return;
            }
            long uid = pack.Head.Uid;
            uid = CacheSvc.Instance.AcctOnline(uid, req.Nickname, pack.Session as ServerSession);
            if (uid == -1) // 已经注册
            {
                code = Result.HasBeenRegistered;
            }
            // 缓存
            if (code == Result.Success)
            {
                if (pack.Session is ServerSession session)
                {
                    var data = session.PlayerSysData;
                    data.Uid = uid;
                    data.Nickname = req.Nickname;
                }
            }
            // 响应
            pack.Session.SendMsg(new Pkg() {
                Head = new Head
                {
                    Uid = uid,
                    Seq = pack.Head.Seq + 1,
                    Cmd = Cmd.Login,
                    Result = code
                },
                Body = new Body()
                {
                    rspLogin = new RspLogin()
                    {
                        Nickname = req.Nickname
                    }
                }
            });
            KcpLog.Log($"[MsgHandler] ReqLoginHandle: {code}");
        }
        #endregion
        
        #region Match
        [GameMessage(Cmd.Match)]
        public static void ReqMatchHandle(MsgPack pack)
        {
            var req = pack.Body.reqMatch;
            Result code = Result.Success;
            int roomId;
            Room room = null;
            long uid = pack.Head.Uid;
            if (!req.isReady)  // 匹配开始，进入房间
            {
                roomId = RoomSvc.Instance.EnterRoom(uid, req.roomId);
                if (roomId == -1)
                {
                    code = Result.RoomBusy;
                }
                else if (roomId == -2)
                {
                    code = Result.RoomFull;
                }
            }
            else  // 已经处于某个房间，选择就绪
            {
                roomId = pack.Body.reqMatch.roomId;
                room = RoomSvc.Instance.GetRoom(roomId);
                room.GetPlayer(uid).IsMatchReady = true;
                // 如果所有人都就绪，进入对局...
                KcpLog.Log($"[MsgHandler] ReqMatchHandle - All Ready! Start Game!");
            }
            
            // 进入房间成功就广播
            room ??= RoomSvc.Instance.GetRoom(roomId);
            if (code == Result.Success)
            {
                Pkg pkg = new Pkg()
                {
                    Head = new Head
                    {
                        Uid = uid,
                        Seq = pack.Head.Seq + 1,
                        Cmd = Cmd.Match,
                        Result = code
                    },
                    Body = new Body()
                    {
                        rspMatch = new RspMatch()
                        {
                            roomId = roomId,
                        }
                    }
                };
                pkg.Body.rspMatch.matchInfoLists.AddRange(room.GetMatchInfoList());
                room.Broadcast(pkg);
            }
            else  // 进入房间失败就单发
            {
                Pkg pkg = new Pkg()
                {
                    Head = new Head
                    {
                        Uid = uid,
                        Seq = pack.Head.Seq + 1,
                        Cmd = Cmd.Match,
                        Result = code
                    },
                    Body = new Body()
                    {
                        rspMatch = new RspMatch()
                        {
                            roomId = roomId,
                        }
                    }
                };
                pkg.Body.rspMatch.matchInfoLists.AddRange(room.GetMatchInfoList());
                pack.Session.SendMsg(pkg);
            }
        }

        #endregion
        
        #region ExitGame
        [GameMessage(Cmd.ExitGame)]
        public static void ReqExitGameHandle(MsgPack pack)
        {
            var session = pack.Session as ServerSession;
            // 已登录就走登录，否则走Session
            if (CacheSvc.Instance.IsAcctOnLine(session))
            {
                CacheSvc.Instance.AcctOffline(session);
            }
            else
            {
                session?.CloseSession();    
            }
        }
        #endregion
    }
}