using System.Numerics;

namespace MiniGameServer
{
    public class MonsterData
    {
        // 怪物血量
        public int HP { get; set; }

        // 怪物位置
        public Vector3 Position { get; set; }

        // 怪物旋转
        public Vector3 Rotation { get; set; }
        
        // 构造函数
        public MonsterData(int hp, Vector3 position, Vector3 rotation)
        {
            HP = hp;
            Position = position;
            Rotation = rotation;
        }
    }
}