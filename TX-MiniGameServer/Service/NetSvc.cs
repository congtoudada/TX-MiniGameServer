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
    public class NetSvc : Singleton<NetSvc> {
        public static readonly string PkgQueueLock = "PkgQueueLock";
        private readonly KcpServerDriver<ServerSession, Pkg> _server = new KcpServerDriver<ServerSession, Pkg>();
        private readonly Queue<MsgPack> _msgPackQue = new Queue<MsgPack>();
        private Dictionary<Cmd, MsgListener> _msgListeners = new Dictionary<Cmd, MsgListener>();

        public override void Init() {
            base.Init();
            _msgPackQue.Clear();

            KcpLog.LogFunc = this.Log;
            KcpLog.WarnFunc = this.Warn;
            KcpLog.ErrorFunc = this.Error;
            KcpLog.ColorLogFunc = (color, msg) => {
                this.ColorLog((LogColor)color, msg);
            };
            RegisterMsgHandlers();  //注册回调函数
            _server.StartAsServer(ServerConfig.Ip, ServerConfig.Port);
            this.Log("NetSvc Init Done.");
        }
        
        private void RegisterMsgHandlers()
        {
            var handlerType = typeof(MsgHandler);  // 定义消息处理函数的类
            var methods = handlerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<MsgHandleAttribute>();
                if (attr != null)
                {
                    var del = (MsgListener)Delegate.CreateDelegate(typeof(MsgListener), method);
                    _msgListeners[attr.Cmd] = del;
                }
            }
        }
        
        // 其他线程添加消息
        public void AddMsgQue(ServerSession session, Pkg pkg) {
            lock(PkgQueueLock) {
                _msgPackQue.Enqueue(new MsgPack(session, pkg));
            }
        }
        
        // 主线程处理消息
        public override void Update()
        {
            base.Update();
            lock (PkgQueueLock)
            {
                if(_msgPackQue.Count > 0) {
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
                Cmd cmd = pack.Head.Cmd;
                if (_msgListeners.ContainsKey(cmd))
                {
                    _msgListeners[cmd]?.Invoke(pack);
                }
                else
                {
                    this.Warn($"Server Handler not found cmd: {cmd}");
                }
                #region 反射分发（弃用）
                // // 分发消息
                // string methodName = pack.Head.protoName;
                // MethodInfo mi = null;
                // if (methodName == "ReqJsonData")  // Json协议需嵌套处理
                // {
                //     methodName = pack.Body.reqJsonData.protoName;
                // }
                // methodName += "Handle";
                // KcpLog.Log($"Receive proto: {methodName}:{pack.Session.GetRemotePoint()}");
                // mi = typeof(MsgHandler).GetMethod(methodName); // 回调规则：协议名+Handle
                // // Json协议解析后触发
                // if (mi != null)
                // {
                //     object[] o = { pack };
                //     mi.Invoke(null, o);
                // }
                // else
                // {
                //     this.Warn("[ 服务器 ] OnReceiveData Invoke fail " + methodName);
                //     return;
                // }
                #endregion
            }
            catch (Exception e)
            {
                this.Error($"[Exception] {e.GetType().Name}: {e.Message}");
                this.Error($"[StackTrace] {e.StackTrace ?? "No stack trace available"}");
            }
        }
    }
}
