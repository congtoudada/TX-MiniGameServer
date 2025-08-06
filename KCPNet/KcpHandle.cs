using System;
using System.Buffers;
using System.Net.Sockets.Kcp;

namespace PENet {
    public class KcpHandle : IKcpCallback {
        public Action<Memory<byte>> Out;
        
        /// <summary>
        /// 当调用SendMsg时会触发该回调，定义数据如何发送给远端（例如，使用UDP发送Kcp封装的buffer）
        /// 实际如何发送由Out指定
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="avalidLength"></param>
        public void Output(IMemoryOwner<byte> buffer, int avalidLength) {
            using(buffer) {
                Out(buffer.Memory.Slice(0, avalidLength));
            }
        }
        
        /// <summary>
        /// 当收到kcp解析后一条完整数据的回调
        /// </summary>
        public Action<byte[]> Recv;
        public void Receive(byte[] buffer) {
            Recv(buffer);
        }
    }
}
