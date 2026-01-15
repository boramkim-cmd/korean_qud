# Caves of Qud 게임 API 설명

**작성일시:** 2026-01-13 10:14  
**목적:** 게임 API 이해 및 활용 방법

---

## 🎮 게임 API란?

**게임 API (Application Programming Interface)**는 Caves of Qud 게임 엔진이 제공하는 **C# 클래스와 메서드 모음**입니다.

### 간단히 말하면
```
게임 API = 게임 내부 기능에 접근하는 도구
```

---

## 📚 Caves of Qud 주요 API

### 1. XRL 네임스페이스
```csharp
using XRL;              // 게임 코어
using XRL.World;        // 게임 오브젝트, 플레이어
using XRL.UI;           // UI, 대화, 메시지
```

### 2. 주요 클래스

#### XRLCore - 게임 코어
```csharp
// 게임 인스턴스 접근
XRLCore.Core

// 현재 게임
XRLCore.Core.Game

// 플레이어
XRLCore.Core.Game.Player.Body
```

#### GameObject - 게임 오브젝트
```csharp
// 모든 게임 내 객체 (플레이어, NPC, 아이템 등)
GameObject obj;

// 이름
obj.DisplayName        // "검"
obj.ShortDisplayName   // "검"
obj.GetDisplayName()   // "빛나는 검"

// 설명
obj.GetShortDescription()
```

#### ConversationNode - 대화
```csharp
// 대화 노드
ConversationNode node;

// 대화 텍스트
node.GetDisplayText()  // "안녕하세요, 여행자."
```

#### MessageQueue - 메시지
```csharp
// 게임 메시지 로그
MessageQueue.AddPlayerMessage("검을 발견했다");
```

---

## 🔍 우리가 사용하는 API

### JosaHandler.cs에서 사용

```csharp
// 1. 플레이어 정보 가져오기
var player = XRLCore.Core?.Game?.Player?.Body;
if (player != null)
{
    string name = player.DisplayName;  // 플레이어 이름
}

// 2. 대화 텍스트 가로채기 (Harmony 패치)
[HarmonyPatch(typeof(ConversationNode), "GetDisplayText")]
public static void Postfix(ref string __result)
{
    // __result = 원본 대화 텍스트
    // 조사 처리 후 다시 __result에 저장
    __result = KoreanJosaHandler.Process(__result);
}

// 3. 메시지 가로채기
[HarmonyPatch(typeof(MessageQueue), "AddPlayerMessage")]
public static void Prefix(ref string Message)
{
    // Message = 원본 메시지
    // 조사 처리 후 다시 Message에 저장
    Message = KoreanJosaHandler.Process(Message);
}
```

---

## 🛠️ API 찾는 방법

### 1. 게임 DLL 디컴파일
```bash
# 게임 설치 폴더
~/Library/Application Support/Steam/steamapps/common/Caves of Qud/

# 주요 DLL
CoQ_Data/Managed/Assembly-CSharp.dll  # 게임 코드
```

### 2. 디컴파일 도구
- **ILSpy** (추천)
- **dnSpy**
- **dotPeek**

### 3. 사용 예시
```
1. ILSpy로 Assembly-CSharp.dll 열기
2. XRL 네임스페이스 찾기
3. GameObject 클래스 찾기
4. GetDisplayName() 메서드 확인
```

---

## 📝 실전 예시

### 예시 1: 아이템 이름 가져오기
```csharp
// 게임에서 아이템 객체
GameObject sword = ...;

// API 사용
string name = sword.DisplayName;  // "검"
string fullName = sword.GetDisplayName();  // "빛나는 검"

// 조사 추가
string result = name + KoreanJosaHandler.Choose(name, "을/를");
// result = "검을"
```

### 예시 2: 플레이어 이름 가져오기
```csharp
// API 사용
var player = XRLCore.Core?.Game?.Player?.Body;
if (player != null)
{
    string playerName = player.DisplayName;
    
    // 조사 추가
    string message = playerName + KoreanJosaHandler.Choose(playerName, "이/가") + " 검을 발견했다";
    
    // 메시지 표시
    MessageQueue.AddPlayerMessage(message);
}
```

### 예시 3: 대화 텍스트 처리
```csharp
// Harmony가 자동으로 호출
[HarmonyPatch(typeof(ConversationNode), "GetDisplayText")]
public static void Postfix(ref string __result)
{
    // __result = "당신은 검<josa_eul_reul> 발견했다"
    
    // API 사용: 텍스트 처리
    __result = KoreanJosaHandler.Process(__result);
    
    // __result = "당신은 검을 발견했다"
}
```

---

## 🎯 왜 API가 필요한가?

### 문제: 변수 값을 알 수 없음
```xml
<!-- XML 파일 -->
<text>당신은 <item.name><josa_eul_reul> 발견했다</text>
```

**질문:** `<item.name>`이 뭔지 어떻게 알아?

**답:** 게임 API로 가져온다!

```csharp
// 게임 실행 중
GameObject item = GetCurrentItem();  // 게임 API
string itemName = item.DisplayName;  // "검" 또는 "사과"

// 조사 선택
string josa = KoreanJosaHandler.Choose(itemName, "eul_reul");
// itemName이 "검" → josa = "을"
// itemName이 "사과" → josa = "를"
```

---

## 🔧 현재 구현 상태

### ✅ 구현됨
- 고정 텍스트 조사 처리
- Harmony 패치 (대화, 메시지)
- 기본 API 연동 (플레이어)

### ⏳ 구현 필요 (추후)
- 동적 변수 처리 (`<item.name>`)
- 컨텍스트 기반 변수 해석
- 더 많은 게임 객체 지원

---

## 📖 참고 자료

### 공식 문서
- Caves of Qud Modding Guide
- Technical Guidebook

### 커뮤니티
- Discord: Caves of Qud 모딩 채널
- Reddit: r/cavesofqud

### 도구
- ILSpy: https://github.com/icsharpcode/ILSpy
- dnSpy: https://github.com/dnSpy/dnSpy

---

## 💡 핵심 정리

1. **게임 API = 게임 기능에 접근하는 C# 코드**
2. **XRL 네임스페이스가 주요 API**
3. **Harmony로 게임 함수를 가로채서 조사 처리**
4. **현재는 고정 텍스트만 지원, 변수는 추후 구현**

---

**작성일:** 2026-01-13 10:14  
**다음:** 첫 번째 테스트 (Quests.xml)
