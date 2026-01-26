using UnityEngine;

namespace ParkSeyang.CombatSystem
{
    [RequireComponent(typeof(Collider))]
    public class HurtBox : MonoBehaviour, IHitTargetPart
    {
        public ICombatAgent Owner { get; private set; }
        public Collider Collider { get; private set; }

        private void Awake() => Collider = GetComponent<Collider>();

        public void Initialize(ICombatAgent owner)
        {
            Owner = owner;
            CombatSystem.Instance.AddHitTarget(Collider, this);
        }

        private void OnDestroy()
        {
            if (CombatSystem.IsInitialized)
            {
                CombatSystem.Instance.RemoveHitTarget(Collider);
            }
        }
    }
}
