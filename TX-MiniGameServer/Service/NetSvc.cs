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
using System.Threading;
using System.Threading.Tasks;
using PEUtils;

namespace MiniGameServer {
    public class NetSvc : Singleton<NetSvc> {
        public static readonly string PkgQueueLock = "PkgQueueLock";
        public KcpServerDriver<ServerSession, Pkg> ServerDriver { get; private set; } = new KcpServerDriver<ServerSession, Pkg>();
        private readonly Queue<MsgPack> _msgPackQue = new Queue<MsgPack>(1024);
        private Dictionary<Cmd, MsgListener> _msgListeners = new Dictionary<Cmd, MsgListener>();
        private List<EventPack> _eventListeners = new List<EventPack>();
        private CancellationTokenSource cts;
        
        public override void Init() {
            base.Init();
            _msgPackQue.Clear();
            cts = new CancellationTokenSource();

            KcpLog.LogFunc = this.Log;
            KcpLog.WarnFunc = this.Warn;
            KcpLog.ErrorFunc = this.Error;
            KcpLog.ColorLogFunc = (color, msg) => {
                this.ColorLog((LogColor)color, msg);
            };
            RegisterHandlers();  //注册回调函数
            ServerDriver.StartAsServer(CommonConfig.InnerIp, CommonConfig.Port);
            Task.Run(DoTimer, cts.Token);
            this.Log("NetSvc Init Done.");
        }
        
        private void RegisterHandlers()
        {
            // 事件回调注册
            var handlerType1 = typeof(EventHandler);
            var methods1 = handlerType1.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var method in methods1)
            {
                var attr = method.GetCustomAttribute<EventMessageAttribute>();
                if (attr != null)
                {
                    var del = (Action)Delegate.CreateDelegate(typeof(Action), method);
                    _eventListeners.Add(new EventPack(del, attr.Interval, attr.Priority));
                }
            }
            _eventListeners.Sort();
            
            // 消息回调注册
            var handlerType2 = typeof(MsgHandler);
            var methods2 = handlerType2.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var method in methods2)
            {
                var attr = method.GetCustomAttribute<GameMessageAttribute>();
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
        
        //获得时间戳
        public static long GetTimeStamp(bool millisecond = true)
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            if (millisecond)
            {
                return Convert.ToInt64(ts.TotalMilliseconds);
            }
            else
            {
                return Convert.ToInt64(ts.TotalSeconds);
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

                if (_msgPackQue.Count > 10)
                {
                    this.Warn("Too Many Server Msg! Please Increase Tick!");
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
        
        //定时回调线程
        private async Task DoTimer()
        {
            this.Log("NetSvc Do Timer Start.");
            while (cts != null && !cts.IsCancellationRequested)
            {
                long now = GetTimeStamp();
                foreach (var item in _eventListeners)
                {
                    if (now - item.LastTime > item.Interval)
                    {
                        item.Callback();
                        item.LastTime = now;
                    }
                }
                await Task.Delay(ServerConfig.EventHandleFps, cts.Token);  // 约30fps
            }
        }
    }
}
