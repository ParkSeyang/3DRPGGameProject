# 부드러운 움직임의 예술, 보간(Interpolation)

## 보간이란 무엇인가? (비유 + 정의 + 왜 필요한가?)

유명 애니메이터가 애니메이션을 만드는 과정을 상상해봅시다. 애니메이터는 캐릭터의 '시작 자세'와 '끝 자세'처럼 가장 중요하고 핵심적인 장면(Keyframe)들만 직접 그립니다. 그리고 그 사이의 수많은 중간 동작들은 어시스턴트들이나 컴퓨터 프로그램이 부드럽게 채워 넣습니다. 이처럼 **'정해진 지점들 사이의 중간 과정을 만들어내는 것'**이 바로 **보간(Interpolation)**입니다.

정의하자면, **보간**은 주어진 데이터 지점들(Keyframes) 사이의 존재하지 않는 새로운 데이터 지점(in-betweens)을 추정하여 만들어내는 과정입니다. 게임 개발에서는 주로 **'시작값'과 '목표값' 사이의 중간값을 계산하여 부드러운 변화를 만들어내는 기술**을 의미합니다. 이는 위치, 회전, 색상, 크기 등 수치로 표현할 수 있는 모든 것에 적용될 수 있습니다.

그렇다면 왜 필요할까요? 게임은 1초에 수십 번씩 화면을 새로 그리는(프레임) 방식으로 동작합니다. 만약 어떤 오브젝트를 A지점에서 B지점으로 옮길 때 보간이 없다면, 오브젝트는 한 프레임에 A에 있다가 다음 프레임에 B로 순간이동할 것입니다. 이는 매우 부자연스럽고 뚝뚝 끊겨 보입니다. 보간은 A와 B 사이의 수많은 중간 지점들을 각 프레임마다 계산하여 오브젝트가 마치 실제로 움직이는 것처럼 부드럽고 자연스러운 착시를 만들어주기 때문에, 모든 동적인 게임의 필수 요소입니다.

## 핵심 요약

-   보간(Interpolation)은 **시작점과 끝점 사이의 중간값을 계산**하여 부드러운 변화를 만드는 과정입니다.
-   **선형 보간(Linear Interpolation, Lerp)**은 등속도로 변화하는 가장 기본적인 보간 방식입니다.
-   Unity에는 `Mathf.Lerp`, `Vector3.Lerp`, `Color.Lerp`, `Quaternion.Slerp` 등 **다양한 데이터 타입**을 위한 보간 함수가 내장되어 있습니다.
-   `SmoothStep`이나 `AnimationCurve`를 사용하면 가속/감속 등 **비선형(Non-linear) 보간**을 통해 훨씬 풍부한 움직임을 표현할 수 있습니다.

## 세부 개념

### 1. 선형 보간 (Linear Interpolation - Lerp)

가장 기본적이고 널리 쓰이는 보간 방식입니다. '선형'이라는 이름처럼, 변화의 속도가 일정한 등속 운동을 구현합니다.

**공식: `결과 = 시작값 + (끝값 - 시작값) * t`**

-   `t`는 0.0에서 1.0 사이의 비율을 나타내며, 보간 과정이 얼마나 진행되었는지를 의미합니다. (0% ~ 100%)

#### 방식 1: 정해진 시간 동안의 보간 (Timed Interpolation)

가장 정석적이고 프레임 속도에 영향을 받지 않는 정확한 보간 방식입니다.

-   **원리**: 경과 시간을 전체 시간으로 나누어 `t`값을 직접 0에서 1로 만들어줍니다.
-   **실용 예제**: 문이 2초에 걸쳐 부드럽게 열리는 효과
    ```csharp
    public Transform door;
    public Vector3 openRotation;
    private Vector3 closedRotation;
    public float duration = 2.0f;

    void Start() { closedRotation = door.eulerAngles; }

    public void OpenDoor()
    {
        StartCoroutine(RotateDoor(openRotation));
    }

    IEnumerator RotateDoor(Vector3 targetRotation)
    {
        float elapsedTime = 0f;
        Vector3 startRotation = door.eulerAngles;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            // LerpAngle은 350도 -> 10도 같은 회전을 자연스럽게 처리
            float currentAngle = Mathf.LerpAngle(startRotation.y, targetRotation.y, t);
            door.eulerAngles = new Vector3(startRotation.x, currentAngle, startRotation.z);
            yield return null;
        }
    }
    ```

#### 방식 2: 지수적 감속 보간 (Exponential Easing)

구현이 매우 간단하여 흔히 쓰이지만, 수학적으로는 선형 보간이 아닌 감속 운동을 구현하는 방식입니다.

-   **원리**: `t`값에 `Time.deltaTime * speed`를 사용하여, 매 프레임 '현재 위치'에서 '목표 위치'까지의 일정 비율만큼 이동합니다. 목표에 가까워질수록 이동 거리가 줄어들어 자연스러운 감속 효과가 나타납니다.
-   **특징**: 수학적으로 목표 지점에 영원히 도달하지는 못하지만(무한히 가까워짐), 매우 부드러운 도착 연출이 가능합니다.
-   **실용 예제**: 플레이어를 부드럽게 따라다니는 카메라
    ```csharp
    public Transform player;
    public float smoothSpeed = 5.0f;
    public Vector3 offset;

    void LateUpdate() // 플레이어 이동이 끝난 후 카메라를 업데이트하기 위해 LateUpdate 사용
    {
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
        transform.position = smoothedPosition;
    }
    ```

### 2. 다양한 데이터 타입의 보간

Unity는 여러 데이터 타입을 위한 보간 함수를 제공합니다.

| 함수 | 설명 | 주 사용처 |
| :--- | :--- | :--- |
| `Mathf.Lerp` | 단일 `float` 값을 보간합니다. | 투명도(alpha), 볼륨, 회색톤(grayscale) 등 |
| `Vector3.Lerp` | 3차원 벡터(위치, 크기)를 보간합니다. | 오브젝트의 부드러운 이동, 크기 변화 |
| `Color.Lerp` | 색상(RGBA)을 보간합니다. | 캐릭터의 데미지 점멸 효과, 시간에 따른 하늘색 변화 |
| `Quaternion.Slerp` | 쿼터니언(회전)을 **구면 선형 보간**합니다. | 3D 오브젝트의 자연스러운 회전 (최단 경로, 등속 회전) |
| `Quaternion.Lerp` | 쿼터니언을 **선형 보간**합니다. `Slerp`보다 계산이 빠르지만, 회전 속도가 일정하지 않을 수 있습니다. | 성능이 매우 중요하고 회전 각도가 크지 않을 때 |

### 3. 비선형 보간 (Non-Linear Interpolation)

움직임을 더 풍부하고 생동감 있게 만들기 위해 사용됩니다.

-   **`Mathf.SmoothStep(from, to, t)`**: `t`가 0에서 1로 변할 때, 결과값이 S자 곡선을 그리며 부드럽게 가속하고 감속합니다. '스르륵' 나타나거나 사라지는 효과에 적합합니다.
-   **`AnimationCurve`**: 개발자가 직접 인스펙터에서 보간 곡선을 시각적으로 디자인할 수 있는 가장 강력한 도구입니다.
    -   **원리**: `public AnimationCurve myCurve;` 처럼 변수를 선언하면 인스펙터에 그래프 편집기가 나타납니다. `myCurve.Evaluate(t)` 함수를 호출하면, `t`값(0~1)에 해당하는 그래프의 y값을 반환해줍니다.
    -   **활용**: 통통 튀는 바운스 효과, 예상치 못하게 튀어나오는 UI, 심장 박동 같은 불규칙적인 움직임 등 거의 모든 종류의 커스텀 움직임을 만들 수 있습니다.

## 개념 다이어그램 & 코드 예제

| 개념 다이어그램 | 코드 예제 |
| :--- | :--- |
| **보간 곡선 비교**<br>1 ---      / (Linear)<br>    S (SmoothStep)<br>  /<br>0 ---<br>   `t=0` -> `t=1`<br><br>**바운스 곡선 (AnimationCurve)**<br>  /\\<br>/  \\/\\<br>---    -----> `t` | ```csharp // InterpolationShowcase.cs // 다양한 보간 방식을 시각적으로 보여주는 예제 using UnityEngine;  public class InterpolationShowcase : MonoBehaviour {     public Transform target;     public AnimationCurve bounceCurve; // 인스펙터에서 바운스 커브를 설정      private float elapsedTime = 0f;     private float duration = 2.0f;     private Vector3 startPos;      void Start()     {         startPos = transform.position;     }      void Update()     {         elapsedTime += Time.deltaTime;         float t = Mathf.Clamp01(elapsedTime / duration);          // 1. 선형 보간 (Lerp)         // transform.position = Vector3.Lerp(startPos, target.position, t);          // 2. 부드러운 가감속 (SmoothStep)         // float smooth_t = Mathf.SmoothStep(0, 1, t);         // transform.position = Vector3.Lerp(startPos, target.position, smooth_t);          // 3. 애니메이션 커브 (Bounce)         float bounce_t = bounceCurve.Evaluate(t);         transform.position = Vector3.Lerp(startPos, target.position, bounce_t);          // 4. 회전 (Slerp)         transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * 2.0f);          // 2초마다 리셋         if (elapsedTime > duration)         {             elapsedTime = 0;         }     } } ``` |

## 활용 예시 (실습 브릿지 포함)

**시나리오**: 플레이어가 피격당했을 때, 0.5초 동안 캐릭터가 빨갛게 점멸했다가 원래 색으로 돌아오는 '데미지 플래시' 효과를 만들어 봅시다.

1.  피격 시 호출될 `TakeDamage()` 함수를 만듭니다.
2.  `TakeDamage()` 함수 안에서 `DamageFlash()` 코루틴을 시작합니다.
3.  `DamageFlash()` 코루틴 내부:
    1.  `elapsedTime = 0`, `duration = 0.5` 등 타이머 변수를 설정합니다.
    2.  `while (elapsedTime < duration)` 루프를 만듭니다.
    3.  루프 안에서 `t = elapsedTime / duration` 으로 진행률을 계산합니다.
    4.  `Color.Lerp(Color.red, originalColor, t)` 를 사용하여 재질(material)의 색상을 변경합니다.
    5.  `elapsedTime += Time.deltaTime;` 과 `yield return null;` 로 한 프레임을 대기합니다.
4.  루프가 끝나면, 만일을 위해 색상을 `originalColor`로 확실하게 되돌려줍니다.

이처럼 보간은 게임에 생동감을 불어넣는 핵심적인 기술입니다. 이제 보간의 원리를 이해했으니, 직접 다양한 움직임을 만들어보며 그 차이를 느껴보세요.

## 실습 문제 (기초/응용 단계화)

1.  **(기초-기억)** `Mathf.Lerp(10, 20, 0.2f)`의 결과값은 무엇일까요?
2.  **(기초-이해)** 3D 모델을 부드럽게 회전시키려 할 때, `Vector3.Lerp`가 아닌 `Quaternion.Slerp`를 사용해야 하는 이유는 무엇인가요?
3.  **(응용-적용)** 사용자가 'Z' 키를 누르면 `Light` 컴포넌트의 색상이 3초에 걸쳐 파란색에서 노란색으로 부드럽게 변하는 스크립트를 작성해보세요. (`Color.Lerp` 사용)
4.  **(응용-분석)** `Vector3.Lerp(transform.position, target.position, 0.5f)` 코드를 `Update()`에 넣으면 오브젝트가 어떻게 움직일까요? 이 움직임과 `Vector3.Lerp(transform.position, target.position, Time.deltaTime * 5f)`의 움직임은 어떻게 다를까요?
5.  **(심화-창작)** `AnimationCurve`를 사용하여 오브젝트가 위로 살짝 튀어 올랐다가 아래로 떨어지면서 바닥에 몇 번 통통 튀고 멈추는 '바운스' 효과를 구현해보세요. 인스펙터에서 직접 커브를 디자인하고, `Update()`에서 `curve.Evaluate()`를 사용하여 Y축 위치를 제어합니다.

## Note: 주의사항

-   **`Lerp`의 `t`값은 0~1로 제한됨**: `Mathf.Lerp`는 `t`값을 자동으로 0과 1 사이로 제한(Clamp)합니다. 만약 1을 넘어 계속 진행하는 '외삽(Extrapolation)'이 필요하다면 `Mathf.LerpUnclamped`를 사용해야 합니다.
-   **회전 보간은 반드시 쿼터니언으로**: `transform.eulerAngles` (Vector3) 값을 직접 보간하면 짐벌락(Gimbal Lock) 등 예기치 못한 회전 오류가 발생할 수 있습니다. 회전 보간은 항상 `Quaternion.Lerp` 또는 `Quaternion.Slerp`를 사용하세요.
-   **성능**: 보간 함수 자체는 매우 빠릅니다. 하지만 수천 개의 오브젝트가 매 프레임 보간을 수행한다면 부하가 될 수 있습니다. 특히 `Update()` 안에서 `GetComponent`와 같은 무거운 함수와 함께 사용하지 않도록 주의해야 합니다.

## Further Study: 확장 학습

-   **이지잉 함수 (Easing Functions)**: `SmoothStep`은 대표적인 이지잉 함수 중 하나입니다. 세상에는 `EaseInQuad`, `EaseOutCubic`, `EaseInOutBounce` 등 훨씬 다양하고 재미있는 움직임을 만드는 수많은 이지잉 함수들이 있습니다. (http://easings.net/ 참고)
-   **스플라인 보간 (Spline Interpolation)**: 두 점 사이가 아닌, 여러 개의 점들을 모두 부드럽게 통과하는 곡선을 만드는 보간법입니다. (예: Catmull-Rom Spline) 아름다운 카메라 이동 경로, AI의 순찰 경로 등을 만드는 데 사용되는 고급 기법입니다.
-   **물리 기반 보간 (`FixedUpdate`)**: `Rigidbody`를 사용하는 물리 오브젝트를 부드럽게 움직일 때는 `Update`가 아닌 `FixedUpdate`에서 보간을 처리하는 것이 더 안정적이고 예측 가능한 결과를 만듭니다. `Rigidbody.MovePosition()` 등 물리 시스템과 함께 보간을 사용하는 방법을 학습해보세요.
