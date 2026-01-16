# 개발 가이드

> [!IMPORTANT]
> 이 문서는 Qud-KR Translation 모드에 새로운 번역을 추가하거나 수정하는 방법을 설명합니다.

---

## 목차

1. [프로젝트 구조](#프로젝트-구조)
2. [새로운 번역 추가하기](#새로운-번역-추가하기)
3. [새로운 패치 추가하기](#새로운-패치-추가하기)
4. [테스트 방법](#테스트-방법)
5. [버그 보고](#버그-보고)
6. [코드 리뷰 체크리스트](#코드-리뷰-체크리스트)

---

## 프로젝트 구조

```
QudKR_Translation/
├── Scripts/
│   ├── 00_Core/              # 핵심 시스템 (수정 금지)
│   │   ├── 00_ModEntry.cs    # Harmony 초기화
│   │   ├── 01_TranslationEngine.cs  # 번역 엔진
│   │   └── 02_ScopeManager.cs       # 범위 관리
│   │
│   ├── 01_Data/              # 번역 데이터 (여기에 추가)
│   │   ├── Common.cs         # 공통 UI 텍스트
│   │   ├── MainMenu.cs       # 메인 메뉴
│   │   ├── Options/          # 옵션 화면별
│   │   └── Gameplay/         # 게임플레이 화면별
│   │
│   ├── 02_Patches/           # Harmony 패치 (여기에 추가)
│   │   ├── Core/             # 전역 패치 (최소화)
│   │   └── UI/               # 화면별 패치
│   │
│   └── 99_Utils/             # 유틸리티
│
└── Docs/                     # 문서
    ├── BugReports/           # 버그 보고서
    └── Solutions/            # 해결책 문서
```

---

## 새로운 번역 추가하기

### 1단계: 번역할 화면 결정

예: 인벤토리 화면을 번역하고 싶다면

### 2단계: 데이터 파일 생성

**파일 위치:** `Scripts/01_Data/Gameplay/Inventory.cs`

```csharp
/*
 * 파일명: Inventory.cs
 * 분류: [Data] 인벤토리 텍스트
 * 역할: 인벤토리 화면의 텍스트를 정의합니다.
 * 작성일: YYYY-MM-DD
 */

using System.Collections.Generic;

namespace QudKRTranslation.Data
{
    public static class InventoryData
    {
        public static Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            // 탭 이름
            { "Inventory", "인벤토리" },
            { "Equipment", "장비" },
            { "Nearby", "주변" },
            
            // 정렬 옵션
            { "Sort by Name", "이름순 정렬" },
            { "Sort by Weight", "무게순 정렬" },
            { "Sort by Value", "가치순 정렬" },
            
            // 버튼
            { "Drop", "버리기" },
            { "Use", "사용" },
            { "Equip", "장착" },
            { "Unequip", "해제" },
            
            // 상태
            { "Weight", "무게" },
            { "Value", "가치" },
            { "Category", "분류" }
        };
    }
}
```

### 3단계: 패치 파일 생성

**파일 위치:** `Scripts/02_Patches/UI/Inventory_Patch.cs`

```csharp
/*
 * 파일명: Inventory_Patch.cs
 * 분류: [UI Patch] 인벤토리 패치
 * 역할: 인벤토리 화면이 열릴 때 번역 범위를 설정합니다.
 * 작성일: YYYY-MM-DD
 */

using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(Qud.UI.InventoryScreen))]
    public static class Inventory_Patch
    {
        [HarmonyPatch("Show")]
        [HarmonyPrefix]
        static void Show_Prefix()
        {
            // InventoryData를 우선, CommonData를 보조로
            ScopeManager.PushScope(
                Data.InventoryData.Translations, 
                Data.CommonData.Translations
            );
            Debug.Log("[Inventory_Patch] Scope activated");
        }
        
        [HarmonyPatch("Show")]
        [HarmonyPostfix]
        static void Show_Postfix()
        {
            ScopeManager.PopScope();
            Debug.Log("[Inventory_Patch] Scope deactivated");
        }
        
        [HarmonyPatch("Show")]
        [HarmonyFinalizer]
        static void Show_Finalizer()
        {
            if (ScopeManager.GetDepth() > 0)
            {
                Debug.LogWarning("[Inventory_Patch] Finalizer cleaning up");
                ScopeManager.PopScope();
            }
        }
    }
}
```

### 4단계: 테스트

1. 게임 실행
2. 인벤토리 화면 열기
3. 모든 텍스트가 한글로 표시되는지 확인
4. 로그 확인: `Player.log`에서 "Scope activated" 메시지 확인

---

## 새로운 패치 추가하기

### 원본 메서드 확인 (필수!)

> [!CAUTION]
> 패치를 작성하기 전에 반드시 원본 게임의 실제 메서드명을 확인하세요!

#### 방법 1: ModEntry.cs에서 확인

`00_ModEntry.cs`의 `VerifyPatchTargets()` 함수에 추가:

```csharp
InspectType("Qud.UI.YourTargetClass");
```

게임 실행 후 `Player.log`에서 사용 가능한 메서드 확인

#### 방법 2: 디컴파일러 사용

- ILSpy 또는 dnSpy로 `Assembly-CSharp.dll` 열기
- 타입 및 메서드 직접 확인

### 패치 작성 규칙

#### ✅ 좋은 예

```csharp
[HarmonyPatch(typeof(TargetClass), "MethodName")]
public static class MyPatch
{
    // 1. Prefix: Scope 설정
    [HarmonyPrefix]
    static void MethodName_Prefix()
    {
        ScopeManager.PushScope(Data.MyData.Translations);
    }
    
    // 2. Postfix: Scope 해제
    [HarmonyPostfix]
    static void MethodName_Postfix()
    {
        ScopeManager.PopScope();
    }
    
    // 3. Finalizer: 예외 시에도 Scope 해제
    [HarmonyFinalizer]
    static void MethodName_Finalizer()
    {
        if (ScopeManager.GetDepth() > 0)
        {
            ScopeManager.PopScope();
        }
    }
}
```

#### ❌ 나쁜 예

```csharp
// 전역 패치 (피할 것!)
[HarmonyPatch(typeof(ScreenBuffer), "Write")]
static void Write_Prefix(ref string text)
{
    // 모든 화면에 영향!
    text = Translate(text);
}

// Scope 해제 누락
[HarmonyPrefix]
static void Show_Prefix()
{
    ScopeManager.PushScope(...);
    // Postfix 없음 - 위험!
}
```

---

## 테스트 방법

### 1. 컴파일 테스트

```bash
# 모드 폴더로 이동
cd /Users/ben/Desktop/QudKR_Translation

# C# 파일 확인
find Scripts -name "*.cs"
```

### 2. 게임 실행 테스트

1. 모드를 게임의 Mods 폴더에 복사
2. 게임 실행
3. Mods 메뉴에서 활성화
4. 해당 화면으로 이동하여 번역 확인

### 3. 로그 확인

```bash
# 로그 파일 위치
/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log

# 에러 확인
grep -i "error\|exception" Player.log

# 패치 적용 확인
grep "패치됨:" Player.log

# Scope 활성화 확인
grep "Scope activated" Player.log
```

### 4. 체크리스트

- [ ] 모든 텍스트가 한글로 표시되는가?
- [ ] 색상 태그가 유지되는가? (예: `{{w|힘}}`)
- [ ] 체크박스가 유지되는가? (예: `[■] 옵션`)
- [ ] 다른 화면으로 이동 시 영향이 없는가?
- [ ] 로그에 에러가 없는가?

---

## 버그 보고

### 버그 발견 시

1. `Docs/BugReports/YYYY-MM-DD_[제목].md` 파일 생성
2. 다음 템플릿 사용:

```markdown
# Bug Report: [간단한 제목]

## 발생 일시
2026-01-15

## 증상
- 무엇이 잘못되었는가?
- 어떤 화면에서 발생했는가?
- 재현 방법

## 원인
- 근본 원인은 무엇인가?
- 왜 이런 일이 발생했는가?
- 관련 코드

## 해결 방법
- 어떻게 해결했는가?
- 코드 변경 사항

## 예방 조치
- 같은 문제가 다시 발생하지 않도록 하는 방법
- 다른 파일에서 주의할 점

## 관련 파일
- [파일명](file:///경로)
```

3. `KNOWN_ISSUES.md`에 요약 추가
4. `CHANGELOG.md`에 변경사항 기록

---

## 코드 리뷰 체크리스트

새로운 코드 추가 시 다음을 확인하세요:

### 데이터 파일 (01_Data/)

- [ ] 딕셔너리 이름이 다른 파일과 중복되지 않는가?
- [ ] 파일명과 클래스명이 일치하는가?
- [ ] 네임스페이스가 `QudKRTranslation.Data`인가?
- [ ] 주석이 명확한가?

### 패치 파일 (02_Patches/)

- [ ] 원본 메서드명을 확인했는가?
- [ ] Prefix/Postfix/Finalizer 3종 세트가 있는가?
- [ ] ScopeManager를 사용하는가?
- [ ] 전역 패치가 아닌가? (전역 패치는 최소화)
- [ ] 로그 메시지가 있는가?
- [ ] 네임스페이스가 `QudKRTranslation.Patches`인가?

### 일반

- [ ] 코드 중복이 없는가?
- [ ] 주석이 충분한가?
- [ ] 에러 핸들링이 있는가?
- [ ] 테스트를 했는가?

---

## 참고 자료

- [Harmony 문서](https://harmony.pardeike.net/)
- [Caves of Qud 모딩 가이드](https://wiki.cavesofqud.com/wiki/Modding)
- [bug_analysis.md](file:///Users/ben/.gemini/antigravity/brain/45cec7a6-188c-4981-95fb-a8a7f17bb8f0/bug_analysis.md) - 과거 버그 분석
- [implementation_plan.md](file:///Users/ben/.gemini/antigravity/brain/45cec7a6-188c-4981-95fb-a8a7f17bb8f0/implementation_plan.md) - 전체 계획

---

## 질문이 있으신가요?

문제가 발생하거나 질문이 있으면:

1. `KNOWN_ISSUES.md` 확인
2. `Docs/BugReports/` 폴더의 과거 보고서 확인
3. 로그 파일 확인
4. 새로운 버그 보고서 작성
