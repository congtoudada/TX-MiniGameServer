using System;
using System.Threading;

namespace MiniGameServer
{
    static class ServerMain
    {
        static void Main(string[] args)
        {
            ServerDriver driver = new ServerDriver();
            driver.Init();
            
            while(true) {
                driver.Update();
                Thread.Sleep(10);
            }
        }
    }
}

