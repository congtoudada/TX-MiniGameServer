/*************************************************
	作者: Plane
	邮箱: 1785275942@qq.com
	日期: 2021/02/25 16:03
	功能: 主线程处理网络包

    //=================*=================\\
           教学官网：www.qiqiker.com
           关注微信公众号: PlaneZhong
           关注微信服务号: qiqikertuts           
               ~~获取更多教学资讯~~
    \\=================*=================//
*************************************************/

using System;
using PENet;
using System.Collections.Generic;
using System.Reflection;
using PEUtils;

namespace MiniGameServer {
    public class MsgPack {
        public ServerSession Session;
        private readonly Pkg _pkg;
        public MsgPack(ServerSession session, Pkg pkg) {
            this.Session = session;
            this._pkg = pkg;
        }
        public Head Head => _pkg.Head;
        public Body Body => _pkg.Body;
    }

    public class NetSvc : Singleton<NetSvc> {
        public static readonly string PkgQueueLock = "PkgQueueLock";
        private KcpServerDriver<ServerSession, Pkg> server = new KcpServerDriver<ServerSession, Pkg>();
        private Queue<MsgPack> _msgPackQue = new Queue<MsgPack>();

        public override void Init() {
            base.Init();
            _msgPackQue.Clear();

            KcpLog.LogFunc = this.Log;
            KcpLog.WarnFunc = this.Warn;
            KcpLog.ErrorFunc = this.Error;
            KcpLog.ColorLogFunc = (color, msg) => {
                this.ColorLog((LogColor)color, msg);
            };

            server.StartAsServer(ServerConfig.Ip, ServerConfig.Port);
            this.Log("NetSvc Init Done.");
        }
        
        // 其他线程添加消息
        public void AddMsgQue(ServerSession session, Pkg pkg) {
            lock(PkgQueueLock) {
                _msgPackQue.Enqueue(new MsgPack(session, pkg));
            }
        }
        
        // 主线程处理消息
        public override void Update() {
            base.Update();

            if(_msgPackQue.Count > 0) {
                lock(PkgQueueLock) {
                    MsgPack msg = _msgPackQue.Dequeue();
                    HandoutMsg(msg);
                }
            }
        }

        //消息分发
        private void HandoutMsg(MsgPack pack)
        {
            try
            {
                // 分发消息
                string methodName = pack.Head.protoName;
                MethodInfo mi = null;
                if (methodName == "ReqJsonData")  // Json协议需嵌套处理
                {
                    methodName = pack.Body.reqJsonData.protoName;
                }
                methodName += "Handle";
                KcpLog.Log($"Receive proto: {methodName}:{pack.Session.GetRemotePoint()}");
                mi = typeof(MsgHandler).GetMethod(methodName); // 回调规则：协议名+Handle
                // Json协议解析后触发
                if (mi != null)
                {
                    object[] o = { pack };
                    mi.Invoke(null, o);
                }
                else
                {
                    this.Warn("[ 服务器 ] OnReceiveData Invoke fail " + methodName);
                    return;
                }
            }
            catch (Exception e)
            {
                this.Error($"[Exception] {e.GetType().Name}: {e.Message}");
                this.Error($"[StackTrace] {e.StackTrace ?? "No stack trace available"}");
            }
        }
    }
}
