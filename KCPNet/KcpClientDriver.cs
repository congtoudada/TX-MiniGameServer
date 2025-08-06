/****************************************************
  文件：KcpClientDriver.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月04日 16:26:52
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
    public class KcpClientDriver<T, K>
        where T : KcpSession<K>, new()
    {
        UdpClient udp;
        IPEndPoint remotePoint;
        
        private CancellationTokenSource cts;
        private CancellationToken ct;
        
        public T clientSession;
        
        public KcpClientDriver() {
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }
        
        /// <summary>
        /// 启动客户端
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void StartAsClient(string ip, int port) {
            udp = new UdpClient(0);  // 0表示由操作系统指定端口
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }
            remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
            KcpLog.ColorLog(KcpLogColor.Green, "Client Start...");
            Task.Run(ClientReceive, ct);  // 客户端接收数据循环
        }
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="maxintervalSum"></param>
        /// <returns></returns>
        public Task<bool> ConnectServer(int interval, int maxintervalSum = 5000) {
            SendUDPMsg(new byte[4], remotePoint);
            int checkTimes = 0;
            Task<bool> task = Task.Run(async () => {
                while(true) {
                    await Task.Delay(interval);
                    checkTimes += interval;
                    if(clientSession != null && clientSession.IsConnected()) {
                        return true;
                    }
                    else {
                        if(checkTimes > maxintervalSum) {
                            return false;
                        }
                    }
                }
            });
            return task;
        }
        
        /// <summary>
        /// 将远端数据送入kcp解析
        /// </summary>
        async void ClientReceive() {
            UdpReceiveResult result;
            while(true) {
                try {
                    if(ct.IsCancellationRequested) {
                        KcpLog.ColorLog(KcpLogColor.Cyan, "ClientReceive Task is Cancelled.");
                        break;
                    }
                    result = await udp.ReceiveAsync();

                    if(Equals(remotePoint, result.RemoteEndPoint)) {
                        uint sid = BitConverter.ToUInt32(result.Buffer, 0);
                        if(sid == 0) {
                            //sid 数据
                            if(clientSession != null && clientSession.IsConnected()) {
                                //已经建立连接，初始化完成了，收到了多的sid,直接丢弃。
                                KcpLog.Warn("Client is Init Done,Sid Surplus.");
                            }
                            else {
                                //未初始化，收到服务器分配的sid数据，初始化一个客户端session
                                sid = BitConverter.ToUInt32(result.Buffer, 4);
                                KcpLog.ColorLog(KcpLogColor.Green, "UDP Request Conv Sid:{0}", sid);

                                //会话初始化
                                clientSession = new T();
                                clientSession.InitSession(sid, SendUDPMsg, remotePoint);
                                clientSession.OnSessionClose = OnClientSessionClose;
                            }
                        }
                        else {
                            //处理业务逻辑(把远端数据传入kcp)
                            if(clientSession != null && clientSession.IsConnected()) {
                                clientSession.ReceiveData(result.Buffer);
                            }
                            else { 
                                //没初始化且sid!=0，数据消息提前到了，直接丢弃消息，直到初
                                //始化完成，kcp重传再开始处理。
                                KcpLog.Warn("Client is Initing...");
                            }
                        }
                    }
                    else {
                        KcpLog.Warn("Client Udp Receive illegal target Data.");
                    }
                }
                catch(Exception e) {
                    KcpLog.Warn("Client Udp Receive Data Exception:{0}", e.ToString());
                }
            }
        }
        
        void SendUDPMsg(byte[] bytes, IPEndPoint remotePoint) {
            if(udp != null) {
                udp.SendAsync(bytes, bytes.Length, remotePoint);
            }
        }
        
        void OnClientSessionClose(uint sid) {
            cts.Cancel();
            if(udp != null) {
                udp.Close();
                udp = null;
            }
            KcpLog.Warn("Client Session Close,sid:{0}", sid);
        }
        
        public void CloseClient() {
            if(clientSession != null) {
                clientSession.CloseSession();
            }
        }
    }
}