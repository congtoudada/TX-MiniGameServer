/****************************************************
  文件：ServerSession.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月04日 16:50:07
  功能：每个客户端连接对应一个Session
*****************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using PENet;

namespace MiniGameServer
{

    
    public class ClientMain
    {
        static KcpClientDriver<ClientSession, Pkg> client;
        static Task<bool> checkTask = null;
        public static Dictionary<Cmd, MsgListener> MsgListeners = new Dictionary<Cmd, MsgListener>();
        static void Main(string[] args)
        {
            client = new KcpClientDriver<ClientSession, Pkg>();
            client.StartAsClient(ServerConfig.Ip, ServerConfig.Port);
            RegisterMsgHandlers();
            checkTask = client.ConnectServer(200, 5000);
            Task.Run(ConnectCheck);
            
            while(true) {
                string ipt = Console.ReadLine();
                if(ipt == "quit") {
                    client.CloseClient();
                    break;
                }
                else {
                    // TODO: 请求测试
                    
                }
            }
            Console.ReadKey();
        }
        private static int counter = 0;
        static async void ConnectCheck() {
            while(true) {
                await Task.Delay(3000);
                if(checkTask != null && checkTask.IsCompleted) {
                    if(checkTask.Result) {
                        KcpLog.ColorLog(KcpLogColor.Green, "ConnectServer Success.");
                        checkTask = null;
                        await Task.Run(SendPingMsg);  // 建立连接立刻发送Ping请求
                        break;
                    }
                    else {
                        ++counter;
                        if(counter > 4) {
                            KcpLog.Error(string.Format("Connect Failed {0} Times,Check Your Network Connection.", counter));
                            checkTask = null;
                            break;
                        }
                        else {
                            KcpLog.Warn(string.Format("Connect Faild {0} Times.Retry...", counter));
                            checkTask = client.ConnectServer(200, 5000);
                        }
                    }
                }
            }
        }
        
        
        private static void RegisterMsgHandlers()
        {
            var handlerType = typeof(MsgHandler);  // 定义消息处理函数的类
            var methods = handlerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<MsgHandleAttribute>();
                if (attr != null)
                {
                    var del = (MsgListener)Delegate.CreateDelegate(typeof(MsgListener), method);
                    MsgListeners[attr.Cmd] = del;
                }
            }
        }

        static async void SendPingMsg() {
            while(true) {
                if(client != null && client.clientSession != null) {
                    client.clientSession.SendMsg(new Pkg() {
                        Head = new Head { Cmd = Cmd.Ping }
                    });
                    KcpLog.ColorLog(KcpLogColor.Green, "Client Send Ping Message.");
                }
                else {
                    KcpLog.ColorLog(KcpLogColor.Green, "Ping Task Cancel");
                    break;
                }
                await Task.Delay(5000);
            }
        }
    }
}