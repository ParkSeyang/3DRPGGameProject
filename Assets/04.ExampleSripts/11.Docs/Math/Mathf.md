# 게임 수학의 만능 도구, Mathf

## Mathf란 무엇인가? (비유 + 정의 + 왜 필요한가?)

게임을 만드는 과정을 요리라고 비유해봅시다. 캐릭터를 움직이고, 점프시키고, 카메라를 흔드는 등 화려한 연출을 위해서는 아주 정밀한 '계량'이 필요합니다. 이때 **Mathf(매스에프)** 는 마치 모든 눈금이 정확하게 새겨진 만능 계량컵, 온도계, 타이머가 모두 들어있는 '마법의 요리 도구 상자'와 같습니다. 이 도구 상자만 있으면 "두 지점 사이의 중간", "값을 특정 범위로 제한하기", "부드럽게 변화시키기" 같은 복잡한 계산을 매번 직접 만들 필요 없이, 이미 만들어진 최고의 도구를 꺼내 쓰기만 하면 됩니다.

정의하자면, **Mathf**는 Unity 엔진에서 게임 개발에 필요한 보편적인 수학 계산 기능들을 미리 모아놓은 C#의 **정적(static) 구조체**입니다. 여기에는 간단한 최대/최소값 비교부터 삼각함수, 그리고 게임 개발에 특히 유용한 보간(Interpolation) 함수까지 포함되어 있습니다.

그렇다면 왜 필요할까요? 게임의 모든 움직임과 변화는 수학에 기반합니다. 캐릭터가 목표 지점까지 부드럽게 다가가거나, 아이템이 통통 튀는 효과, 혹은 플레이어의 체력이 0 이하로 내려가지 않게 막는 것까지 모두 수학적 계산이 필요합니다. Mathf는 이런 게임 개발의 단골 수학 문제들을 **미리, 그리고 매우 효율적으로** 구현해놓았기 때문에, 개발자는 복잡한 수학 공식을 직접 코딩하는 수고를 덜고 게임의 핵심 로직 개발에 더 집중할 수 있습니다.

## 핵심 요약

-   Mathf는 **정적(static) 클래스**이므로, `new Mathf()`처럼 객체를 생성할 필요 없이 `Mathf.PI`, `Mathf.Lerp()`처럼 바로 사용할 수 있습니다.
-   `Mathf.PI`(원주율), `Mathf.Deg2Rad`(각도→라디안 변환)와 같은 필수적인 **수학 상수**를 제공합니다.
-   `Clamp`(범위 제한), `Lerp`(선형 보간), `Sin`(사인 함수) 등 게임 내 움직임과 값의 변화를 제어하는 **핵심 함수**들을 포함합니다.
-   캐릭터의 움직임, 애니메이션, 게임 규칙 등 Unity 개발의 거의 모든 영역에서 사용되는 필수 클래스입니다.

## 세부 개념

### 1. 주요 상수 (Constants)

코딩에 필요한 기본적인 수학 상수들을 미리 정의해 두었습니다.

| 상수 | 설명 |
| :--- | :--- |
| `Mathf.PI` | 원주율 (약 3.141592) 입니다. 원의 둘레나 넓이를 계산하는 등 기하학 계산에 사용됩니다. |
| `Mathf.Infinity` | 양의 무한대를 나타내는 값입니다. |
| `Mathf.Deg2Rad` | 각도(Degree) 단위를 라디안(Radian) 단위로 변환할 때 곱하는 상수입니다. (약 0.01745) |
| `Mathf.Rad2Deg` | 라디안 단위를 각도 단위로 변환할 때 곱하는 상수입니다. (약 57.29578) |

-   **각도(Degree)와 라디안(Radian)은 왜 중요할까?**
    -   우리는 "90도 회전"처럼 **각도** 단위에 익숙하지만, 대부분의 수학 함수(특히 삼각함수)는 **라디안** 단위를 사용합니다.
    -   Unity의 `transform.eulerAngles`는 각도 단위를 사용하지만, `Mathf.Sin()`, `Mathf.Cos()` 등은 라디안 단위를 입력받습니다. 따라서 이 둘 사이의 값을 변환해야 할 때 `Deg2Rad`와 `Rad2Deg` 상수는 매우 유용합니다.
    -   **예시**: `float angleInDegrees = 90.0f; float angleInRadians = angleInDegrees * Mathf.Deg2Rad;`

### 2. 값의 범위 제한 및 처리

게임에서는 값이 특정 범위를 벗어나지 않도록 '강제'해야 하는 경우가 매우 많습니다.

-   **`Mathf.Clamp(float value, float min, float max)`**
    -   `value`가 `min`과 `max` 사이의 값이 되도록 강제로 조정합니다. `value`가 `min`보다 작으면 `min`을, `max`보다 크면 `max`를 반환합니다.
    -   **문법 구조**: `결과 = Mathf.Clamp(현재값, 최소값, 최대값);`
    -   **단순 예제**: `Mathf.Clamp(150, 0, 100)` → `100` 반환
    -   **실용 예제 (체력 제한)**: 플레이어의 체력이 0 미만으로 내려가거나 최대 체력을 초과하지 않도록 막습니다.
        ```csharp
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        ```
    -   **잘못된 코드 예시**: `min`과 `max` 값을 반대로 넣는 경우. `Mathf.Clamp(50, 100, 0)`은 항상 `max` 값인 `0`을 반환하게 되어 의도와 다르게 동작합니다.

-   **`Mathf.Min(a, b)` / `Mathf.Max(a, b)`**
    -   두 개의 값 중 더 작은 값(`Min`) 또는 더 큰 값(`Max`)을 반환합니다.
    -   **실용 예제**: 두 플레이어 중 더 높은 점수를 가진 플레이어를 찾을 때. `float highScore = Mathf.Max(player1Score, player2Score);`

-   **`Mathf.Abs(f)`**
    -   값의 부호를 무시한 절대값을 반환합니다. `Mathf.Abs(-10)` → `10`

-   **`Mathf.Round()`, `Mathf.Ceil()`, `Mathf.Floor()`**
    -   `Round`: 소수점 첫째 자리에서 반올림하여 가장 가까운 정수를 반환합니다. (예: `1.5f` → `2`, `1.4f` → `1`)
    -   `Ceil` (Ceiling, 천장): 값을 무조건 올림하여 정수를 반환합니다. (예: `1.1f` → `2`)
    -   `Floor` (바닥): 값을 무조건 내림(버림)하여 정수를 반환합니다. (예: `1.9f` → `1`)

### 3. 보간 (Interpolation) - 부드러운 변화의 핵심

'보간'은 두 지점 사이의 중간값을 찾는 과정으로, 게임에서 부드러운 움직임이나 값의 변화를 만드는 데 핵심적인 역할을 합니다.

-   **`Mathf.Lerp(float a, float b, float t)`**
    -   **L**inear Int**erp**olation (선형 보간)의 약자입니다. 시작값 `a`와 목표값 `b`가 있을 때, `t`라는 비율(0.0 ~ 1.0)에 따라 그 사이의 값을 계산합니다.
    -   **수학적 원리**: `결과 = a + (b - a) * t`
        -   `t`가 0이면 시작값 `a`를 반환합니다.
        -   `t`가 1이면 목표값 `b`를 반환합니다.
        -   `t`가 0.5이면 `a`와 `b`의 정확히 중간 지점을 반환합니다.
    -   **단순 예제**: `Mathf.Lerp(10f, 20f, 0.5f)` → `15f` 반환
    -   **실용 예제 1 (부드러운 색상 변경)**: 2초에 걸쳐 현재 색상에서 목표 색상으로 부드럽게 변경합니다.
        ```csharp
        public Color startColor = Color.blue;
        public Color endColor = Color.red;
        public float duration = 2.0f;
        private float elapsedTime = 0.0f;
        
        void Update()
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration); // t를 0과 1 사이로 유지
            GetComponent<Renderer>().material.color = Color.Lerp(startColor, endColor, t);
        }
        ```
    -   **실용 예제 2 (부드러운 이동 - Ease Out 효과)**: `t` 값에 `Time.deltaTime`을 사용하면 목표 지점에 가까워질수록 느려지는 감속 효과를 쉽게 만들 수 있습니다. (정확한 시간 제어는 아님)
        ```csharp
        public Transform target;
        public float smoothSpeed = 5.0f;

        void Update()
        {
            // 매 프레임 현재 위치에서 목표 위치까지의 10%씩 이동 (예: speed=5일때)
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * smoothSpeed);
        }
        ```

-   **`Mathf.SmoothStep(float from, float to, float t)`**
    -   `Lerp`와 비슷하지만, 시작과 끝 지점에서 속도가 0이 되도록 부드러운 S자 곡선(Ease-in & Ease-out)으로 보간합니다. `Lerp`가 등속 운동이라면, `SmoothStep`은 가속 및 감속 운동과 같습니다.

### 4. 삼각 함수 (Trigonometry) - 주기적인 움직임의 마법

삼각함수는 원의 운동과 관련이 깊으며, -1과 1 사이를 부드럽게 오가는 주기적인 값을 만들어냅니다.

-   **`Mathf.Sin(float f)` / `Mathf.Cos(float f)`**
    -   입력값 `f`(라디안)에 대한 사인(sin), 코사인(cos) 값을 반환합니다.
    -   **핵심 특징**: 입력값이 계속 증가하면, 결과값은 -1과 1 사이를 부드러운 곡선 형태로 영원히 반복합니다.
    -   **실용 예제 (두둥실 떠다니는 효과)**: `Time.time`을 입력값으로 사용하여 오브젝트가 위아래로 부드럽게 움직이게 합니다.
        ```csharp
        public float amplitude = 0.5f; // 움직임의 폭
        public float frequency = 1.0f; // 움직임의 속도
        private Vector3 startPos;

        void Start() { startPos = transform.position; }

        void Update()
        {
            float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
        ```

## 개념 다이어그램 & 코드 예제

| 개념 다이어그램 | 코드 예제 |
| :--- | :--- |
| **Lerp (선형 보간)**<br>A ---------------- B<br>   `t=0`           `t=0.5`          `t=1`<br><br>**SmoothStep (S자 보간)**<br>A S--------------S B<br>  `t=0`           `t=0.5`          `t=1`<br><br>**Sin Wave**<br>  /\<br>/  \<br>---  ----  ---> `Time.time`<br>\  /<br> \/ | ```csharp // MathfFunctionsExample.cs using UnityEngine;  public class MathfFunctionsExample : MonoBehaviour {     public Transform target;     private float health = 100f;      void Update()     {         // 1. Clamp: 체력이 0~100 사이를 벗어나지 않도록 함         if (Input.GetKeyDown(KeyCode.Space))         {             health -= 30f;             health = Mathf.Clamp(health, 0f, 100f);             Debug.Log("현재 체력: " + health);
        }
          
         // 2. Lerp: 매 프레임 타겟을 향해 부드럽게 다가감         transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * 2f);
          
         // 3. Sin: y축으로 두둥실 떠다니는 효과         float bobbing = Mathf.Sin(Time.time * 3f) * 0.25f;
         transform.position += new Vector3(0, bobbing, 0);
     }
 }
 ``` |

## 주요 Public 속성(Property) 및 메서드

| 구분 | 이름 | 설명 |
| :--- | :--- | :--- |
| **상수** | `PI`, `Deg2Rad`, `Rad2Deg` | 원주율 및 각도-라디안 변환 상수입니다. |
| **범위 제한** | `Clamp`, `Min`, `Max` | 값의 범위를 제한하거나 두 값 중 하나를 선택합니다. |
| **보간** | `Lerp`, `LerpAngle`, `SmoothStep` | 두 값 사이를 부드럽게 보간합니다. `LerpAngle`은 360도를 넘어가는 각도 보간에 특화되어 있습니다. |
| **삼각함수** | `Sin`, `Cos`, `Tan`, `Atan2` | 주기적인 움직임이나 각도를 계산할 때 사용합니다. |
| **처리** | `Abs`, `Round`, `Ceil`, `Floor` | 절대값, 반올림, 올림, 내림을 처리합니다. |
| **기타** | `Sqrt`, `Pow` | 제곱근(`Sqrt`)과 거듭제곱(`Pow`)을 계산합니다. |

## 활용 예시 (실습 브릿지 포함)

**시나리오**: 적을 발견하면 부드럽게 그쪽으로 방향을 트는 포탑(Turret)을 만들어 봅시다. 단, 포탑은 좌우 45도, 총 90도의 제한된 각도로만 움직일 수 있습니다.

1.  **목표 각도 계산**: `Vector2 direction = target.position - turret.position;` 으로 목표 방향을 구한 뒤, `Mathf.Atan2(direction.y, direction.x)`를 사용하여 목표 지점까지의 각도를 라디안 단위로 구합니다.
2.  **단위 변환**: `Mathf.Rad2Deg`를 곱하여 라디안을 우리가 이해하기 쉬운 각도 단위로 변환합니다.
3.  **각도 제한**: `Mathf.Clamp(targetAngle, -45f, 45f)`를 사용하여 계산된 각도가 포탑의 회전 범위(-45도 ~ 45도)를 벗어나지 않도록 제한합니다.
4.  **부드러운 회전**: `Mathf.LerpAngle()` 함수를 사용하여 포탑의 현재 각도에서 제한된 목표 각도까지 부드럽게 회전시킵니다. `LerpAngle`은 350도에서 10도로 회전할 때 -20도가 아닌 +20도로 최단거리 회전하도록 처리해주는 똑똑한 함수입니다.

이제 여러분은 Mathf의 강력한 기능들을 이해했습니다. 다음 실습 문제를 통해 직접 다양한 수학적 움직임을 구현해보면서 개념을 확실히 다져봅시다.

## 실습 문제 (기초/응용 단계화)

1.  **(기초-기억)** `Mathf.Clamp(10, 20, 30)`의 결과는 무엇일까요? 그 이유는 무엇인가요?
2.  **(기초-이해)** `Mathf.Lerp`와 `Mathf.SmoothStep`의 가장 큰 차이점은 무엇이며, 어떤 상황에 `SmoothStep`을 사용하는 것이 더 자연스러울까요?
3.  **(응용-적용)** `Mathf.Sin`과 `Time.time`을 사용하여, 불빛(Light)의 밝기(`intensity`)가 0.5와 2.0 사이에서 부드럽게 깜빡이는 스크립트를 작성해보세요.
4.  **(응용-분석)** 아래 코드는 오브젝트가 2초에 걸쳐 목표 지점에 도착하게 하려 했지만, 실제로는 영원히 목표에 닿지 못하고 근처에서 느려집니다. 그 이유를 `Mathf.Lerp`의 동작 원리와 관련지어 설명해보세요.
    ```csharp
    void Update() {
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * 0.5f);
    }
    ```
5.  **(심화-창작)** 마우스 스크롤 휠을 사용하여 카메라의 시야각(`fieldOfView`)을 조절하는 줌(Zoom) 기능을 만들어보세요. `Input.GetAxis("Mouse ScrollWheel")`로 입력값을 받고, `Mathf.Lerp`를 사용해 부드럽게, `Mathf.Clamp`를 사용해 최소 줌(예: 30)과 최대 줌(예: 60)을 벗어나지 않도록 구현해보세요.

## Note: 주의사항

-   **`Mathf` vs `System.Math`**: C#에는 `System.Math`라는 기본 수학 클래스도 있습니다. 하지만 이는 `double`(64비트 실수) 자료형을 주로 사용하는 반면, Unity의 `Mathf`는 게임 개발에 표준적으로 쓰이는 `float`(32비트 실수) 자료형을 사용합니다. Unity 개발 시에는 성능 및 형 변환 문제를 피하기 위해 `Mathf`를 사용하는 것이 일반적입니다.
-   **`Lerp`와 프레임 의존성**: `Lerp`의 `t`값에 `Time.deltaTime`을 곱하는 방식은 구현이 간편하지만, 결과가 프레임 속도에 미세하게 영향을 받을 수 있습니다. 정확한 시간 제어가 필요하다면 '활용 예시'의 색상 변경 코드처럼 경과 시간을 직접 계산하여 `t`값을 0에서 1로 만들어주는 것이 정석입니다.
-   **라디안과 각도**: 삼각함수는 라디안, 트랜스폼 회전은 각도. 이 규칙을 항상 기억하고 `Mathf.Deg2Rad`와 `Mathf.Rad2Deg`를 적재적소에 사용하여 단위를 변환하는 습관을 들이는 것이 중요합니다.

## Further Study: 확장 학습

-   **Vector3 / Vector2**: `Mathf`가 단일 숫자(float)를 다룬다면, `Vector3`와 `Vector2`는 3차원/2차원 공간의 위치와 방향을 다룹니다. `Vector3.Lerp`, `Vector3.Distance`, `Vector3.Dot` 등 `Mathf`와 함께 게임 개발의 근간을 이루는 필수 클래스이므로 반드시 학습해야 합니다.
-   **Quaternion**: Unity의 회전은 `Quaternion`이라는 복잡하지만 강력한 시스템으로 처리됩니다. `Quaternion.Lerp`, `Quaternion.Slerp` 등을 사용하면 3D 공간에서 오브젝트를 부드럽게 회전시킬 수 있습니다.
-   **AnimationCurve**: `SmoothStep`보다 더 복잡하고 자유로운 가감속 곡선을 만들고 싶을 때 사용합니다. 인스펙터에서 직접 그래프를 그려서 부드러운 변화의 패턴을 시각적으로 디자인할 수 있는 강력한 기능입니다.
