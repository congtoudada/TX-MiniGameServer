/****************************************************
  文件：KcpExtension.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月05日 17:49:39
  功能：
*****************************************************/

using Newtonsoft.Json;
using PENet;

namespace MiniGameServer
{
    public static class KcpServerExtension
    {
        public static void SendRspJsonMsg(this ServerSession session, JsonMsgBase obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            session.SendRspJsonMsg(obj.GetType().Name, json);
        }
        
        public static void SendRspJsonMsg(this ServerSession session, string name, string content)
        {
            Pkg pkg = new Pkg()
            {
                Head = new Head
                {
                    protoName = nameof(RspJsonData)
                },
                Body = new Body
                {
                    rspJsonData = new RspJsonData()
                    {
                        protoName = name,
                        Content = content
                    }
                }
            };
            session.SendMsg(pkg);
        }
    }
}