using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HurtBox : MonoBehaviour, IHitTargetPart
{
    public ICombatAgent Owner { get; private set; }
    public Collider Collider { get; private set; }
    public GameObject gameObject => base.gameObject;

    private void Awake()
    {
        Collider = GetComponent<Collider>();
    }

    public void Initialize(ICombatAgent owner)
    {
        Owner = owner;
        if (CombatSystem.Instance != null)
        {
            CombatSystem.Instance.AddHitTarget(Collider, this);
        }
        else
        {
            Debug.LogError("CombatSystem Instance is null!");
        }
    }

    private void OnDestroy()
    {
        if (CombatSystem.IsInitialized)
        {
            CombatSystem.Instance.RemoveHitTarget(Collider, this);
        }
    }
}
