/****************************************************
  文件：EventHandler.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月07日 12:09:44
  功能：服务器定时回调
*****************************************************/

using MiniGameServer;
using PENet;

namespace MiniGameServer
{
    public partial class EventHandler
    {
        [EventMessage(ServerConfig.PingQueryFrequency)]
        public static void PingEvent()
        {
            long now = NetSvc.GetTimeStamp();
            // 遍历每个Session更新心跳包
            foreach (var pair in NetSvc.Instance.ServerDriver.SessionDic)
            {
                var session = pair.Value;
                if (session.IsBeginPing)
                {
                    if (now - session.LastPingTime > ServerConfig.PingDestroyInterval)
                    {
                        KcpLog.Log("[EventHandler] Check Heart Ping Offline!");
                        // 已登录就走登录，否则走Session
                        if (CacheSvc.Instance.IsAcctOnLine(pair.Value))
                        {
                            CacheSvc.Instance.AcctOffline(pair.Value);
                        }
                        else
                        {
                            pair.Value.CloseSession();    
                        }
                    }
                }
            }
        }
    }
}