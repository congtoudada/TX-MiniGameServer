/****************************************************
  文件：TimerSvc.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月05日 11:48:57
  功能：
*****************************************************/

namespace MiniGameServer
{
    public class TimerSvc : Singleton<TimerSvc> {
        public override void Init() {
            base.Init();

            this.Log("TimeSvc Init Done.");
        }

        public override void Update() {
            base.Update();
        }
    }
}