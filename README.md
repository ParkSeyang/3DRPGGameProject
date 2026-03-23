# 3D RPG Game - Technical Documentation (First Build Master)

본 프로젝트는 유니티 6를 기반으로 설계된 고성능 핵앤슬래시 RPG로, **객체 지향 설계 원칙(SOLID)**과 **데이터 중심 아키텍처**를 조화시켜 유지보수성과 확장성을 극대화했습니다.

## 1. 핵심 아키텍처 (Core Architecture)

### A. 엄격한 계층형 실행 제어 (Strict Layered Execution)
객체 간 의존성으로 인한 초기화 버그(Race Condition)를 원천 차단하기 위해 Unity의 `Execution Order`를 활용한 계층화된 실행 순서를 채택했습니다.
- **Data DB Layer (-100 ~ -95):** 데이터 원본 로드 (DataManager, EnemyDataManager, ItemDataManager).
- **Infra Layer (-85 ~ -81):** 핵심 매니저 및 이벤트 버스 구축 (CombatSystem, UIManager, GameSceneManager).
- **Domain Layer (-72 ~ -70):** 인벤토리 데이터 캐시 및 스킬 트리 로직.
- **World & Physics Layer (-60 ~ -45):** 물리 객체 스폰 및 물리 안정화 제어.
- **Input Layer (-10):** 최상위 사용자 입력 감지 및 조작부.

### B. 상태 패턴 기반 AI (Modular State FSM)
몬스터의 행동 로직을 독립된 클래스로 캡슐화하여 AI의 복잡도를 낮추고 가독성을 확보했습니다.
- `BaseState` 추상 클래스를 통한 공통 인터페이스 제공.
- 각 행동(`Idle`, `Patrol`, `Chase`, `Attack`, `Hit`, `Dead`, `Return`)을 독립된 클래스로 구현하여 개방-폐쇄 원칙(OCP) 준수.

## 2. 고도화된 전투 및 제어 엔진 (Combat & Control)

### A. 프레임 단위 정밀 전투 시스템
- **Observer Pattern:** 전투 데이터를 전파하는 허브와 이를 구독하는 연출(VFX/SFX) 로직을 분리하여 코드 유연성 증대.
- **Animation Sync Logic:** `AnimTriggerEventSender`를 활용, 시각적 모션과 물리적 판정(HitBox/GuardBox)을 프레임 단위로 동기화.
- **Minimum Damage Correction:** 공격력과 방어력의 수치적 격차와 무관하게 타격감을 유지하는 최소 데미지(1) 보정 로직 구현.

### B. 정교한 방어 메커니즘 (Advanced Guard)
- **Vector Math Calculation:** `DotProduct`(내적)를 활용하여 전방 약 160도 범위의 공격만 가드로 판정하는 정교함 확보.
- **Action Lockdown:** 가드 해제 및 스킬 시전 애니메이션이 끝날 때까지 조작을 제한하여 묵직한 조작감 구현.
- **Fail-safe Logic:** 피격(`Hit`) 발생 시 즉시 조작 제한을 해제하여 캐릭터 굳음 현상을 방지하는 안전장치 구축.

## 3. 데이터 관리 및 지속성 (Data & Persistence)

### A. 데이터 주도 설계 (Data-Driven Design)
- **Runtime TSV Injection:** 모든 몬스터, 아이템, 퀘스트 기획 수치를 코드 외부(TSV)에서 관리하여 밸런싱 효율성 극대화.
- **SO Runtime Instancing:** ScriptableObject 원본 데이터의 불변성을 유지하기 위해 런타임 복제(Instantiate) 방식을 채택.

### B. JSON 기반 데이터 지속성 (Persistence)
- **Integrated Save/Load:** 인벤토리, 유저 스탯, 퀘스트 진행도를 단일 JSON 파일로 통합 관리하는 직렬화 시스템.
- **Full-Sync Loading Sequence:** 씬 로드 후 10초간의 안정화 프로세스(데이터 복구 -> 물리 복구 -> 조작 해제)를 통해 물리 버그 및 데이터 누락 원천 차단.

## 4. 유니티 6 특화 기술 및 UI 아키텍처

### A. 위임 기반 UI 시스템 (Delegation Architecture)
- **Separation of Concerns:** 드래그 앤 드롭 행위(UserInput)와 실제 비즈니스 로직(Trade/Equip)을 분리하여 유지보수성 향상.
- **Dynamic Monster UI:** `MonsterStatUI`를 통해 몬스터의 이름, 레벨, 실시간 체력을 월드 공간 UI로 실시간 트래킹.

### B. 차세대 물리 엔진 최적화
- **Linear Physics:** 유니티 6 권장 `linearVelocity` API를 전수 적용하여 물리 시뮬레이션 정밀도 향상.
- **Custom Gravity Force:** 질량이 반영된 `ForceMode.Force` 기반 커스텀 중력을 통해 실제적인 물리 액션 구현.
- **Singleton Stability:** 앱 종료 플래그를 통한 싱글톤 소멸 순서 제어로 런타임 에러 완전 제거.