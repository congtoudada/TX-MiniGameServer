/****************************************************
  文件：PlayerTempData.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月11日 11:16:53
  功能：每位玩家的局外数据
*****************************************************/

using System;

namespace MiniGameServer
{
    /// <summary>
    /// 服务器玩家内存数据
    /// </summary>
    public class PlayerSysData
    {
        public long Uid = 0;
        public string Nickname = "";
        public int RoomId;  // 是否处于房间内 (>0)
    }
}