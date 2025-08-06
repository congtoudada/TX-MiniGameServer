using System;
using System.Threading;

namespace PENet
{
    public static class KcpLog
    {
        public static Action<string> LogFunc;
        public static Action<KcpLogColor, string> ColorLogFunc;
        public static Action<string> WarnFunc;
        public static Action<string> ErrorFunc;

        public static void Log(string msg, params object[] args) {
            msg = string.Format(msg, args);
            if(LogFunc != null) {
                LogFunc(msg);
            }
            else {
                ConsoleLog(msg, KcpLogColor.None);
            }
        }
        public static void ColorLog(KcpLogColor color, string msg, params object[] args) {
            msg = string.Format(msg, args);
            if(ColorLogFunc != null) {
                ColorLogFunc(color, msg);
            }
            else {
                ConsoleLog(msg, color);
            }
        }
        public static void Warn(string msg, params object[] args) {
            msg = string.Format(msg, args);
            if(WarnFunc != null) {
                WarnFunc(msg);
            }
            else {
                ConsoleLog(msg, KcpLogColor.Yellow);
            }
        }
        public static void Error(string msg, params object[] args) {
            msg = string.Format(msg, args);
            if(ErrorFunc != null) {
                ErrorFunc(msg);
            }
            else {
                ConsoleLog(msg, KcpLogColor.Red);
            }
        }
        private static void ConsoleLog(string msg, KcpLogColor color) {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            msg = $"Thread:{threadId} {msg}";

            switch(color) {
                case KcpLogColor.Red:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KcpLogColor.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KcpLogColor.Blue:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KcpLogColor.Cyan:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KcpLogColor.Magenta:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KcpLogColor.Yellow:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case KcpLogColor.None:
                default:
                    break;
            }

        }
    }
}