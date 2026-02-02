using UnityEngine;
using System.Collections.Generic;

public class HitBox : MonoBehaviour, IHitDetector
{
    [field: SerializeField] public LayerMask DetectionLayer { get; private set; }
    public ICombatAgent Owner { get; private set; }
    
    private Collider col;
    private HashSet<IHitTargetPart> hitList = new HashSet<IHitTargetPart>();

    private void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
    }

    public void Initialize(ICombatAgent owner)
    {
        Owner = owner;
    }

    public void EnableDetection()
    {
        col.enabled = true;
        hitList.Clear();
    }

    public void DisableDetection()
    {
        col.enabled = false;
        hitList.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        // CombatSystem이 없거나, 타겟이 등록되지 않은 경우 무시
        if (CombatSystem.Instance == null || CombatSystem.Instance.HasHitTarget(other) == false) return;

        // 레이어 마스크 체크 (확장 메서드 사용)
        if (DetectionLayer.Contains(other.gameObject.layer) == false) return;

        IHitTargetPart targetPart = CombatSystem.Instance.GetHitTarget(other);

        // 이미 이번 공격에 맞은 대상이면 무시 (중복 타격 방지)
        if (hitList.Contains(targetPart)) return;

        HitInfo hitInfo = new HitInfo();
        hitInfo.hitTarget = targetPart;
        hitInfo.receiver = targetPart.Owner;
        hitInfo.gameObject = other.gameObject;
        hitInfo.position = other.ClosestPoint(transform.position);
        hitInfo.parameter = 0; // 필요시 공격 타입 등 전달

        // 공격자(Owner)에게 타격 성공 알림
        Owner?.OnHitDetected(hitInfo);

        // 타격 리스트에 추가
        hitList.Add(targetPart);
    }
}
