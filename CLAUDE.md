# Claude Code 프로젝트 규칙

> **qud_korean** - Caves of Qud 한글화 프로젝트 | v3.1 (2026-01-30)

## 다음 세션 할 일

> 이 작업 완료 후 이 섹션을 비우고 커밋할 것

### 1. 세계 생성 성능 최적화 (진행 중)

**현재 상태:** 배포 완료, 게임 테스트 필요
- DisplayName 파이프라인 스킵 제거 → 세계 생성 중에도 파이프라인 허용
- TMP_Text setter만 세계 생성 중 스킵 유지
- 세계 생성 시간 측정 필요: `grep "World generation ended" Player.log`

**구현 완료:**
- `ObjectTranslatorV2` 빠른 캐시 (`_fastCache`): 1836 블루프린트 프리로드
- `_knownBlueprints` HashSet: 데이터에 없는 블루프린트 즉시 스킵
- `StripSuffixFast()`: Regex 없이 접미사(수량/상태/스탯) 제거 → 빠른 캐시 히트율 향상
- 성능 카운터: FastHit/FastSkip/Pipeline/Total + 핫스팟 리포트 (5000호출마다 TOP 10)
- `WorldGenActivityIndicator`: ControlManager.Update 기반 점 애니메이션 오버레이

**측정 결과 (파이프라인 스킵 + StripSuffixFast 적용):**
- Total: 3113, FastHit: 558 (18%), FastSkip: 2168 (70%), Pipeline: 387 (12% - 스킵됨)
- 세계 생성 시간: 7.5초 (모드 꺼짐: ~10초 미만)
- 파이프라인 허용 시: 80초+ (DirectMatchHandler의 AllCreatures/AllItems 전체 순회가 O(n) 병목)
- StripSuffixFast로 80개 파이프라인→빠른 캐시 전환 성공

**현재 문제 (다음 세션 해결):**
1. **상점/인벤토리 로드 느림** — 파이프라인 387개가 화면 열 때 한꺼번에 실행
2. **번역 누락** — 파이프라인 스킵으로 세계 생성 중 번역 안 된 것들이 게임에 남음
   - 누락 예시: wooden arrow, torch, basic toolkit, cracked lens, bent metal sheet,
     copper nugget, congealed salve, canned Have-It-All, chrome revolver, fungicide grenade, canteen
3. **파이프라인 허용 시 80초+** — `DirectMatchHandler`의 AllCreatures/AllItems 전체 순회 O(n)이 병목

**해결 방향:**
- `DirectMatchHandler` O(n) 순회 → Dictionary 인덱스로 O(1) 변환
- 또는 `StripSuffixFast` 확장으로 빠른 캐시 히트율을 387→0에 가깝게
- 파이프라인 최적화 후 세계 생성 중 스킵 제거 가능

**수정된 파일:**
- `Scripts/02_Patches/10_UI/02_10_11_WorldCreation.cs` — 활성 표시기 + IsWorldGenActive 플래그
- `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs` — TMP setter 세계 생성 중 스킵
- `Scripts/02_Patches/20_Objects/V2/ObjectTranslatorV2.cs` — 빠른 캐시 + StripSuffixFast + 카운터
- `Scripts/02_Patches/20_Objects/02_20_01_DisplayNamePatch.cs` — (스킵 제거됨, 정상 동작)

**주의:** 성능 카운터/핫스팟 코드는 디버깅용. 안정화 후 제거할 것.

### 2. 동적 패턴 85개 (게임 테스트 후 판단)
- `=creatureRegionAdjective= X` (58개) — CompoundTranslator 런타임 처리 확인
- `*SultanName*` / `*creature*` (26개) — 동적 치환 확인
- 처리 안 되면 → C# 패치 필요

### 3. Phase 4: 커뮤니티
- Steam Workshop 배포, README 한글화, 기여 가이드

---

## 세션 시작

1. **위 "다음 세션 할 일"이 있으면 그것부터 실행**
2. `git log --oneline -3`

## 트리거 테이블

| 상황 | 사용할 Skill/도구 |
|------|-------------------|
| 번역 안 되는 이유 파악 | `.claude/skills/diagnose/` |
| 패턴 코드 수정 전/후 | `.claude/skills/regression/` |
| C# 핵심 파일 수정 전 | `.claude/skills/code-context/` |
| 세션 종료 | `.claude/skills/session-end/` |
| 번역 커버리지 확인 | `python3 tools/build_asset_index.py --stats` |
| 배포 | `./deploy.sh` |

## 절대 규칙

1. **AttributeDataElement.Attribute** 직접 번역 → 크래시
2. **Dictionary 중복 키** → TypeInitializationException
3. **게임 테스트 생략** → Python 통과 ≠ 실제 동작
4. **커밋 지연** 금지 — 즉시 커밋
5. **셰이더 이름 번역** 금지 — `{{shader|content}}`에서 shader는 유지
6. **커밋 + 푸시 자동화** — 작업 완료 시 사용자 확인 없이 `git commit` + `git push` 즉시 실행
7. **사용자 대면 한글 사용** — 선택지 제시, 작업 보고, 질문 등 사용자에게 보이는 텍스트는 한글로 작성 (내부 사고·코드·커밋 메시지는 영어 가능)

## 문서 레이어

| Layer | 파일 | 읽기 시점 |
|-------|------|----------|
| 0 | `.claude/session-state.md` | 항상 |
| 1 | `.claude/CONTEXT.md` | 필요시 |
| 2 | `.claude/danger-zones.md`, `.claude/anti-patterns.md` | 위험 작업 시 |
| 3 | `.claude/code-context/*.yaml` | C# 수정 시 |
| 4 | `.claude/skills/*/SKILL.md` | 트리거 테이블 참조 |
