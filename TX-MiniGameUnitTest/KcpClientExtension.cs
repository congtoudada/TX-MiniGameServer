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
    public static class KcpClientExtension
    {
        public static void SendReqJsonMsg(this ClientSession session, JsonMsgBase obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            session.SendReqJsonMsg(obj.GetType().Name ,json);
        }
        
        public static void SendReqJsonMsg(this ClientSession session, string name, string json)
        {
            Pkg pkg = new Pkg()
            {
                Head = new Head
                {
                    protoName = nameof(ReqJsonData)
                },
                Body = new Body
                {
                    reqJsonData = new ReqJsonData()
                    {
                        protoName = name,
                        Content = json
                    }
                }
            };
            session.SendMsg(pkg);
        }
    }
}