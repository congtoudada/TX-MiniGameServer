/*************************************************
	作者: Plane
	邮箱: 1785275942@qq.com
	日期: 2021/02/25 16:07
	功能: 业务系统基类

    //=================*=================\\
           教学官网：www.qiqiker.com
           关注微信公众号: PlaneZhong
           关注微信服务号: qiqikertuts           
               ~~获取更多教学资讯~~
    \\=================*=================//
*************************************************/

namespace MiniGameServer {
    public abstract class SystemRoot<T> : Singleton<T> where T : new() {
        protected NetSvc netSvc = null;
        protected CacheSvc cacheSvc = null;
        protected TimerSvc timerSvc = null;

        public override void Init() {
            base.Init();
            netSvc = NetSvc.Instance;
            cacheSvc = CacheSvc.Instance;
            timerSvc = TimerSvc.Instance;
        }
    }
}
