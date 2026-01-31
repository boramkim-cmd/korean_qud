# ⛔ 위험 구역 (절대 금지 규칙)

> 이 규칙을 어기면 게임 크래시 또는 번역 시스템 전체 실패

---

## 1. 절대 번역 금지 필드

| 클래스 | 필드 | 이유 | 안전한 패치 위치 |
|--------|------|------|------------------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` 사용 | UI Postfix만 |
| `ChoiceWithColorIcon` | `Id` | 선택 로직 비교에 사용 | Title만 번역 |

```csharp
// ❌ 금지: 데이터 필드 직접 수정
attr.Attribute = "힘";  // 크래시!

// ✅ 허용: UI 요소만 수정
textElement.text = "힘";
```

---

## 2. Dictionary 중복 키 금지

**증상**: `TypeInitializationException` → 클래스 전체 로드 실패

```csharp
// ❌ 금지: 중복 키
{ "worn", "낡은" },     // 147줄
// ... 76줄 후 ...
{ "worn", "낡은" },     // 223줄 → ArgumentException!
```

**예방법**:
```bash
# 추가 전 반드시 실행
grep -n "추가할키" ObjectTranslator.cs
```

---

## 3. 색상 태그 중복 포함 금지

**TranslationEngine이 자동 복원하므로 JSON에 포함하면 2중 적용됨**

```json
// ❌ 금지
{"{{c|u}} text": "{{c|u}} 번역"}

// ✅ 허용
{"{{c|u}} text": "번역"}
```

---

## 4. 게임 테스트 생략 금지

```
deploy.sh 성공 ≠ 코드 정상
Python 테스트 통과 ≠ 게임 동작 보장
```

**필수 검증**:
```bash
# 1. 게임 실행
# 2. 로그 확인
grep -i "error\|exception" ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | tail -20
# 3. 인게임 테스트 (kr:stats, 해당 기능 직접 확인)
```

---

## 5. 커밋 지연 금지

```
코드 변경 → 테스트 → 정상 확인 → 즉시 커밋
                              ↑
                    "나중에 한꺼번에" 금지!
```

**이유**:
- 롤백 용이성
- 버그 도입 지점 추적
- 세션 간 작업 손실 방지

---

## 6. 용어 불일치 금지

| 영문 | 한글 (표준) | 잘못된 예 |
|------|------------|-----------|
| Toughness | 건강 | ~~지구력~~ |
| Willpower | 의지 | ~~의지력~~ |
| warden | 경비관 | ~~경비원~~ |
| armor | 갑옷 | ~~방어구~~ |

> 전체 목록: `Docs/terminology_standard.md`

---

## 7. 번들 모드 사전 로딩 누락 (세션 6에서 수정됨)

**증상**: `kr:stats`에서 Species: 0, Nouns: 0 → 파이프라인 전체 번역 실패

**원인**: `LoadBundledData()`만 호출하고 `LoadItemCommon()`/`LoadCreatureCommon()` 등 보충 로딩 누락

**수정**: 번들 로딩 후 소스 파일에서 보충 사전 로드 추가 (`JsonRepository.cs:410-416`)

**향후 주의**: 새 어휘 카테고리 추가 시 `AddToVocabularyByCategory()`에 case 추가 필요

---

## 8. 중복 Harmony 패치 금지 (세션 6에서 수정됨)

**증상**: 같은 메서드에 2개 Postfix/Prefix → 이중 실행, 예측 불가 결과

**제거된 중복**:
- `QudKREngine.MessageLogPatch` (→ `02_10_16_MessageLog.cs`에서 처리)
- `QudKREngine.DescriptionPatch` (→ `02_20_02_DescriptionPatch.cs`에서 처리)
- `02_10_15_EmbarkOverlay.cs` (→ `02_10_10_CharacterCreation.cs`에서 처리)

**규칙**: 새 Harmony 패치 추가 전 `grep -rn "HarmonyPatch.*TargetClass.*TargetMethod" Scripts/` 실행

---

## 9. 스레드 안전성

**핵심 캐시**: `TranslationContext._globalCache`와 `ObjectTranslatorV2._negativeCache`는
`ConcurrentDictionary` 사용 (세션 6에서 수정).

**규칙**: 정적 mutable 컬렉션 추가 시 `ConcurrentDictionary` 또는 `lock` 필수
