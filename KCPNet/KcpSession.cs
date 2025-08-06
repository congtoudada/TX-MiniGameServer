using System;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Threading;
using System.Threading.Tasks;

namespace PENet {
    public enum SessionState {
        None,
        Connected,
        DisConnected
    }

    public abstract class KcpSession<T>
    {
        protected uint m_sid;  // Session ID
        Action<byte[], IPEndPoint> m_udpSender;  //UDP发送器委托
        protected IPEndPoint m_remotePoint;  // 目的端地址
        protected SessionState m_sessionState = SessionState.None;  //连接状态
        public Action<uint> OnSessionClose;  // Session关闭触发的回调

        public KcpHandle m_handle;  // Kcp收发句柄
        public Kcp m_kcp;  // kcp核心
        private CancellationTokenSource cts;
        private CancellationToken ct;

        public void InitSession(uint sid, Action<byte[], IPEndPoint> udpSender, IPEndPoint remotePoint) {
            m_sid = sid;
            m_udpSender = udpSender;
            m_remotePoint = remotePoint;
            m_sessionState = SessionState.Connected;

            m_handle = new KcpHandle();
            m_kcp = new Kcp(sid, m_handle);
            m_kcp.NoDelay(1, 10, 2, 1);
            m_kcp.WndSize(64, 64);
            m_kcp.SetMtu(512);

            m_handle.Out = (Memory<byte> buffer) => {
                byte[] bytes = buffer.ToArray();
                m_udpSender(bytes, m_remotePoint);  //通过udp发送kcp封装的数据
            };

            m_handle.Recv = (byte[] buffer) => {
                buffer = KcpTool.DeCompress(buffer);
                T msg = KcpTool.DeSerialize<T>(buffer);
                if(msg != null) {
                    OnReceiveMsg(msg);
                }
            };

            OnConnected();

            cts = new CancellationTokenSource();
            ct = cts.Token;
            Task.Run(Update, ct);
        }
        public void ReceiveData(byte[] buffer) {
            m_kcp.Input(buffer.AsSpan());
        }
        
        public void SendMsg(T msg) {
            if(IsConnected()) {
                byte[] bytes = KcpTool.Serialize(msg);
                if(bytes != null) {
                    SendMsg(bytes);
                }
            }
            else {
                KcpLog.Warn("Session Disconnected.Can not send msg.");
            }
        }
        public void SendMsg(byte[] msg_bytes) {
            if(IsConnected()) {
                msg_bytes = KcpTool.Compress(msg_bytes);
                m_kcp.Send(msg_bytes.AsSpan());
            }
            else {
                KcpLog.Warn("Session Disconnected.Can not send msg.");
            }
        }
        public void CloseSession() {
            cts.Cancel();
            OnDisConnected();

            OnSessionClose?.Invoke(m_sid);
            OnSessionClose = null;

            m_sessionState = SessionState.DisConnected;
            m_remotePoint = null;
            m_udpSender = null;
            m_sid = 0;

            m_handle = null;
            m_kcp = null;
            cts = null;
        }
        
        /// <summary>
        /// 对kcp解析后的数据进行使用
        /// </summary>
        async void Update() {
            try {
                while(true) {
                    DateTime now = DateTime.UtcNow;
                    OnUpdate(now);
                    if(ct.IsCancellationRequested) {
                        KcpLog.ColorLog(KcpLogColor.Cyan, "SessionUpdate Task is Cancelled.");
                        break;
                    }
                    else {
                        m_kcp.Update(now);
                        int len;
                        while((len = m_kcp.PeekSize()) > 0) {
                            var buffer = new byte[len];
                            if(m_kcp.Recv(buffer) >= 0) {
                                m_handle.Receive(buffer);
                            }
                        }
                        await Task.Delay(10, ct);
                    }
                }
            }
            catch(Exception e) {
                KcpLog.Warn("Session Update Exception:{0}", e.ToString());
            }
        }

        protected abstract void OnUpdate(DateTime now);
        protected abstract void OnConnected();
        protected abstract void OnReceiveMsg(T msg);
        protected abstract void OnDisConnected();


        public override bool Equals(object obj) {
            if(obj is KcpSession<T>) {
                KcpSession<T> us = obj as KcpSession<T>;
                return m_sid == us.m_sid;
            }
            return false;
        }
        public override int GetHashCode() {
            return m_sid.GetHashCode();
        }
        public uint GetSessionID() {
            return m_sid;
        }

        public bool IsConnected() {
            return m_sessionState == SessionState.Connected;
        }

        public IPEndPoint GetRemotePoint()
        {
            return m_remotePoint;
        }
    }
}
