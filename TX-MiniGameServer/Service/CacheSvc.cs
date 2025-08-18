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
            // NickName不存在代表首次注册，随机生成uid
            var cacheData = Instance.ServerDiskData;
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
                session.CloseSession();
                _onLineDict.Remove(session.Uid);
                this.Log($"Acct Offline: {session.Uid} {session.PlayerTempData.Nickname}");
            }
        }

        public PlayerTempData GetPlayerTempData(long key)
        {
            if (IsAcctOnLine(key))
            {
                return _onLineDict[key].PlayerTempData;
            }

            return null;
        }
    }
}