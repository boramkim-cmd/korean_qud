# Caves of Qud 튜토리얼 한글화 공식 가이드

이 문서는 Caves of Qud 튜토리얼(Tutorial) 텍스트의 한글화 및 관리 표준을 정의합니다. 번역자와 개발자 모두를 위한 공식 워크플로우, 폴더 구조, JSON 규칙, Harmony 패치 방식, 특수 처리사항을 명확히 안내합니다.

---

## 1. 폴더 및 파일 구조

- 모든 튜토리얼 텍스트는 `LOCALIZATION/GAMEPLAY/tutorial.json` 단일 파일로 관리
- 예시:

```
LOCALIZATION/
└── GAMEPLAY/
    └── tutorial.json
```

---

## 2. JSON 구조 및 키 네이밍 규칙

- 최상위는 `tutorial` 오브젝트
- 각 튜토리얼 단계별로 StepClassName(예: `IntroTutorialStart`, `MoveToChest`) 섹션 구분
- 각 섹션 내에서 의미별 descriptive_key 사용
- Gamepad/Keyboard 분기 텍스트는 `_keyboard`, `_gamepad` suffix로 구분
- 공통 버튼/메시지는 `common` 섹션에 분리

### 예시
```json
{
  "tutorial": {
    "IntroTutorialStart": {
      "welcome_mutated_human": "Caves of Qud 튜토리얼에 오신 것을 환영합니다...",
      "pick_marsh_taur": "캐릭터 생성은 깊고 때로는 긴 과정입니다..."
    },
    "MoveToChest": {
      "main_stage_intro": "이것이 메인 게임플레이 화면입니다...",
      "walk_to_chest_keyboard": "방 끝에 상자가 있는 것 같습니다...",
      "walk_to_chest_gamepad": "방 끝에 상자가 있는 것 같습니다..."
    },
    "common": {
      "continue_button": "[~Accept] 계속"
    }
  }
}
```

---

## 3. Harmony 패치 구현 방식

- `Scripts/02_Patches/10_UI/02_10_15_Tutorial.cs`에 Harmony 패치 구현
- 대상 메서드: TutorialManager의 다음 6개 메서드
  - ShowCellPopup
  - ShowCIDPopupAsync
  - ShowIntermissionPopupAsync
  - Highlight
  - HighlightByCID
  - HighlightCell
- 각 메서드의 `text`(또는 `message`, `buttonText`) 파라미터에 대해 번역 적용
- 예시 코드:

```csharp
[HarmonyPatch(typeof(TutorialManager))]
public static class Patch_TutorialManager {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TutorialManager.ShowCellPopup))]
    static void ShowCellPopup_Prefix(ref string text) {
        if (TryTranslateTutorial(text, out var translated))
            text = translated;
    }
    // ...다른 메서드도 동일 패턴...
}
```

---

## 4. 특수 처리사항

- **~Command 플레이스홀더**: `~CmdLook`, `~Accept` 등은 번역에서 그대로 유지 (게임이 런타임에 실제 키로 자동 변환)
- **색상 태그(`{{W|text}}`)**: 번역 JSON에는 태그를 포함하지 않음 (TranslationEngine이 자동 복원)
- **멀티라인 텍스트**: 줄바꿈은 `\n`으로 표현, 전체 블록을 하나의 키로 번역
- **Gamepad/Keyboard 분기**: `_keyboard`, `_gamepad` suffix로 각각 번역

---

## 5. 번역/검수 워크플로우

1. 튜토리얼 단계별로 원문 텍스트를 `tutorial.json`에 키별로 정리
2. 번역자는 각 키에 맞춰 한글 번역 작성 (플레이스홀더, 태그 주의)
3. Harmony 패치 적용 후 실제 게임에서 표시 확인
4. 오타/문맥 오류 발견 시 즉시 수정 및 검수

---

## 6. 이 문서의 지위

- 본 가이드는 Caves of Qud 한글화 프로젝트의 **튜토리얼 번역 공식 표준**입니다.
- 모든 신규/기존 튜토리얼 번역 및 패치 작업은 본 문서 기준을 따라야 합니다.
- 변경/개정 시 반드시 전체 번역팀과 개발팀에 공지해야 합니다.

---

(최종 갱신: 2026-01-21)
