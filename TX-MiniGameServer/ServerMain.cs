using System;
using System.Threading;

namespace MiniGameServer
{
    static class ServerMain
    {
        static void Main(string[] args)
        {
            ServerLauncher launcher = new ServerLauncher();
            launcher.Init();
            long begin;
            int sleepTime;
            while(true)
            {
                begin = NetSvc.GetTimeStamp();
                launcher.Update();
                sleepTime = (int)Math.Max(0, ServerConfig.Tick - (NetSvc.GetTimeStamp() - begin));
                Thread.Sleep(sleepTime);
            }
        }
    }
}

