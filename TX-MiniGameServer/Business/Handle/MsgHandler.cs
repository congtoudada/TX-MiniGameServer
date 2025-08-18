/****************************************************
	文件：MsgHandler.cs
	作者：聪头
	邮箱: 1322080797@qq.com
	日期：2021/02/14 22:38   	
	功能：定义消息回调
*****************************************************/
using System;
using PENet;

namespace MiniGameServer
{
    public partial class MsgHandler
    {
        [GameMessage(Cmd.Ping)]
        public static void ReqPingHandle(MsgPack pack)
        {
            // KcpLog.Log("Server Handle ReqPing!");
            var session = pack.Session as ServerSession;
            if (session == null) return;
            session.IsBeginPing = true; //开始校验Ping
            session.LastPingTime = NetSvc.GetTimeStamp();  // 更新Ping时间戳
            session.SendMsg(new Pkg() {
                Head = new Head
                {
                    Uid = session.Uid,
                    Seq = pack.Head.Seq + 1,
                    Cmd = Cmd.Ping,
                    Result = Result.Success
                },
                Body = new Body
                {
                    rspPing = new RspPing()
                    {
                        Timestamp = pack.Body.reqPing.Timestamp
                    }
                }
            });
            // KcpLog.Log($"Response - ReqPingHandle");
        }

        [GameMessage(Cmd.Login)]
        public static void ReqLoginHandle(MsgPack pack)
        {
            var req = pack.Body.reqLoginData;
            Result code = Result.Success;
            if (req == null)
            {
                KcpLog.Error("[MsgHandler] Protocol Error! reqLoginData is null!");
                return;
            }
            long uid = pack.Head.Uid;
            if (CacheSvc.Instance.IsAcctOnLine(uid))  // 已经在线
            {
                KcpLog.Warn($"[MsgHandler] Acct has online! uid: {uid}");
                code = Result.HasOnline;
            }
            else
            {
                uid = CacheSvc.Instance.AcctOnline(uid, req.Nickname, pack.Session as ServerSession);
                if (uid == -1) // 已经注册
                {
                    code = Result.HasBeenRegistered;
                }
            }
            // 缓存
            if (code == Result.Success)
            {
                if (pack.Session is ServerSession session)
                {
                    var data = session.PlayerTempData;
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
                    rspLoginData = new RspLogin()
                    {
                        Nickname = req.Nickname
                    }
                }
            });
            KcpLog.Log($"[MsgHandler] ReqLoginHandle: {code}");
        }
    }

}
