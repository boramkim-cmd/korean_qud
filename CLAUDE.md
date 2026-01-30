# Claude Code 프로젝트 규칙

> **qud_korean** - Caves of Qud 한글화 프로젝트 | v3.1 (2026-01-30)

## 다음 세션 할 일

> 이 작업 완료 후 이 섹션을 비우고 커밋할 것

### 1. 게임 테스트 (필수)
배포 완료 상태. 게임 실행 후:
- `kr:stats` → Mode: bundle, 항목 수 3445개 확인
- `kr:perf` → 스킵율 50%+, 폰트 캐시 히트 확인
- 팩션/지역생물/위시커맨드/음식 한글 확인
- 수정된 번역 확인: "경비견", "투망", "부장품", "의료실", "별 야자수" 등

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
