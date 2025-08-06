using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using ProtoBuf;

namespace PENet {
    public static class KcpTool 
    {
        #region Protobuf
        public static byte[] Serialize<T>(T msg) {
            using(MemoryStream ms = new MemoryStream()) {
                try {
                    Serializer.Serialize(ms, msg);
                    return ms.ToArray();
                }
                catch(SerializationException e) {
                    KcpLog.Error("Failed to serialize.Reason:{0}", e.Message);
                    throw;
                }
            }
        }
        
        public static T DeSerialize<T>(byte[] bytes) {
            using(MemoryStream ms = new MemoryStream(bytes)) {
                try {
                    T msg = Serializer.Deserialize<T>(ms);
                    return msg;
                }
                catch(SerializationException e) {
                    KcpLog.Error("Failed to Deserialize.Reason:{0} bytesLen:{1}", e.Message, bytes.Length);
                    throw;
                }
            }
        }
        #endregion
        
        #region 二进制序列化
        /// <summary>
        /// 对象->字节数组
        /// </summary>
        /// <param name="msg"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static byte[] SerializeBinary<T>(T msg) {
            using(MemoryStream ms = new MemoryStream()) {
                try {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, msg);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms.ToArray();
                }
                catch(SerializationException e) {
                    KcpLog.Error("Failed to serialize.Reason:{0}", e.Message);
                    throw;
                }
            }
        }
        /// <summary>
        /// 字节数组->对象
        /// </summary>
        /// <param name="bytes"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DeSerializeBinary<T>(byte[] bytes) {
            using(MemoryStream ms = new MemoryStream(bytes)) {
                try {
                    BinaryFormatter bf = new BinaryFormatter();
                    T msg = (T)bf.Deserialize(ms);
                    return msg;
                }
                catch(SerializationException e) {
                    KcpLog.Error("Failed to Deserialize.Reason:{0} bytesLen:{1}", e.Message, bytes.Length);
                    throw;
                }
            }
        }
        #endregion

        public static byte[] Compress(byte[] input) {
            using(MemoryStream outMS = new MemoryStream()) {
                using(GZipStream gzs = new GZipStream(outMS, CompressionMode.Compress, true)) {
                    gzs.Write(input, 0, input.Length);
                    gzs.Close();
                    return outMS.ToArray();
                }
            }
        }
        public static byte[] DeCompress(byte[] input) {
            using(MemoryStream inputMS = new MemoryStream(input)) {
                using(MemoryStream outMs = new MemoryStream()) {
                    using(GZipStream gzs = new GZipStream(inputMS, CompressionMode.Decompress)) {
                        byte[] bytes = new byte[1024];
                        int len = 0;
                        while((len = gzs.Read(bytes, 0, bytes.Length)) > 0) {
                            outMs.Write(bytes, 0, len);
                        }
                        gzs.Close();
                        return outMs.ToArray();
                    }
                }
            }
        }

        static readonly DateTime utcStart = new DateTime(1970, 1, 1);
        public static ulong GetUTCStartMilliseconds() {
            TimeSpan ts = DateTime.UtcNow - utcStart;
            return (ulong)ts.TotalMilliseconds;
        }
    }

    public enum KcpLogColor {
        None,       // 无（无颜色）
        Red,        // 红色
        Green,      // 绿色
        Blue,       // 蓝色
        Cyan,       // 青色（蓝绿色）
        Magenta,   //  洋红色（应为 Magenta，可能拼写错误）
        Yellow      // 黄色
    }
}
