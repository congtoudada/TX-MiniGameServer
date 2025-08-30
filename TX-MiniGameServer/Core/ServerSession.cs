/****************************************************
  文件：ServerSession.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月04日 16:50:07
  功能：每个客户端连接对应一个Session（子线程执行）
*****************************************************/

using System;
using System.Collections.Generic;
using PENet;

namespace MiniGameServer
{
    public class ServerSession : KcpSession<Pkg>
    {
        public long Uid => PlayerSysData.Uid;
        public bool IsBeginPing = false;
        public long LastPingTime = 0;

        private readonly HashSet<Cmd> _logBlack = new HashSet<Cmd>()
        {
            Cmd.Ping,
            // Cmd.PlayerTick,
            // Cmd.SyncPosMonster,
            // Cmd.PlayerBullet,
            // Cmd.Recoil
        };

        public PlayerSysData PlayerSysData { get; } = new PlayerSysData();

        protected override void OnConnected()
        {
            KcpLog.ColorLog(KcpLogColor.Green, $"Client Online,Sid:{m_sid} {m_remotePoint}");
        }
        
        protected override void OnReceiveMsg(Pkg msg)
        {
            if (msg == null)
            {
                KcpLog.ColorLog(KcpLogColor.Red, "Sid:{0} Receive null!", m_sid);
                return;
            }

            if (!_logBlack.Contains(msg.Head.Cmd))
            {
                KcpLog.ColorLog(KcpLogColor.Magenta, "Sid:{0}, RcvReq Cmd:{1}", m_sid, msg.Head.Cmd);
            }
            NetSvc.Instance.AddMsgQue(this, msg);  // 由主线程NetSvc处理
        }
        
        protected override void OnUpdate(DateTime now) 
        {

        }

        protected override void OnDisConnected()
        {
            KcpLog.Warn("Client Offline, Sid:{0}", m_sid);
        }
    }
}