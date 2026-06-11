using System;

namespace EnjoyJob.Battle
{
    // 亚比动画资源路径。
    // 路径建议写 Resources 相对路径，例如 Animations/Pets/rocker/normal_attack_1。
    [Serializable]
    public class PetAnimationPaths
    {
        public string idle;
        public string normalAttack1;
        public string normalAttack2;
        public string specialAttack1;
        public string specialAttack2;
        public string attributeAttack1;
        public string attributeAttack2;
        public string hurt;
    }
}
