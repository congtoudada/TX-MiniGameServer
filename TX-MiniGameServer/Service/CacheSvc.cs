/****************************************************
  文件：CacheSvc.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月05日 11:49:34
  功能：
*****************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms.Internals;

namespace MiniGameServer
{
    public class CacheSvc : Singleton<CacheSvc> {
        //uid-session
        private readonly Dictionary<long, ServerSession> _onLineDict = new Dictionary<long, ServerSession>();
        public Dictionary<long, ServerSession> OnlineDict => _onLineDict;
        //uid-nickName:用于快速判断用户名是否存在(持久化)
        public ServerDiskData ServerDiskData { get; private set; } = null;
        
        public override void Init()
        {
            base.Init();
            if (File.Exists(ServerConfig.PlayerDatabase))
            {
                string json = File.ReadAllText(ServerConfig.PlayerDatabase, Encoding.UTF8);
                ServerDiskData = JsonConvert.DeserializeObject<ServerDiskData>(json);
            }
            else
            {
                ServerDiskData = new ServerDiskData();
            }
            this.Log("CacheSvc Init Done.");
        }

        public override void Update() {
            base.Update();
        }

        public bool IsAcctOnLine(long uid) {
            return _onLineDict.ContainsKey(uid);
        }

        public bool IsAcctOnLine(ServerSession session)
        {
            if (session == null)
                return false;
            // return session.Uid != 0;  // 不存在0的Uid，如果是就代表没登录
            return IsAcctOnLine(session.Uid);
        }

        public void SaveCacheData()
        {
            string json = JsonConvert.SerializeObject(ServerDiskData, Formatting.Indented);
            File.WriteAllText(ServerConfig.PlayerDatabase, json, Encoding.UTF8);
        }
        
        /// <summary>
        /// 注册失败返回-1，否则正常返回uid
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="nickName"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public long AcctOnline(long uid, string nickName, ServerSession session) {
            // 通过Nickname找真实uid
            var cacheData = Instance.ServerDiskData;
            if (cacheData.RegisterDict.ContainsKey(nickName))
            {
                uid = cacheData.RegisterDict[nickName];
            }
            if (Instance.IsAcctOnLine(uid))  // 已经在线
            {
                this.Warn($"[MsgHandler] Acct has online! uid: {uid}");
                // code = Result.HasOnline;
                // 给已在线用户发送踢下线协议并强制下线
                var onlineSession = Instance.GetSession(uid);
                onlineSession.SendMsg(new Pkg() {
                    Head = new Head
                    {
                        Uid = uid,
                        Seq = 1,
                        Cmd = Cmd.Kick,
                        Result = Result.Success
                    },
                    Body = new Body()
                    {
                        rspKick = new RspKick()
                    }
                });
                Instance.AcctOffline(onlineSession);  // 踢下线
            }
            // NickName不存在代表首次注册，随机生成uid
            if (!cacheData.RegisterDict.ContainsKey(nickName))
            {
                uid = ++cacheData.CurrentId;
                cacheData.RegisterDict.Add(nickName, uid);
                SaveCacheData();
                this.Log($"Acct register: {uid} {nickName}");
            }
            else
            {
                uid = cacheData.RegisterDict[nickName];
            }
            
            _onLineDict.Add(uid, session);
            this.Log($"Acct Online: {uid} {nickName}");
            return uid;
        }
        
        public void AcctOffline(ServerSession session)
        {
            if (session != null)
            {
                bool ret = RoomSvc.Instance.ExitRoom(session.Uid, session.PlayerSysData.RoomId);
                if (ret)
                {
                    RoomSvc.Instance.GetRoom(session.PlayerSysData.RoomId)?.BroadcastMatch(session.Uid);
                }
                _onLineDict.Remove(session.Uid);
                session.CloseSession();
                this.Log($"Acct Offline: {session.Uid} {session.PlayerSysData.Nickname}");
            }
        }

        public PlayerSysData GetPlayerTempData(long uid)
        {
            if (IsAcctOnLine(uid))
            {
                return _onLineDict[uid].PlayerSysData;
            }

            return null;
        }

        public ServerSession GetSession(long uid)
        {
            if (IsAcctOnLine(uid))
            {
                return _onLineDict[uid];
            }
            return null;
        }
    }
}