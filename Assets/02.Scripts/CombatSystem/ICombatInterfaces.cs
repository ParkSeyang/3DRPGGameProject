using UnityEngine;

namespace ParkSeyang.CombatSystem
{
    public interface ICombatAgent
    {
        void TakeDamage(int damage);
        void OnHitDetected(HitInfo hitInfo);
    }

    public interface IHitTargetPart
    {
        ICombatAgent Owner { get; }
        Collider Collider { get; }
        GameObject gameObject { get; }
        void Initialize(ICombatAgent owner);
    }

    public interface IHitDetector
    {
        ICombatAgent Owner { get; }
        LayerMask DetectionLayer { get; }
        void Initialize(ICombatAgent owner);
        void EnableDetection();
        void DisableDetection();
    }
}
