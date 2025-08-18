/****************************************************
  文件：EventPack.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月07日 12:21:55
  功能：
*****************************************************/

using System;

namespace MiniGameServer
{
    public class EventPack : IComparable<EventPack>
    {
        public Action Callback;
        public int Interval;
        public long LastTime;
        public int Priority;

        public EventPack(Action callback, int interval, int priority)
        {
            Callback = callback;
            Interval = interval;
            Priority = priority;
        }

        public int CompareTo(EventPack other)
        {
            // Priority 大的排前面（降序）
            return other.Priority.CompareTo(this.Priority);
        }
    }
}