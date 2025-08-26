/****************************************************
  文件：PlayerGameData.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月19日 11:14:34
  功能：每位玩家的局内数据
*****************************************************/

using System.Collections.Generic;
using System.Numerics;

namespace MiniGameServer
{
    public class PlayerGameData
    {
        public PlayerSysData SysData;  // 局外数据引用(掉线后为null)
        public ServerSession Session;  // 连接Session(掉线后为null)
        public bool IsMatchReady;  // 是否准备就绪

        public List<WeaponType> WeaponTypes; // 武器列表
        public bool IsGameReady;  // 装备结束，是否准备就绪

        public Vector3 Position;
    }
}