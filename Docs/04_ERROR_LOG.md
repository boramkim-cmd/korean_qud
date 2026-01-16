# Caves of Qud 한글화 프로젝트 - 에러/이슈 로그

> **버전**: 1.1 | **최종 업데이트**: 2026-01-16

> [!WARNING]
> **AI 에이전트**: 작업 전 이 문서의 미해결 이슈(🔴 OPEN)를 확인하세요!
> 먼저 `00_PRINCIPLES.md`를 읽고, 에러 발생 시 이 문서에 기록하세요.

개발 중 발생한 에러와 이슈를 기록하고 해결 방법을 문서화합니다.
**동일한 이슈가 반복되지 않도록** 모든 에러는 이 문서에 기록되어야 합니다.

---

## 📋 문서 시스템 연동

### 연동 구조
```
01_DEVELOPMENT_GUIDE.md (불변 참조)
          ↓
02_TODO.md (동적 추적)
          ↓
03_CHANGELOG.md (완료 기록)
          ↓
04_ERROR_LOG.md (이 문서 - 에러/이슈 추적)
```

### 기록 원칙
1. **모든 에러 기록**: 사소한 에러도 기록
2. **해결 과정 기록**: 시도한 모든 방법 기록
3. **최종 해결책 강조**: 성공한 방법 명확히 표시
4. **예방 가이드 작성**: 동일 이슈 방지 방법 문서화

---

## 📊 이슈 분류

### 상태 표기
| 상태 | 의미 |
|------|------|
| 🔴 **OPEN** | 미해결 - 작업 필요 |
| 🟡 **IN PROGRESS** | 해결 중 |
| 🟢 **RESOLVED** | 해결 완료 |
| ⚪ **WONTFIX** | 해결 불가 또는 수정 불필요 |

### 심각도 분류
| 심각도 | 의미 | 예시 |
|--------|------|------|
| 🔴 **Critical** | 게임 크래시 또는 모드 로드 실패 | Harmony 패치 오류 |
| 🟠 **High** | 주요 기능 동작 안함 | 번역 표시 안됨 |
| 🟡 **Medium** | 일부 기능 이상 | 특정 화면 번역 누락 |
| 🟢 **Low** | 사소한 문제 | 오타, 스타일 불일치 |

---

# 🔴 미해결 이슈 (Open Issues)

---

## ERR-001: 인벤토리 "*All" 필터 미번역

### 기본 정보
| 항목 | 내용 |
|------|------|
| **상태** | 🔴 OPEN |
| **심각도** | 🟠 High |
| **발견일** | 2026-01-15 |
| **관련 TODO** | P1-01 |

### 증상
인벤토리 화면 상단의 필터 바에서 "*All" 텍스트가 영어로 표시됨.
다른 카테고리 (Weapons, Armor, Tools 등)도 영어로 표시.

### 원인 분석
- 필터 바 컴포넌트가 일반적인 TMP_Text 훅으로 접근되지 않음
- `InventoryAndEquipmentStatusScreen` 내부에서 별도로 필터 UI 생성
- FilterBar 컴포넌트에 직접 접근하는 패치 필요

### 시도한 해결 방법
| # | 방법 | 결과 |
|---|------|------|
| 1 | UITextSkin 패치로 시도 | ❌ 실패 - 필터 바에 적용 안됨 |
| 2 | TMP_Text.text setter 훅 | ❌ 실패 - 필터 텍스트 캡처 안됨 |

### 다음 시도 예정
- [ ] `UpdateViewFromData` 내부 Traverse로 FilterBar 접근
- [ ] 필터 컴포넌트 타입 분석

### 관련 파일
```
Scripts/02_Patches/UI/02_10_07_Inventory.cs
Assets/core_source/_GameSource/Qud.UI/InventoryAndEquipmentStatusScreen.cs
```

---

## ERR-002: 조사 처리 시 색상 태그 내부 미인식

### 기본 정보
| 항목 | 내용 |
|------|------|
| **상태** | 🔴 OPEN |
| **심각도** | 🟡 Medium |
| **발견일** | 2026-01-15 |
| **관련 TODO** | P1-03 |

### 증상
```
입력: "{{w|검}}{을/를}"
기대: "{{w|검}}을"
실제: "{{w|검}}{을/를}" (변환 안됨)
```

조사 플레이스홀더가 색상 태그 바로 뒤에 올 때 받침 인식이 안됨.

### 원인 분석
- `ResolveJosa()` 함수가 조사 앞 문자를 확인할 때 `}}` 문자를 보게 됨
- `}}`는 한글이 아니므로 받침 판별 불가
- 색상 태그 내부 텍스트를 추출하는 로직 없음

### 시도한 해결 방법
*아직 시도한 방법 없음*

### 예상 해결 방법
```csharp
// 색상 태그 내부 마지막 한글 추출
private static char? GetLastHangul(string text)
{
    // 패턴: {{X|내용}} 에서 '내용'의 마지막 한글
    var match = Regex.Match(text, @"\{\{[a-zA-Z]\|([^}]+)\}\}$");
    if (match.Success)
    {
        string inner = match.Groups[1].Value;
        for (int i = inner.Length - 1; i >= 0; i--)
        {
            if (IsHangul(inner[i])) return inner[i];
        }
    }
    return null;
}
```

### 관련 파일
```
Scripts/00_Core/00_99_QudKREngine.cs
```

---

## ERR-003: Options 빈 값 약 50개 존재

### 기본 정보
| 항목 | 내용 |
|------|------|
| **상태** | 🔴 OPEN |
| **심각도** | 🟡 Medium |
| **발견일** | 2026-01-15 |
| **관련 TODO** | P1-02 |

### 증상
`glossary_options.json`에 약 50개의 빈 값 존재.
설정 화면에서 해당 옵션들이 영어로 표시됨.

### 원인 분석
- 옵션 추출 시 일부 항목 누락
- 특히 Hidden 옵션이나 조건부 표시 옵션

### 관련 파일
```
LOCALIZATION/glossary_options.json
Assets/StreamingAssets/Base/Options.xml
```

---

## ERR-004: 변이 설명 5개 누락

### 기본 정보
| 항목 | 내용 |
|------|------|
| **상태** | 🔴 OPEN |
| **심각도** | 🟢 Low |
| **발견일** | 2026-01-15 |
| **관련 TODO** | P1-04 |

### 증상
`glossary_mutations.json`에 5개의 변이 설명이 빈 값으로 존재.
현재 완성도: 96.5%

### 관련 파일
```
LOCALIZATION/glossary_mutations.json
```

---

# 🟢 해결된 이슈 (Resolved Issues)

---

## ERR-R001: ScreenBuffer 네임스페이스 오류 [예시]

### 기본 정보
| 항목 | 내용 |
|------|------|
| **상태** | 🟢 RESOLVED |
| **심각도** | 🔴 Critical |
| **발견일** | - |
| **해결일** | - |

### 증상
```
error CS0246: The type or namespace name 'ScreenBuffer' could not be found
```

### 원인
`ScreenBuffer`를 `XRL.UI` 네임스페이스에서 찾으려고 시도.
실제로는 `ConsoleLib.Console` 네임스페이스에 위치.

### ❌ 실패한 시도
```csharp
using XRL.UI;
[HarmonyPatch(typeof(ScreenBuffer))]  // 컴파일 에러
```

### ✅ 최종 해결
```csharp
using ConsoleLib.Console;  // 올바른 네임스페이스
[HarmonyPatch(typeof(ScreenBuffer))]
```

### 예방 가이드
> [!TIP]
> 클래스 패치 전 **반드시** 네임스페이스 확인:
> ```bash
> grep -r "class ScreenBuffer" Assets/core_source/
> ```
> 결과: `ConsoleLib.Console/ScreenBuffer.cs`

---

## ERR-R002: Undefined target method 에러 [예시]

### 기본 정보
| 항목 | 내용 |
|------|------|
| **상태** | 🟢 RESOLVED |
| **심각도** | 🔴 Critical |
| **발견일** | - |
| **해결일** | - |

### 증상
```
Harmony: Error patching 'Show' - undefined target method
```
모드 로드 시 Harmony 패치 실패.

### 원인
`Show` 메서드가 여러 오버로드를 가지고 있거나, 시그니처가 예상과 다름.

### ❌ 실패한 시도
```csharp
[HarmonyPatch(typeof(OptionsScreen), "Show")]  // 오버로드 미지정
```

### ✅ 최종 해결
```csharp
[HarmonyPatch(typeof(OptionsScreen), "Show", new Type[] { })]  // 파라미터 없는 버전 명시
```

또는 `TargetMethod` 사용:
```csharp
static MethodBase TargetMethod()
{
    return AccessTools.Method(typeof(OptionsScreen), "Show", new Type[] { });
}
```

### 예방 가이드
> [!TIP]
> 메서드 패치 전 **반드시** 시그니처 확인:
> ```bash
> grep "void Show" Assets/core_source/_GameSource/Qud.UI/OptionsScreen.cs
> ```
> 오버로드가 있으면 파라미터 타입 명시

---

## ERR-R003: JSON 파싱 오류 [예시]

### 기본 정보
| 항목 | 내용 |
|------|------|
| **상태** | 🟢 RESOLVED |
| **심각도** | 🟠 High |
| **발견일** | - |
| **해결일** | - |

### 증상
```
LocalizationManager: Failed to parse glossary_ui.json
```
용어집 로드 실패, 번역이 전혀 작동하지 않음.

### 원인
JSON 파일에서 마지막 항목 뒤에 쉼표(trailing comma) 존재.

### ❌ 실패한 시도
```json
{
  "ui": {
    "newGame": "새 게임",
    "continue": "계속하기",  // ← 마지막 항목 뒤 쉼표
  }
}
```

### ✅ 최종 해결
```json
{
  "ui": {
    "newGame": "새 게임",
    "continue": "계속하기"   // ← 쉼표 제거
  }
}
```

### 예방 가이드
> [!TIP]
> JSON 수정 후 **반드시** 검증:
> ```bash
> python3 tools/project_tool.py
> ```
> 또는 VS Code JSON 확장 사용

---

# 📚 자주 발생하는 이슈 (FAQ)

---

## FAQ-001: 번역이 게임에 표시되지 않음

### 체크리스트
1. **모드 활성화 확인**
   - 메인 메뉴 → Mods → Korean Localization 체크

2. **모드 배포 확인**
   ```bash
   ./tools/deploy-mods.sh
   ```

3. **JSON 문법 오류 확인**
   ```bash
   python3 tools/project_tool.py
   ```

4. **게임 재시작**
   - 모드 활성화 후 게임 완전 재시작 필요

5. **로그 확인**
   ```bash
   tail -100 ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | grep -i "error\|qud-kr"
   ```

---

## FAQ-002: Harmony 패치 실패

### 체크리스트
1. **네임스페이스 확인**
   ```bash
   grep -r "class TargetClass" Assets/core_source/
   ```

2. **메서드 시그니처 확인**
   ```bash
   grep "void MethodName" Assets/core_source/Path/To/File.cs
   ```

3. **오버로드 확인**
   - 동일 이름의 메서드가 여러 개 있는지 확인
   - `new Type[] { }` 로 파라미터 명시

4. **XRL.UI vs Qud.UI 양쪽 확인**
   - 많은 화면이 이중 구현되어 있음
   - 실제 사용되는 클래스 확인 필요

---

## FAQ-003: 조사가 올바르게 처리되지 않음

### 체크리스트
1. **플레이스홀더 형식 확인**
   ```
   ✅ 올바름: {이/가}, {을/를}, {은/는}
   ❌ 잘못됨: (이/가), [이/가], 이/가
   ```

2. **중괄호 사용 확인**
   - 조사 플레이스홀더는 반드시 중괄호 `{}` 사용

3. **색상 태그 뒤 조사**
   - 현재 미지원 (ERR-002 참조)
   - `{{w|검}}{을/를}` → 동작 안함

4. **QudKREngine 로드 확인**
   - 모드 로드 순서 확인

---

## FAQ-004: 특정 텍스트만 번역 안됨

### 체크리스트
1. **용어집에 키 존재 확인**
   ```bash
   grep -r "해당 텍스트" LOCALIZATION/*.json
   ```

2. **대소문자 확인**
   - TranslationEngine은 대소문자 변형 검색 지원
   - 하지만 정확한 케이스가 가장 빠름

3. **스코프 확인**
   - 해당 화면에서 올바른 스코프가 Push 되었는지 확인
   - 패치 파일에서 `PushScope` 호출 확인

4. **특수 문자 확인**
   - 앞뒤 공백, 줄바꿈 문자 등 제거

---

## FAQ-005: 게임 업데이트 후 모드 동작 안함

### 체크리스트
1. **패치 대상 메서드 변경 확인**
   ```bash
   # 게임 업데이트 후 소스 비교
   diff Assets/core_source/old/File.cs Assets/core_source/new/File.cs
   ```

2. **클래스 이름 변경 확인**
   - 일부 화면은 버전에 따라 클래스명 변경

3. **필드/프로퍼티 이름 변경 확인**
   - Traverse 접근 시 필드명 변경에 취약

4. **에러 로그 확인**
   ```bash
   tail -200 ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | grep -i "harmony\|error"
   ```

---

# 📝 에러 기록 템플릿

새 에러 발생 시 아래 템플릿 사용:

```markdown
## ERR-XXX: [에러 제목]

### 기본 정보
| 항목 | 내용 |
|------|------|
| **상태** | 🔴 OPEN |
| **심각도** | 🟠 High |
| **발견일** | YYYY-MM-DD |
| **관련 TODO** | P?-?? |

### 증상
[에러 메시지 또는 현상 상세 설명]

### 원인 분석
[왜 이 에러가 발생하는지 분석]

### 시도한 해결 방법
| # | 방법 | 결과 |
|---|------|------|
| 1 | [시도한 방법] | ❌ 실패 - [이유] |
| 2 | [시도한 방법] | ❌ 실패 - [이유] |

### ✅ 최종 해결 (해결 시)
[성공한 해결 방법 상세 설명]
[코드 예시가 있으면 포함]

### 예방 가이드 (해결 시)
> [!TIP]
> [이 에러를 미리 방지하는 방법]

### 관련 파일
```
관련 파일 경로 목록
```
```

---

*ERROR_LOG 버전 1.0 | 2026-01-16 | 신규 생성*
