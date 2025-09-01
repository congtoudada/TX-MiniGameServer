/*************************************************
	作者: Plane
	邮箱: 1785275942@qq.com
	日期: 2021/02/25 18:06
	功能: 通用配置数据

    //=================*=================\\
           教学官网：www.qiqiker.com
           关注微信公众号: PlaneZhong
           关注微信服务号: qiqikertuts           
               ~~获取更多教学资讯~~
    \\=================*=================//
*************************************************/

namespace MiniGameServer {
    public static class CommonConfig {
        // 公网
        // public const string Ip = "111.229.199.15";
        // public const string InnerIp = "10.0.12.9";
        // 本地
        // public const string Ip = "127.0.0.1";
        // public const string InnerIp = "127.0.0.1";
        // 内网
        public const string Ip = "10.46.47.69";
        public const string InnerIp = "10.46.47.69";
        public const int Port = 17666;
        public const int ReqPingInterval = 8 * 1000;  // 30s 心跳发送频率
    }
}
