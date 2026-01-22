---
title: 작업 흐름 가이드
category: reference
audience: [developer, ai-agent]
update_frequency: rare
prerequisites: [00_PRINCIPLES.md]
last_updated: 2026-01-18
estimated_read_time: 8min
ai_summary: |
  새 화면 번역, 용어집 추가, 버그 수정 등 주요 작업 유형별 표준 절차.
  조사(Investigation) → 계획(Planning) → 실행(Execution) → 검증(Verification) → 기록(Documentation) 사이클.
---

# 작업 흐름 가이드

> 이 문서는 번역 작업의 표준 절차를 설명합니다.

---

## 기본 작업 사이클

```
1. 조사 (Investigation)
   ↓ 실제 상황 파악
2. 계획 (Planning)
   ↓ 구체적 실행 계획 수립
3. 실행 (Execution)
   ↓ 계획대로 구현
4. 검증 (Verification)
   ↓ 결과 확인 및 테스트
5. 기록 (Documentation)
   → 완료 내용 문서화
```

---

## 작업 유형별 가이드

### 유형 A: 새 화면 번역

**사전 조사**:
```bash
# 1. 클래스 위치 확인 (XRL.UI + Qud.UI 양쪽!)
grep -r "class ScreenName" Assets/core_source/

# 2. 메서드 시그니처 확인
grep -A 5 "public void Show" Assets/core_source/_GameSource/*/ScreenName.cs

# 3. 텍스트 출처 확인
grep -ri "버튼 텍스트" Assets/core_source/ Assets/StreamingAssets/Base/

# 4. 기존 패치 확인
grep -r "ScreenName" Scripts/02_Patches/
```

**패치 코드 템플릿**:
```csharp
/*
 * 파일명: 02_10_XX_NewScreen.cs
 * 분류: [UI Patch] 새 화면 번역
 * 역할: {화면명} UI 텍스트 번역
 */

using HarmonyLib;
using QudKRTranslation.Core;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(TargetClass))]
    public static class Patch_NewScreen
    {
        private static bool _scopePushed = false;

        [HarmonyPatch(nameof(TargetClass.Show))]
        [HarmonyPrefix]
        static void Show_Prefix()
        {
            if (!_scopePushed)
            {
                var dict = LocalizationManager.GetCategory("category_name");
                if (dict != null)
                {
                    ScopeManager.PushScope(dict);
                    _scopePushed = true;
                }
            }
        }

        [HarmonyPatch(nameof(TargetClass.Hide))]
        [HarmonyPostfix]
        static void Hide_Postfix()
        {
            if (_scopePushed)
            {
                ScopeManager.PopScope();
                _scopePushed = false;
            }
        }
    }
}
```

**검증**:
```bash
python3 tools/project_tool.py
./tools/deploy-mods.sh
# 게임 실행 후 해당 화면 확인
```

---

### 유형 B: 용어집 항목 추가/수정

**Layer 1 (CHARGEN/, GAMEPLAY/, UI/ 폴더)**:

```bash
# 1. 올바른 카테고리 확인
grep -r "검색할 영문" LOCALIZATION/**/*.json

# 2. JSON 수정
# {
#   "category": {
#     "English Key": "한글 값"
#   }
# }

# 3. 검증
python3 tools/project_tool.py
```

**Layer 2 (구조화된 데이터)**:

```bash
# 1. C# 소스에서 정확한 텍스트 확인
find Assets/core_source -name "MutationName.cs"
grep -A 10 "GetDescription" [파일경로]
grep -A 10 "GetLevelText" [파일경로]

# 2. JSON 생성 (스키마 준수)
# LOCALIZATION/MUTATIONS/Category/MutationName.json

# 3. 검증
python3 tools/project_tool.py
```

---

### 유형 C: 변이 번역 (수동 작업 필수)

> ⚠️ **자동화 스크립트 사용 금지!** 구조가 복잡하여 수동 검증 필수.

**작업 절차**:

1. **C# 파일 찾기**:
   ```bash
   find Assets/core_source -name "Stinger*.cs"
   ```

2. **GetDescription() 확인**:
   ```bash
   grep -A 5 "GetDescription" [파일경로]
   ```

3. **GetLevelText() 확인** (있는 경우):
   ```bash
   grep -A 10 "GetLevelText" [파일경로]
   ```

4. **Variant 확인** (Properties 클래스):
   ```bash
   # Stinger → StingerPoisonProperties, StingerParalyzingProperties 등
   grep -r "StingerProperties" Assets/core_source/
   ```

5. **JSON 작성**:
   ```json
   {
     "names": {
       "Stinger (Poisoning Venom)": "독침 (독성 맹독)"
     },
     "description": "You bear a tail with a stinger...",
     "description_ko": "적에게 독성 맹독을 전달하는...",
     "leveltext": ["20% chance...", "Stinger is a long blade..."],
     "leveltext_ko": ["근접 공격 시 20%...", "독침은 긴 칼날..."]
   }
   ```

6. **게임 내 확인**:
   - 캐릭터 생성 → 변이 선택 → 설명 확인
   - 줄바꿈, 색상 태그 정상 표시 확인

---

### 유형 D: 버그 수정

**디버그 절차**:

1. **증상 기록**:
   ```markdown
   ## 증상
   - 화면: [어디서]
   - 현상: [무엇이]
   - 기대: [어떻게 되어야]
   ```

2. **로그 확인**:
   ```bash
   tail -f ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | grep "Qud-KR"
   ```

3. **디버그 로그 추가** (필요시):
   ```csharp
   UnityEngine.Debug.Log($"[Qud-KR DEBUG] value: {value}");
   ```

4. **원인 분석 및 수정**

5. **ERROR_LOG.md 기록**:
   ```markdown
   ## ERR-XXX: [이슈 제목]
   
   ### 기본 정보
   | 항목 | 내용 |
   |------|------|
   | 상태 | 🟢 RESOLVED |
   | 심각도 | 🟡 Medium |
   
   ### 증상
   ...
   
   ### 원인
   ...
   
   ### ✅ 최종 해결
   ...
   ```

---

## 커밋 규칙

### 접두어

| 접두어 | 용도 |
|--------|------|
| `feat:` | 새로운 번역/기능 추가 |
| `fix:` | 버그 수정 |
| `refactor:` | 코드/구조 리팩토링 |
| `docs:` | 문서 업데이트 |
| `chore:` | 빌드, 도구, 설정 변경 |
| `style:` | 스타일 가이드 적용 |

### 메시지 형식

```
feat: 인벤토리 필터바 번역 추가

- Scripts/02_Patches/10_UI/02_10_07_Inventory.cs 수정
- "*All" → "전체" 변환
- 카테고리별 필터 번역 (Weapons→무기, Armor→방어구 등)

Resolves: P1-01
```

---

## 검증 체크리스트

### 코드 품질
- [ ] `python3 tools/project_tool.py` 검증 통과
- [ ] 기존 기능 정상 동작 유지
- [ ] 스코프 Push/Pop 균형 확인
- [ ] 예외 처리 완료

### 문서화
- [ ] `02_TODO.md` 상태 업데이트
- [ ] 에러 발생 시 `04_ERROR_LOG.md` 기록
- [ ] `CONTEXT.yaml` 업데이트 (significant changes)

### 테스트
- [ ] 게임 내 번역 표시 확인
- [ ] 관련 기능 정상 동작 확인
- [ ] 로그에 에러 없음 확인

---

## 작업 완료 기준

작업이 "완료"로 간주되려면:

```
☑ project_tool.py 검증 통과
☑ 게임 내 테스트 완료
☑ 02_TODO.md 상태 업데이트 ([/] → [x])
☑ 에러 발생 시 04_ERROR_LOG.md 기록
☑ 커밋 및 푸시 완료
```

---

## 관련 문서

- [00_PRINCIPLES.md](00_PRINCIPLES.md) - 개발 대원칙
- [05_ARCHITECTURE.md](05_ARCHITECTURE.md) - 시스템 구조
- [LOCALIZATION/README.md](../LOCALIZATION/README.md) - 데이터 구조
