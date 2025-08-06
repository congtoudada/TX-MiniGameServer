/*************************************************
	作者: Plane
	邮箱: 1785275942@qq.com
	日期: 2021/02/25 15:59
	功能: 单例类

    //=================*=================\\
           教学官网：www.qiqiker.com
           关注微信公众号: PlaneZhong
           关注微信服务号: qiqikertuts           
               ~~获取更多教学资讯~~
    \\=================*=================//
*************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace MiniGameServer {
    public class Singleton<T> where T : new() {
        private static T instance;
        public static T Instance {
            get {
                if(instance == null) {
                    instance = new T();
                }
                return instance;
            }
        }

        public virtual void Init() { }

        public virtual void Update() { }
        
        public virtual void DeInit() { }
    }
}
