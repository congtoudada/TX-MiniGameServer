/****************************************************
  文件：CacheSvc.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月05日 11:49:34
  功能：
*****************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniGameServer
{
    public class CacheSvc : Singleton<CacheSvc> {
        //acct-session
        private Dictionary<string, ServerSession> onLineAcctDic;
        //seesion-userdata
        private Dictionary<ServerSession, PlayerData> onLineSessionDic;

        public override void Init() {
            base.Init();
            onLineAcctDic = new Dictionary<string, ServerSession>();
            onLineSessionDic = new Dictionary<ServerSession, PlayerData>();

            this.Log("CacheSvc Init Done.");
        }

        public override void Update() {
            base.Update();
        }

        public bool IsAcctOnLine(string acct) {
            return onLineAcctDic.ContainsKey(acct);
        }

        public void AcctOnline(string acct, ServerSession session, PlayerData playerData) {
            onLineAcctDic.Add(acct, session);
            onLineSessionDic.Add(session, playerData);
        }
    }
}