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
        public static void ReqPingHandle(MsgPack pack)
        {
            KcpLog.Log("Server Handle ReqPing!");
            pack.Session.SendMsg(new Pkg() {
                Head = new Head { protoName = nameof(RspPing) }
            });
        }
    }

}
