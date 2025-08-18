/****************************************************
  文件：KcpServer.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月04日 16:26:21
  功能：
*****************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PENet
{
    public class KcpServerDriver<T, K>
        where T : KcpSession<K>, new()
    {
        UdpClient udp;
        IPEndPoint remotePoint;
        
        private CancellationTokenSource cts;
        
        // 客户端Session字典
        public Dictionary<uint, T> SessionDic { get; private set; } = new Dictionary<uint, T>();
        
        public KcpServerDriver() {
            cts = new CancellationTokenSource();
        }
        
        public void StartAsServer(string ip, int port) {
            udp = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), port));
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }
            remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
            KcpLog.ColorLog(KcpLogColor.Green, "Server Start...");
            Task.Run(ServerReceive, cts.Token);
        }
        
        async void ServerReceive() {
            UdpReceiveResult result;
            while(true) {
                try {
                    if(cts.Token.IsCancellationRequested) {
                        KcpLog.ColorLog(KcpLogColor.Cyan, "SeverReceive Task is Cancelled.");
                        break;
                    }
                    result = await udp.ReceiveAsync();
                    uint sid = BitConverter.ToUInt32(result.Buffer, 0);
                    // 处理首次发送请求，规定响应的sid从第4位开始
                    if(sid == 0) {
                        sid = GenerateUniqueSessionID();
                        KcpLog.ColorLog(KcpLogColor.Cyan, $"Sever Listen Success!Gen sid:{sid}");
                        byte[] sid_bytes = BitConverter.GetBytes(sid);
                        byte[] conv_bytes = new byte[8];
                        Array.Copy(sid_bytes, 0, conv_bytes, 4, 4);
                        SendUDPMsg(conv_bytes, result.RemoteEndPoint);
                    }
                    else {
                        // 为新连接建立Session
                        if(!SessionDic.TryGetValue(sid, out T session)) {
                            // KcpLog.ColorLog(KcpLogColor.Magenta, $"Create Session! sid: {sid} remote:{result.RemoteEndPoint}");
                            session = new T();
                            session.InitSession(sid, SendUDPMsg, result.RemoteEndPoint);
                            session.OnSessionClose = OnServerSessionClose;
                            lock(SessionDic) {
                                SessionDic.Add(sid, session);
                            }
                        }
                        else {
                            session = SessionDic[sid];
                        }
                        session.ReceiveData(result.Buffer);
                    }
                }
                catch(Exception e) {
                    KcpLog.Warn("Server Udp Receive Data Exception:{0}", e.ToString());
                }
            }
        }
        
        void OnServerSessionClose(uint sid) {
            if(SessionDic.ContainsKey(sid)) {
                lock(SessionDic) {
                    SessionDic.Remove(sid);
                    KcpLog.Warn("Session:{0} remove from SessionDic.", sid);
                }
            }
            else {
                KcpLog.Error("Session:{0} cannot find in SessionDic", sid);
            }
        }
        public void CloseServer() {
            foreach(var item in SessionDic) {
                item.Value.CloseSession();
            }
            SessionDic = null;

            if(udp != null) {
                udp.Close();
                udp = null;
                cts.Cancel();
            }
        }
        
        void SendUDPMsg(byte[] bytes, IPEndPoint remotePoint) {
            if(udp != null) {
                udp.SendAsync(bytes, bytes.Length, remotePoint);
            }
        }
        public void BroadCastMsg(K msg) {
            byte[] bytes = KcpTool.Serialize<K>(msg);
            foreach(var item in SessionDic) {
                item.Value.SendMsg(bytes);
            }
        }
        private uint sid = 0;
        public uint GenerateUniqueSessionID() {
            lock(SessionDic) {
                while(true) {
                    ++sid;
                    if(sid == uint.MaxValue) {
                        sid = 1;
                    }
                    if(!SessionDic.ContainsKey(sid)) {
                        break;
                    }
                }
            }
            return sid;
        }
    }
}