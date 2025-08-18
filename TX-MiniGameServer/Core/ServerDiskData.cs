/****************************************************
  文件：CacheData.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月07日 15:04:33
  功能：
*****************************************************/

using System;
using System.Collections.Generic;

namespace MiniGameServer
{
    [Serializable]
    public class ServerDiskData
    {
        public long CurrentId = 0;
        public Dictionary<string, long> RegisterDict = new Dictionary<string, long>();
    }
}