# 하드코딩 → 변수화 실전 가이드

## 🎯 목표: "Crystalism" 변수화하기

### 현재 상황 (하드코딩)

```csharp
// Data_QudKRContent/Scripts/01_Data/SomeFile.cs
public static Dictionary<string, string> Translations = new Dictionary<string, string>()
{
    { "Crystalism", "크리스탈리즘" },  // ← 하드코딩
    { "Crystalism Follower", "크리스탈리즘 추종자" },
    { "The Crystalism faith", "크리스탈리즘 신앙" }
};
```

**문제점:**
- "크리스탈리즘"을 "수정교"로 바꾸고 싶으면?
- 모든 파일에서 일일이 수정해야 함

---

## ✅ 해결책: 3단계 변수화

### 1단계: glossary.json에 추가

```json
{
  "factions": {
    "mechanimists": "메카니카신자",
    "barathrumites": "바라스럼추종자",
    "crystalism": "크리스탈리즘"  ← 추가!
  }
}
```

### 2단계: 코드에서 사용

**변경 전:**
```csharp
public static Dictionary<string, string> Translations = new Dictionary<string, string>()
{
    { "Crystalism", "크리스탈리즘" },  // ← 하드코딩
};
```

**변경 후:**
```csharp
using QudKRTranslation.Core;  // ← GlossaryLoader 사용

public static Dictionary<string, string> Translations
{
    get  // ← property로 변경 (중요!)
    {
        return new Dictionary<string, string>()
        {
            { "Crystalism", GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘") },  // ← JSON에서 로드
        };
    }
}
```

### 3단계: 용어 변경 (초간단!)

```json
// glossary.json만 수정
{
  "factions": {
    "crystalism": "수정교"  ← 여기만 바꾸면 끝!
  }
}
```

→ 게임 재시작 → 모든 "크리스탈리즘"이 "수정교"로 변경!

---

## 🔍 상세 설명

### GlossaryLoader.GetTerm() 사용법

```csharp
GlossaryLoader.GetTerm(category, key, fallback)
```

**파라미터:**
- `category`: JSON의 카테고리 (예: "factions", "weapons", "ui")
- `key`: 용어 키 (예: "crystalism", "shortBow")
- `fallback`: JSON에 없을 때 사용할 기본값 (예: "크리스탈리즘")

**예시:**
```csharp
// JSON: {"factions": {"crystalism": "수정교"}}
GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘")
// → "수정교" 반환

// JSON에 없으면
GlossaryLoader.GetTerm("factions", "unknown", "기본값")
// → "기본값" 반환
```

---

## 📋 실전 예시: 전체 파일 변환

### 변경 전 (하드코딩)

```csharp
// Data_QudKRContent/Scripts/01_Data/Factions.cs
using System.Collections.Generic;

namespace QudKRTranslation.Data
{
    public static class FactionsData
    {
        public static Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            { "Crystalism", "크리스탈리즘" },
            { "Crystalism Follower", "크리스탈리즘 추종자" },
            { "Mechanimists", "메카니카신자" },
            { "Barathrumites", "바라스럼추종자" }
        };
    }
}
```

### 변경 후 (변수화)

```csharp
// Data_QudKRContent/Scripts/01_Data/Factions.cs
using System.Collections.Generic;
using QudKRTranslation.Core;  // ← 추가

namespace QudKRTranslation.Data
{
    public static class FactionsData
    {
        public static Dictionary<string, string> Translations
        {
            get  // ← property로 변경
            {
                // 용어집 로드
                GlossaryLoader.LoadGlossary();
                
                return new Dictionary<string, string>()
                {
                    // JSON에서 로드
                    { "Crystalism", GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘") },
                    { "Crystalism Follower", GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘") + " 추종자" },
                    { "Mechanimists", GlossaryLoader.GetTerm("factions", "mechanimists", "메카니카신자") },
                    { "Barathrumites", GlossaryLoader.GetTerm("factions", "barathrumites", "바라스럼추종자") }
                };
            }
        }
    }
}
```

### glossary.json

```json
{
  "factions": {
    "crystalism": "크리스탈리즘",
    "mechanimists": "메카니카신자",
    "barathrumites": "바라스럼추종자"
  }
}
```

---

## 🎯 핵심 포인트

### 1. `= new Dictionary` → `get { return new Dictionary }`

**왜?** 
- 매번 접근할 때마다 JSON에서 최신 값을 로드하기 위해
- Property로 만들어야 동적 로딩 가능

### 2. `using QudKRTranslation.Core;` 추가

**왜?**
- `GlossaryLoader` 클래스를 사용하기 위해

### 3. `GlossaryLoader.LoadGlossary();` 호출

**왜?**
- JSON 파일을 메모리에 로드 (최초 1회만)

---

## 🔄 복합 용어 처리

### 예시 1: 용어 조합

```csharp
// "크리스탈리즘 추종자"
{ "Crystalism Follower", GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘") + " 추종자" }
```

### 예시 2: 조사 처리

```csharp
// "크리스탈리즘{을/를}"
{ "Crystalism Object", GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘") + "{을/를}" }
```

### 예시 3: 문장 내 사용

```csharp
// "크리스탈리즘 신앙을 믿습니다"
{ "Crystalism Faith", GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘") + " 신앙" }
```

---

## 📊 비교: 용어 변경 시

### 하드코딩 방식

```csharp
// 파일 1
{ "Crystalism", "크리스탈리즘" },  // ← 수정
{ "Crystalism Follower", "크리스탈리즘 추종자" },  // ← 수정

// 파일 2
{ "The Crystalism", "크리스탈리즘" },  // ← 수정

// 파일 3
{ "Crystalism Faith", "크리스탈리즘 신앙" },  // ← 수정
```

**문제:** 모든 파일을 찾아서 수정해야 함 (누락 위험)

### 변수화 방식

```json
// glossary.json만 수정
{
  "factions": {
    "crystalism": "수정교"  ← 여기 한 곳만!
  }
}
```

**장점:** 한 곳만 수정하면 모든 곳에 자동 적용!

---

## 🚀 빠른 시작 체크리스트

- [ ] 1. `glossary.json`에 용어 추가
- [ ] 2. `.cs` 파일에 `using QudKRTranslation.Core;` 추가
- [ ] 3. `Dictionary` → `Dictionary { get { return ... } }` 변경
- [ ] 4. 하드코딩 값 → `GlossaryLoader.GetTerm()` 변경
- [ ] 5. 게임 재시작 및 테스트

---

## ⚠️ 주의사항

1. **Property 변환 필수**
   ```csharp
   // ❌ 안 됨
   public static Dictionary<string, string> Translations = new Dictionary<string, string>() { ... };
   
   // ✅ 됨
   public static Dictionary<string, string> Translations { get { return new Dictionary<string, string>() { ... }; } }
   ```

2. **Fallback 값 제공**
   - JSON 로드 실패 시 기본값 사용
   - 항상 세 번째 파라미터에 기본값 지정

3. **카테고리 일관성**
   - JSON 구조와 코드의 카테고리명 일치 필요
   - 대소문자 구분 없음 (JSON은 소문자 권장)

---

## 💡 실전 팁

### 팁 1: 점진적 마이그레이션

```csharp
// 일부만 변수화
{
    { "Crystalism", GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘") },  // ← JSON
    { "Some Other Term", "다른 용어" }  // ← 하드코딩 유지
}
```

### 팁 2: 자주 바뀌는 것만 변수화

- ✅ 변수화: 세력명, 무기명, 능력치명
- ❌ 하드코딩 유지: UI 버튼 텍스트 ("확인", "취소" 등)

### 팁 3: 테스트 방법

```csharp
// 디버그 로그 추가
string term = GlossaryLoader.GetTerm("factions", "crystalism", "크리스탈리즘");
Debug.Log($"Loaded term: {term}");
```

---

**요약:** `glossary.json`에 추가 → 코드에서 `GlossaryLoader.GetTerm()` 사용 → JSON만 수정하면 모든 곳에 적용! 🎉
