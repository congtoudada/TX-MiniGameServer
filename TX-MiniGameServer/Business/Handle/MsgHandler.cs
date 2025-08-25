/****************************************************
	文件：MsgHandler.cs
	作者：聪头
	邮箱: 1322080797@qq.com
	日期：2021/02/14 22:38   	
	功能：定义消息回调
*****************************************************/
using System;
using System.Numerics;
using PENet;

namespace MiniGameServer
{
    public partial class MsgHandler
    {
        #region Ping
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
        #endregion

        public static Vector3 NetVtoV(NetVector3 netV)
        {
            return new Vector3(netV.X, netV.Y, netV.Z);
        }
        
        public static NetVector3 VtoNetV(Vector3 v)
        {
            NetVector3 NetV = new NetVector3();
            NetV.X = v.X;
            NetV.Y = v.Y;
            NetV.Z = v.Z;

            return NetV;
        }
    }
}
