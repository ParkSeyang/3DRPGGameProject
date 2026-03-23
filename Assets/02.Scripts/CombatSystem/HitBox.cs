using UnityEngine;
using System.Collections.Generic;

public class HitBox : MonoBehaviour, IHitDetector
{
    [field: SerializeField] public LayerMask DetectionLayer { get; private set; }
    public ICombatAgent Owner { get; private set; }
    
    private Collider hitBoxCollider;
    private HashSet<IHitTargetPart> hitList = new HashSet<IHitTargetPart>();

    private void Awake()
    {
        hitBoxCollider = GetComponent<Collider>();
        if (hitBoxCollider != null)
        {
            // 설계 원칙: 판정용 박스는 반드시 트리거여야 함
            hitBoxCollider.isTrigger = true; 
            hitBoxCollider.enabled = false;
        }
    }

    public void Initialize(ICombatAgent owner)
    {
        Owner = owner;
    }

    public void Initialize(ICombatAgent owner, LayerMask detectionLayer)
    {
        Owner = owner;
        DetectionLayer = detectionLayer;
    }

    public void EnableDetection()
    {
        if (hitBoxCollider != null) hitBoxCollider.enabled = true;
        hitList.Clear();
    }

    public void DisableDetection()
    {
        if (hitBoxCollider != null) hitBoxCollider.enabled = false;
        hitList.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CombatSystem.Instance == null)
        {
            Debug.LogWarning($"[{gameObject.name}] CombatSystem 인스턴스가 존재하지 않습니다!");
            return;
        }

        // [체크 1] 레이어 마스크 필터링 (정상적인 필터링이므로 로그 없이 리턴)
        if (DetectionLayer.Contains(other.gameObject.layer) == false) return;

        // [체크 2] CombatSystem에 등록된 유효한 HurtBox인지 확인 (레이어는 타격 대상인데 HurtBox가 없는 설정 오류)
        if (CombatSystem.Instance.HasHitTarget(other) == false)
        {
            Debug.LogWarning($"[{gameObject.name}] {other.name}은(는) 타격 레이어에 속해있지만, CombatSystem에 등록된 HurtBox가 없습니다.");
            return;
        }

        IHitTargetPart targetPart = CombatSystem.Instance.GetHitTarget(other);
        
        // [체크 4] 중복 타격 방지
        if (hitList.Contains(targetPart)) return;

        // --- 모든 검증 통과: 데미지 전송 ---
        HitInfo hitInfo = new HitInfo();
        hitInfo.hitTarget = targetPart;
        hitInfo.receiver = targetPart.Owner;
        hitInfo.gameObject = other.gameObject;
        hitInfo.position = other.ClosestPoint(transform.position);
        hitInfo.parameter = 0;

        Owner?.OnHitDetected(hitInfo);
        hitList.Add(targetPart);
    }
}