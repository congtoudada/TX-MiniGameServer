/****************************************************
  文件：ServerBusinessConfig.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月11日 11:41:59
  功能：
*****************************************************/

namespace MiniGameServer
{
    public static class ServerConfig
    {
        public const int Tick = (int)(1 / 100.0f * 1000);  // 服务器Tick
        public const int EventHandleFps = (int)(1 / 20.0f * 1000);  // 消息处理帧率 30fps
        public const string PlayerDatabase = "CachePlayerData.json";  // 用户持久化数据
        public const int RoomMaxCount = 4;  // 房间最大人数
    
        /** ------------------------- 协议相关 ------------------------- **/
        public const int PingQueryFrequency = 10 * 1000;  // n s轮询一次失效心跳
        public const int PingDestroyInterval = 300 * 1000; //客户端n s没有心跳就销毁
        public const int DestroyRoomFrequency = 10 * 1000;  // n s轮询一次销毁房间
        public const int BroadcastStatusFrequency = (int)(1.0f / 40.0f) * 1000;  // 处理同步玩家状态帧率
    }
}