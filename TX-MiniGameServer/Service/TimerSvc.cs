/****************************************************
  文件：TimerSvc.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月05日 11:48:57
  功能：
*****************************************************/

using System;
using PETimer;

namespace MiniGameServer
{
    public class TimerSvc : Singleton<TimerSvc> {
        TickTimer timer = new TickTimer(0, false);
        public override void Init() {
            base.Init();

            timer.LogFunc = this.Log;
            timer.WarnFunc = this.Warn;
            timer.ErrorFunc = this.Error;

            this.Log("TimeSvc Init Done.");
        }

        public override void Update() {
            base.Update();

            timer.UpdateTask();
        }

        public int AddTask(uint delay, Action<int> taskCB, Action<int> cancelCB = null, int count = 1) {
            return timer.AddTask(delay, taskCB, cancelCB, count);
        }

        public bool DeleteTask(int tid) {
            return timer.DeleteTask(tid);
        }
    }
}