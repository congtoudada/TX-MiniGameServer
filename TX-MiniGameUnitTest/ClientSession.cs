/****************************************************
  文件：ClientSession.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月05日 12:51:41
  功能：
*****************************************************/

using System;
using System.Linq;
using PENet;

namespace MiniGameServer
{
    public class ClientSession : KcpSession<Pkg>
    {
        protected override void OnConnected()
        {
            KcpLog.ColorLog(KcpLogColor.Green, "Client Online,Sid:{0}", m_sid);
        }
        
        protected override void OnReceiveMsg(Pkg msg)
        {
            KcpLog.ColorLog(KcpLogColor.Magenta, "Sid:{0},RcvClient,Name:{1}", m_sid, msg.Head.protoName);
            // Json协议
            string protoName = msg.Head.protoName;
            if (protoName == "RspJsonData")
            {
                if (msg.Body.rspJsonData != null)
                {
                    protoName = msg.Body.rspJsonData.protoName;
                }
            }
            protoName += "Handle";
            // 普通协议
            if (ClientMain.MsgListeners.ContainsKey(protoName))
            {
                ClientMain.MsgListeners[protoName]?.Invoke(msg);
            }
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