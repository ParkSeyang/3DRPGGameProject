using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParkSeyang.CombatSystem
{
    public class CombatSystem : SingletonBase<CombatSystem>
    {
        public class Events
        {
            public Action<CombatEvent> OnSomeoneTakeDamage;
            public Action<CombatEvent> OnSomeoneHeal;
        }

        public Events Subscribe { get; } = new();

        private const int EventProcessPerFrame = 10;
        private readonly Dictionary<Collider, IHitTargetPart> hitTargetDic = new();
        private readonly Queue<CombatEvent> combatEventQueue = new();

        protected override void OnInitialize() => Debug.Log("[CombatSystem] 초기화 완료");

        private void Update()
        {
            for (var i = 0; i < EventProcessPerFrame; i++)
            {
                if (combatEventQueue.Count == 0) break;
                HandleCombatEvent(combatEventQueue.Dequeue());
            }
        }

        public void AddCombatEvent(CombatEvent combatEvent) => combatEventQueue.Enqueue(combatEvent);

        private void HandleCombatEvent(CombatEvent combatEvent)
        {
            combatEvent.receiver.TakeDamage(combatEvent.damage);
            Subscribe.OnSomeoneTakeDamage?.Invoke(combatEvent);
        }

        public void AddHitTarget(Collider col, IHitTargetPart hitTarget) => hitTargetDic.TryAdd(col, hitTarget);

        public void RemoveHitTarget(Collider col)
        {
            if (hitTargetDic.ContainsKey(col) == false) return;
            hitTargetDic.Remove(col);
        }

        public bool HasHitTarget(Collider col) => hitTargetDic.ContainsKey(col);

        public IHitTargetPart GetHitTarget(Collider col) => hitTargetDic[col];
    }
}
