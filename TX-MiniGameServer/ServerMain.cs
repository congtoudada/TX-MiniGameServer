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
            
            while(true) {
                launcher.Update();
                Thread.Sleep(10);
            }
        }
    }
}

