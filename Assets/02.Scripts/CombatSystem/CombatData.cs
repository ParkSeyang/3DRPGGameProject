using UnityEngine;

namespace ParkSeyang.CombatSystem
{
    public struct HitInfo
    {
        public ICombatAgent receiver;
        public Vector3 position;
        public IHitTargetPart hitTarget;
        public int parameter; // 타격 종류 (예: 1 = 약공격, 2: 강공격)
        public GameObject gameObject;
    }

    public struct CombatEvent
    {
        public ICombatAgent sender;
        public ICombatAgent receiver;
        public int damage;
        public HitInfo hitInfo;
    }
}
