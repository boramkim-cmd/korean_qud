# ERR-012: 속성 선택 툴팁 한글 폰트 미표시

> **날짜**: 2026-01-20  
> **상태**: ✅ 해결됨  
> **심각도**: High

## 증상
캐릭터 생성 > 속성 선택 화면에서 계급 보너스 툴팁이 `  +3` (공백 + 숫자)로 표시됨.
번역 로그에서는 `화로의자녀 계급 +3`가 정상 출력됨.

## 근본 원인 (3가지 복합)
1. `deploy-mods.sh`가 `StreamingAssets/` 폴더 미배포
2. `QudKREngine.cs` 폰트 검색 경로가 모드 폴더 미포함
3. `ApplyTooltipFont()`가 실제 팝업 TMP 컴포넌트에 폰트 미적용

## 핵심 실수: ModelShark 구조 미분석
- `TooltipTrigger` ≠ 툴팁 팝업 (별도 GameObject)
- `Tooltip` 클래스는 `Component`가 아닌 일반 C# 클래스
- `as Component` 캐스팅이 조용히 `null` 반환

## 해결책
```csharp
var tooltip = tooltipTrigger.Tooltip;  // public property
var tmps = tooltip.GameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
foreach (var tmp in tmps) { ... }
```

## 교훈
1. **서드파티 라이브러리 소스 분석 필수** (`Assets/core_source/ThirdParty/`)
2. **리플렉션 전 public API 확인**
3. **캐스팅 후 null 체크 로그 필수**
4. **트리거와 팝업 UI는 별도 계층**

## 관련 파일
- [deploy-mods.sh](file:///Users/ben/Desktop/qud_korean/tools/deploy-mods.sh)
- [QudKREngine.cs](file:///Users/ben/Desktop/qud_korean/Scripts/00_Core/00_00_99_QudKREngine.cs)
- [CharacterCreation.cs](file:///Users/ben/Desktop/qud_korean/Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs)
