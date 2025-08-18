/****************************************************
  文件：EventMessageAttribute.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月07日 12:12:35
  功能：
*****************************************************/

using System;

namespace MiniGameServer
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventMessageAttribute : Attribute
    {
        /// <summary>
        /// 调用间隔（毫秒）
        /// </summary>
        public readonly int Interval;

        /// <summary>
        /// 优先级（越高越先触发）
        /// </summary>
        public int Priority = 0;
        
        public EventMessageAttribute(int interval, int priority = 0)
        {
            Interval = interval;
            Priority = priority;
        }
    }
}