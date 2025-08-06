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
        [MsgHandle(Cmd.Ping)]
        public static void RspPingHandle(MsgPack pack)
        {
            KcpLog.Log("Client Handle RspPing!");
        }
    }

}
