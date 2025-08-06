/****************************************************
  文件：PkgHandleAttribute.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月06日 15:48:37
  功能：
*****************************************************/

using System;

namespace MiniGameServer
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MsgHandleAttribute : Attribute
    {
        /// <summary>
        /// 协议号
        /// </summary>
        public readonly Cmd Cmd;
        
        public MsgHandleAttribute(Cmd cmd)
        {
            Cmd = cmd;
        }
    }
}