# JSON 기반 용어집 시스템

## 🎯 개요

**문제**: 용어를 변경할 때마다 여러 `.cs` 파일을 수정해야 함  
**해결**: JSON 파일에서 용어를 중앙 관리하고, 코드에서 로드

## 📁 파일 구조

```
qud_korean/
├── LOCALIZATION/
│   └── glossary.json                    ← 용어집 (JSON)
└── Data_QudKRContent/
    └── Scripts/
        ├── 00_Core/
        │   └── GlossaryLoader.cs        ← JSON 로더
        └── 01_Data/
            ├── MainMenu.cs              ← 기존 방식 (하드코딩)
            └── MainMenu_JSON_Example.cs ← JSON 방식 (예시)
```

## 📝 사용 방법

### 1단계: glossary.json 수정

```json
{
  "ui": {
    "mainMenu": {
      "newGame": "새 게임",
      "quit": "종료"
    }
  },
  "weapons": {
    "shortBow": "짧은 활"  ← 여기만 수정!
  }
}
```

### 2단계: 코드에서 사용

```csharp
using QudKRTranslation.Core;

// JSON에서 로드
string term = GlossaryLoader.GetTerm("weapons", "shortBow", "숏보우");
// → "짧은 활" 반환

// 번역 Dictionary에 추가
{ "Short Bow", GlossaryLoader.GetTerm("weapons", "shortBow", "숏보우") }
```

### 3단계: 게임 재시작

- JSON 파일만 수정하면 됨
- 코드 재컴파일 불필요
- 게임 재시작만 하면 적용

## ✅ 장점

1. **중앙 관리**: 모든 용어가 `glossary.json`에 집중
2. **쉬운 수정**: JSON만 편집하면 됨
3. **안전성**: 코드 수정 없음
4. **버전 관리**: JSON 파일 하나만 추적

## ⚠️ 단점

1. **초기 작업**: 기존 코드를 JSON 방식으로 변환 필요
2. **런타임 로드**: 게임 시작 시 JSON 파일 읽기 (성능 영향 미미)
3. **Unity 제약**: JsonUtility 사용 (중첩 구조 제한)

## 🔄 마이그레이션 예시

### 기존 방식 (하드코딩)
```csharp
public static Dictionary<string, string> Translations = new Dictionary<string, string>()
{
    { "Short Bow", "숏보우" },  // ← 코드 수정 필요
    { "Long Sword", "롱소드" }
};
```

### JSON 방식 (변수화)
```csharp
public static Dictionary<string, string> Translations
{
    get
    {
        return new Dictionary<string, string>()
        {
            { "Short Bow", GlossaryLoader.GetTerm("weapons", "shortBow", "숏보우") },  // ← JSON만 수정
            { "Long Sword", GlossaryLoader.GetTerm("weapons", "longSword", "롱소드") }
        };
    }
}
```

### glossary.json
```json
{
  "weapons": {
    "shortBow": "짧은 활",  ← 여기만 수정!
    "longSword": "장검"
  }
}
```

## 🎯 권장 사항

### 옵션 A: 점진적 마이그레이션 (권장 ⭐)
1. **자주 바뀌는 용어**만 JSON으로 이동
   - 무기 이름
   - 아이템 이름
   - 능력치 이름
2. **고정된 UI 텍스트**는 하드코딩 유지
   - "새 게임", "종료" 등

### 옵션 B: 완전 마이그레이션
- 모든 번역을 JSON으로 이동
- 작업량 많지만 장기적으로 유리

### 옵션 C: 현재 방식 유지
- 하드코딩 유지
- 간단하지만 용어 변경 시 불편

## 📊 비교표

| 항목 | 하드코딩 | JSON 방식 |
|------|---------|-----------|
| 용어 수정 | 코드 수정 필요 | JSON만 수정 |
| 재컴파일 | 필요 | 불필요 |
| 안전성 | 낮음 (코드 오류 가능) | 높음 (JSON만 수정) |
| 초기 작업 | 없음 | 마이그레이션 필요 |
| 성능 | 빠름 | 약간 느림 (로딩 시) |

## 🚀 빠른 시작

### 1. glossary.json 생성
```bash
# 이미 생성됨
/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary.json
```

### 2. GlossaryLoader.cs 추가
```bash
# 이미 생성됨
/Users/ben/Desktop/qud_korean/Data_QudKRContent/Scripts/00_Core/GlossaryLoader.cs
```

### 3. 기존 코드 수정 (예시)
```bash
# MainMenu_JSON_Example.cs 참조
/Users/ben/Desktop/qud_korean/Data_QudKRContent/Scripts/01_Data/MainMenu_JSON_Example.cs
```

### 4. 테스트
```bash
# 모드 복사
cp -r Data_QudKRContent "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/"

# 게임 실행 및 확인
```

## 💡 사용 예시

### 예시 1: 무기 이름 변경
```json
// glossary.json
{
  "weapons": {
    "shortBow": "짧은 활"  // "숏보우" → "짧은 활"
  }
}
```

게임 재시작 → 모든 "숏보우"가 "짧은 활"로 변경!

### 예시 2: 능력치 이름 변경
```json
{
  "attributes": {
    "strength": "근력"  // "힘" → "근력"
  }
}
```

### 예시 3: 세력 이름 변경
```json
{
  "factions": {
    "mechanimists": "기계교단"  // "메카니카신자" → "기계교단"
  }
}
```

## 🎯 결론

**개인 사용**이므로 **옵션 A (점진적 마이그레이션)**를 권장합니다:
- 자주 바뀌는 용어만 JSON으로
- 나머지는 하드코딩 유지
- 필요할 때 점진적으로 확장

**장점**:
- ✅ 용어 변경이 쉬워짐
- ✅ 코드 수정 불필요
- ✅ 안전성 향상

**시작 방법**:
1. `glossary.json`에 자주 바뀌는 용어 추가
2. 해당 `.cs` 파일에서 `GlossaryLoader.GetTerm()` 사용
3. 테스트 후 점진적으로 확장
