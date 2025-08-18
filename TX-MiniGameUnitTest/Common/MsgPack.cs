/****************************************************
  文件：MsgPack.cs
  作者：聪头
  邮箱：1322080797@qq.com
  日期：2025年08月06日 16:08:20
  功能：
*****************************************************/

using PENet;

namespace MiniGameServer
{
    //消息类型委托
    public delegate void MsgListener(MsgPack pack);
    
    public class MsgPack {
        public KcpSession<Pkg> Session;
        private readonly Pkg _pkg;
        public MsgPack(KcpSession<Pkg> session, Pkg pkg) {
            this.Session = session;
            this._pkg = pkg;
        }
        public Head Head => _pkg.Head;
        public Body Body => _pkg.Body;
    }
}